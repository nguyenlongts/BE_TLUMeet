import { Prop, Schema, SchemaFactory } from '@nestjs/mongoose';
import { Document } from 'mongoose';

export type MeetingStatDocument = MeetingStat & Document;

@Schema({ timestamps: true, collection: 'meeting_stats' })
export class MeetingStat {
  @Prop({ required: true, unique: true })
  meetingId: number;

  @Prop({ required: true })
  title: string;

  @Prop({ required: true })
  roomCode: string;

  @Prop({ required: true })
  hostEmail: string;

  @Prop({
    default: 'scheduled',
    enum: ['scheduled', 'live', 'ended', 'waitingForHost'],
  })
  status: string;

  @Prop()
  createdAt: Date;

  @Prop()
  startedAt?: Date;

  @Prop()
  endedAt?: Date;

  @Prop()
  deletedAt?: Date;

  @Prop({ default: 0 })
  totalParticipants: number;
}

export const MeetingStatSchema = SchemaFactory.createForClass(MeetingStat);
