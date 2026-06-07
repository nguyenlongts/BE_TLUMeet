import { Prop, Schema, SchemaFactory } from '@nestjs/mongoose';
import { Document } from 'mongoose';

export type UserStatDocument = UserStat & Document;

@Schema({ timestamps: true, collection: 'user_stats' })
export class UserStat {
  @Prop({ required: true, unique: true })
  userId: number;

  @Prop({ required: true })
  email: string;

  @Prop({ required: true })
  userName: string;

  @Prop({ default: 'user', enum: ['user', 'admin'] })
  role: string;
  
  @Prop()
  avatarUrl: string;

  @Prop({required:true, default:true})
  isActive: boolean;

  @Prop()
  registeredAt: Date;
}

export const UserStatSchema = SchemaFactory.createForClass(UserStat);
