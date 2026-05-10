import { Injectable, HttpException } from '@nestjs/common';
import { InjectModel } from '@nestjs/mongoose';
import { Model } from 'mongoose';
import { UserStat, UserStatDocument } from '../schemas/user-stat.schema';
import {
  MeetingStat,
  MeetingStatDocument,
} from '../schemas/meeting-stat.schema';

@Injectable()
export class AdminService {
  constructor(
    @InjectModel(UserStat.name) private userStatModel: Model<UserStatDocument>,
    @InjectModel(MeetingStat.name) private meetingStatModel: Model<MeetingStatDocument>,
  ) {}

  async getStats() {
    const [totalUsers, totalMeetings, activeMeetings] = await Promise.all([
      this.userStatModel.countDocuments(),
      this.meetingStatModel.countDocuments(),
      this.meetingStatModel.countDocuments({ status: 'live' }),
    ]);

    return { totalUsers, totalMeetings, activeMeetings };
  }

  async getUsers() {
    return this.userStatModel.find().sort({ registeredAt: -1 });
  }

  async getMeetings() {
    return this.meetingStatModel.find().sort({ createdAt: -1 });
  }


  async handleUserRegistered(data: {
    UserId: number;
    UserName: string;
    Email: string;
    RegisteredAt: string;
  }) {
    const exists = await this.userStatModel.findOne({ userId: data.UserId });
    if (exists) return;

    await this.userStatModel.create({
      userId: data.UserId,
      userName: data.UserName,
      email: data.Email.toLowerCase(),
      registeredAt: new Date(data.RegisteredAt),
    });
  }


  async handleMeetingCreated(data: {
    MeetingId: number;
    Title: string;
    RoomCode: string;
    HostEmail: string;
    CreatedAt: string;
  }) {
    const exists = await this.meetingStatModel.findOne({
      meetingId: data.MeetingId,
    });
    if (exists) return;

    await this.meetingStatModel.create({
      meetingId: data.MeetingId,
      title: data.Title,
      roomCode: data.RoomCode,
      hostEmail: data.HostEmail,
      status: 'created',
      createdAt: new Date(data.CreatedAt),
    });
  }

  async handleMeetingStarted(data: {
    MeetingId: number;
    RoomCode: string;
    HostEmail: string;
    StartedAt: string;
  }) {
    await this.meetingStatModel.findOneAndUpdate(
      { meetingId: data.MeetingId },
      { status: 'started', startedAt: new Date(data.StartedAt) },
    );
  }

  async handleMeetingEnded(data: {
    MeetingId: number;
    RoomCode: string;
    HostEmail: string;
    EndedAt: string;
  }) {
    await this.meetingStatModel.findOneAndUpdate(
      { meetingId: data.MeetingId },
      { status: 'ended', endedAt: new Date(data.EndedAt) },
    );
  }

  async handleMeetingDeleted(data: {
    MeetingId: number;
    RoomCode: string;
    HostEmail: string;
    DeletedAt: string;
  }) {
    await this.meetingStatModel.findOneAndUpdate(
      { meetingId: data.MeetingId },
      { status: 'deleted', deletedAt: new Date(data.DeletedAt) },
    );
  }

  async handleParticipantJoined(data: { MeetingId: number }) {
    await this.meetingStatModel.findOneAndUpdate(
      { meetingId: data.MeetingId },
      { $inc: { totalParticipants: 1 } },
    );
  }

  async handleParticipantLeft(data: { MeetingId: number }) {
    await this.meetingStatModel.findOneAndUpdate(
      { meetingId: data.MeetingId },
      { $inc: { totalParticipants: -1 } },
    );
  }
}
