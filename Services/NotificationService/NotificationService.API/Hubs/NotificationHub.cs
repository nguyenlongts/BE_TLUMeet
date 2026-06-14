using Microsoft.AspNetCore.SignalR;
using System.Diagnostics.Metrics;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;
namespace NotificationService.API.Hubs
{
    public class NotificationHub :Hub
    {
        // JWT phát hành claim tên "email" (xem AuthService). Tùy cấu hình map claim,
        // nó có thể bị ánh xạ sang ClaimTypes.Email. Đọc cả hai để chắc chắn lấy được.
        // Chuẩn hóa lowercase để khớp group khi gửi (email lưu ở DB đều lowercase).
        private static string? GetEmail(System.Security.Claims.ClaimsPrincipal? user)
        {
            var email = user?.FindFirst("email")?.Value
                        ?? user?.FindFirst(ClaimTypes.Email)?.Value;
            return string.IsNullOrEmpty(email) ? null : email.Trim().ToLowerInvariant();
        }

        public override async Task OnConnectedAsync()
        {
            var email = GetEmail(Context.User);
            if (!string.IsNullOrEmpty(email))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, email);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var email = GetEmail(Context.User);
            if (!string.IsNullOrEmpty(email))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, email);
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Nhóm theo phòng họp để đẩy sự kiện vòng đời (bắt đầu/kết thúc) realtime,
        // thay cho việc client polling trạng thái. Dùng được cho cả khách (không JWT)
        // vì tư cách thành viên phòng được xác định bằng roomCode.
        public static string MeetingGroup(string roomCode) => $"meeting:{roomCode}";

        public Task JoinMeetingGroup(string roomCode) =>
            Groups.AddToGroupAsync(Context.ConnectionId, MeetingGroup(roomCode));

        public Task LeaveMeetingGroup(string roomCode) =>
            Groups.RemoveFromGroupAsync(Context.ConnectionId, MeetingGroup(roomCode));
    }
}
