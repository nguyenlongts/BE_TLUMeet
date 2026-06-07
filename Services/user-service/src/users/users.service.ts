// src/users/users.service.ts
import { Injectable, NotFoundException, Inject } from '@nestjs/common';
import { InjectModel } from '@nestjs/mongoose';
import { Model } from 'mongoose';
import { ClientKafka } from '@nestjs/microservices';
import { User, UserDocument } from './users.schema';


@Injectable()
export class UsersService {
  constructor(
    @InjectModel(User.name) private userModel: Model<UserDocument>,
    @Inject('KAFKA_SERVICE') private kafkaClient: ClientKafka,
  ) {}

  // ── CRUD ──────────────────────────────────────────────

  async findAll() {
    return this.userModel.find();
  }

  async createUserFromEvent(data: {
    UserId: number;
    UserName: string;
    Email: string;
    RegisteredAt: string;
  }) {
    const exists = await this.userModel.findOne({ userId: data.UserId });
    if (exists) return; 

    await this.userModel.create({
      userId: data.UserId,
      name: data.UserName,
      email: data.Email.toLowerCase(),
      registeredAt: new Date(data.RegisteredAt),
    });
  }

  async getProfile(userId: number) {
    const user = await this.userModel.findOne({ userId });
    if (!user) throw new NotFoundException('User không tồn tại');
    return user;
  }

  async updateProfile(userId:number,data:{name?:string,phone?:string,address?:string,avatarUrl?:string}){

    const updatedUser=await this.userModel.findOneAndUpdate(
      {userId},
      data,
      {new:true}
    )
    if (!updatedUser) throw new NotFoundException("User không tồn tại!")
    const { name, ...rest } = data
    const updatedFields = {
      ...rest,
      ...(name && { userName: name }),
    }

    this.kafkaClient.emit('user-updated-events',{
      key: String(userId),          
      value: {
        userId,
        updatedFields,
        updatedAt: new Date().toISOString(),
      },
    })
    return updatedUser;
  }


  async remove(id: string) {
    const user = await this.userModel.findByIdAndDelete(id);
    if (!user) throw new NotFoundException('User không tồn tại');

    this.kafkaClient.emit('user.deleted', {
      userId: id,
      deletedAt: new Date(),
    });

    return { deleted: true };
  }

  async updateAvatar(userId: string, avatarUrl: string) {
    return this.userModel.findByIdAndUpdate(userId, { avatarUrl })
  }

}