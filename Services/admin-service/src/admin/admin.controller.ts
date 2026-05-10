import { Controller, Get, UseGuards } from '@nestjs/common';
import { EventPattern, Payload } from '@nestjs/microservices';
import { AdminService } from './admin.service';
import { JwtAdminGuard } from './jwt-admin.guard';

@Controller('api/admin')
export class AdminController {
  constructor(private adminService: AdminService) {}

  @Get('stats')
  @UseGuards(JwtAdminGuard)
  getStats() {
    return this.adminService.getStats();
  }

  @Get('users')
  @UseGuards(JwtAdminGuard)
  getUsers() {
    return this.adminService.getUsers();
  }

  @Get('meetings')
  @UseGuards(JwtAdminGuard)
  getMeetings() {
    return this.adminService.getMeetings();
  }


  @EventPattern('user-registered-events')
  handleUserRegistered(@Payload() data: {
    UserId: number;
    UserName: string;
    Email: string;
    RegisteredAt: string;
  }) {
    return this.adminService.handleUserRegistered(data);
  }

  @EventPattern('meeting-created-events')
  handleMeetingCreated(
    @Payload()
    data: {
      MeetingId: number;
      Title: string;
      RoomCode: string;
      HostEmail: string;
      CreatedAt: string;
    }) {
    return this.adminService.handleMeetingCreated(data);
  }

  @EventPattern('meeting-started-events')
  handleMeetingStarted(@Payload() data: {
    MeetingId: number;
    RoomCode: string;
    HostEmail: string;
    StartedAt: string;
  }) {
    return this.adminService.handleMeetingStarted(data);
  }

  @EventPattern('meeting-ended-events')
  handleMeetingEnded(@Payload() data: {
    MeetingId: number;
    RoomCode: string;
    HostEmail: string;
    EndedAt: string;
  }) {
    return this.adminService.handleMeetingEnded(data);
  }

  @EventPattern('meeting-deleted-events')
  handleMeetingDeleted(@Payload() data: {
    MeetingId: number;
    RoomCode: string;
    HostEmail: string;
    DeletedAt: string;
  }) {
    return this.adminService.handleMeetingDeleted(data);
  }

  @EventPattern('participant-joined-events')
  handleParticipantJoined(@Payload() data: { MeetingId: number }) {
    return this.adminService.handleParticipantJoined(data);
  }

  @EventPattern('participant-left-events')
  handleParticipantLeft(@Payload() data: { MeetingId: number }) {
    return this.adminService.handleParticipantLeft(data);
  }
}