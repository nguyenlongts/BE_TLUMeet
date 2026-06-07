import { Module } from '@nestjs/common';
import { MongooseModule } from '@nestjs/mongoose';
import { JwtModule } from '@nestjs/jwt';
import { ClientsModule, Transport } from '@nestjs/microservices';
import { UserStat, UserStatSchema } from '../schemas/user-stat.schema';
import { MeetingStat, MeetingStatSchema } from '../schemas/meeting-stat.schema';
import { AdminService } from './admin.service';
import { AdminController } from './admin.controller';
import { JwtAdminGuard } from './jwt-admin.guard';

@Module({
  imports: [
    MongooseModule.forFeature([
      { name: UserStat.name, schema: UserStatSchema },
      { name: MeetingStat.name, schema: MeetingStatSchema },
    ]),
    JwtModule.register({}),

    ClientsModule.register([
      {
        name: 'KAFKA_PRODUCER',
        transport: Transport.KAFKA,
        options: {
          client: {
            clientId: 'admin-service-producer',
            brokers: (process.env.KAFKA_BROKERS ?? 'kafka:29092').split(','),
          },
          producer: {
            allowAutoTopicCreation: true,
          },
        },
      },
    ]),
  ],
  providers: [AdminService, JwtAdminGuard],
  controllers: [AdminController],
})
export class AdminModule {}
