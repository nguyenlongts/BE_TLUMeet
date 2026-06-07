// src/users/users.controller.ts
import {
  Controller,
  Get, Patch, Delete,
  Body, Param, Headers,
  ParseIntPipe,
  UseInterceptors,
  BadRequestException,
  UploadedFile,
  Req,
  Post,
} from '@nestjs/common';
import { EventPattern, Payload } from '@nestjs/microservices';
import { UsersService } from './users.service';
import { memoryStorage } from 'multer';
import { FileInterceptor } from '@nestjs/platform-express';
import { CloudinaryService } from 'src/cloudinary/cloudinary.service';

@Controller('api/users')
export class UsersController {
  constructor(
    private usersService: UsersService,
    private cloudinaryService: CloudinaryService
  ) 
    {}


  @Get()
  findAll() {
    return this.usersService.findAll();
  }

  @Get(':userId')
  getProfile(@Param('userId',ParseIntPipe) userId:number){
    return this.usersService.getProfile(userId)
  }

  @Patch('/update/:userId')
  updateProfile(
    @Param('userId') userId:number,
    @Body() body:{
      name?: string;
      phone?: string;
      address?: string;
      avatarUrl?: string;
    }
  ){
    return this.usersService.updateProfile(userId,body)
  }

  @Delete(':id')
  remove(@Param('id') id: string) {
    return this.usersService.remove(id);
  }

  @Post('/upload-avatar')
  // @UseGuards(JwtAuthGuard)
  @UseInterceptors(FileInterceptor('avatar', {
    storage: memoryStorage(), 
    fileFilter: (req, file, cb) => {
      if (!file.mimetype.match(/\/(jpg|jpeg|png|webp)$/)) {
        return cb(new BadRequestException('Chỉ chấp nhận ảnh'), false)
      }
      cb(null, true)
    },
    limits: { fileSize: 2 * 1024 * 1024 }, // 2MB
  }))
  async uploadAvatar(
    @UploadedFile() file: Express.Multer.File,
    @Req() req,
  ) {
    const avatarUrl = await this.cloudinaryService.uploadImage(file)
    // await this.usersService.updateAvatar(req.user.id, avatarUrl)
    return { avatarUrl }
  }


  @EventPattern('user-registered-events')
  handleUserRegistered(@Payload() data: {
    UserId: number;
    UserName: string;
    Email: string;
    RegisteredAt:string
  }) {
    return this.usersService.createUserFromEvent(data)
  }

}