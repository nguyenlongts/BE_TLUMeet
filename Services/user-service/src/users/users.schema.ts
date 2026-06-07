// export type UserDocument=User & Document
import { Document } from 'mongoose';
import { Prop, Schema, SchemaFactory } from '@nestjs/mongoose';
export type UserDocument=User & Document

@Schema({timestamps:true,collection:"users"})
export class User{
  @Prop({required:true,unique:true})
  userId: number;

  @Prop({ required: true, trim: true })
  name: string;

  @Prop({ required: true, trim: true, lowercase: true, unique: true })
  email: string;
  
  @Prop({ default: 'user', enum: ['user', 'admin'] })
  role: string;
  
  @Prop({required:true, default:true})
  isActive: boolean;

  @Prop()
  registeredAt:Date;

  @Prop()
  phone?:string;

  @Prop()
  address?: string;

  @Prop()
  avatarUrl?: string;

}

export const UserSchema=SchemaFactory.createForClass(User)