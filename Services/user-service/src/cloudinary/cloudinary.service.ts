import { Injectable } from '@nestjs/common'
import { v2 as cloudinary, UploadApiResponse } from 'cloudinary'
import { Express } from 'express'
import { Readable } from 'stream'

@Injectable()
export class CloudinaryService {
  async uploadImage(file:Express.Multer.File): Promise<string> {
    return new Promise((resolve, reject) => {
      const upload = cloudinary.uploader.upload_stream(
        { folder: 'avatars' },
        (error, result: UploadApiResponse | undefined) => {
          if (error) return reject(new Error(error.message))
          if (!result) return reject(new Error('Upload thất bại'))
          resolve(result.secure_url)
        }
      )
      Readable.from(file.buffer).pipe(upload)
    })
  }
}