using Bussiness;
using Game.Base.Packets;
using Game.Logic.Phy.Object;
using Game.Server.Buffer;
using Game.Server.Rooms;
using SqlDataProvider.Data;
using System;
namespace Game.Server.Packets.Client
{
    [PacketHandler((int)ePackageType.WORLD_BOSS, "场景用户离开")]
    public class WorldBossHandler : IPacketHandler
    {
        public int HandlePacket(GameClient client, GSPacketIn packet)
        {
            byte b = packet.ReadByte();
            if (!RoomMgr.WorldBossRoom.WorldbossOpen)
            {
                client.Out.SendMessage(eMessageType.Normal, LanguageMgr.GetTranslation("Boss thế giới đã kết thúc!"));
                return 0;
            }
            GSPacketIn gSPacketIn = new GSPacketIn((int)ePackageType.WORLD_BOSS, client.Player.PlayerCharacter.ID);
            {
                switch(b)
                {
                    case (byte)eWorldBossPackageRecvType.ENTER_WORLDBOSSROOM:
                        gSPacketIn.WriteByte((byte)eWorldBossPackageType.CANENTER);
                        gSPacketIn.WriteBoolean(true);
                        gSPacketIn.WriteBoolean(false);
                        gSPacketIn.WriteInt(0);
                        gSPacketIn.WriteInt(0);
                        client.Out.SendTCP(gSPacketIn);
                        return 0;
                    case (byte)eWorldBossPackageRecvType.LEAVE_ROOM:
                        RoomMgr.WorldBossRoom.RemovePlayer(client.Player);
                        client.Player.IsInWorldBossRoom = false;
                        break;
                    case (byte)eWorldBossPackageRecvType.ADDPLAYERS:
                        {
                            int x = packet.ReadInt();
                            int y = packet.ReadInt();
                            client.Player.X = x;
                            client.Player.Y = y;
                            if (client.Player.CurrentRoom != null)
                            {
                                client.Player.CurrentRoom.RemovePlayerUnsafe(client.Player);
                            }
                            BaseWorldBossRoom worldBossRoom = RoomMgr.WorldBossRoom;
                            if (client.Player.IsInWorldBossRoom)
                            {
                                gSPacketIn.WriteByte((byte)eWorldBossPackageType.WORLDBOSS_EXIT);
                                gSPacketIn.WriteInt(client.Player.PlayerId);
                                worldBossRoom.SendToALL(gSPacketIn);
                                worldBossRoom.RemovePlayer(client.Player);
                                client.Player.IsInWorldBossRoom = false;
                            }
                            else
                            {
                                if (worldBossRoom.AddPlayer(client.Player))
                                {
                                    worldBossRoom.ViewOtherPlayerRoom(client.Player);
                                }
                            }
                            break;
                        }
                    case (byte)eWorldBossPackageRecvType.MOVE:
                        {
                            int num = packet.ReadInt();
                            int num2 = packet.ReadInt();
                            string str = packet.ReadString();
                            gSPacketIn.WriteByte((byte)eWorldBossPackageType.MOVE);
                            gSPacketIn.WriteInt(client.Player.PlayerId);
                            gSPacketIn.WriteInt(num);
                            gSPacketIn.WriteInt(num2);
                            gSPacketIn.WriteString(str);
                            client.Player.SendTCP(gSPacketIn);
                            RoomMgr.WorldBossRoom.SendToALL(gSPacketIn, client.Player);
                            client.Player.X = num;
                            client.Player.Y = num2;
                            break;
                        }
                    case (byte)eWorldBossPackageRecvType.STAUTS:
                        {
                            byte b2 = packet.ReadByte();
                            if (b2 != 3 || client.Player.States != 3)
                            {
                                gSPacketIn.WriteByte((byte)eWorldBossPackageType.WORLDBOSS_PLAYERSTAUTSUPDATE);
                                gSPacketIn.WriteInt(client.Player.PlayerId);
                                gSPacketIn.WriteByte(b2);
                                gSPacketIn.WriteInt(client.Player.X);
                                gSPacketIn.WriteInt(client.Player.Y);
                                RoomMgr.WorldBossRoom.SendToALL(gSPacketIn);
                                if (b2 == 3 && client.Player.CurrentRoom.Game != null)
                                {
                                    client.Player.CurrentRoom.RemovePlayerUnsafe(client.Player);
                                }
                                string nickName = client.Player.PlayerCharacter.NickName;
                                RoomMgr.WorldBossRoom.SendPrivateInfoToCenter(nickName);
                            }
                            client.Player.States = b2;
                            break;
                        }
                    case (byte)eWorldBossPackageRecvType.REQUEST_REVIVE:
                        {
                            int num3 = packet.ReadInt();
                            packet.ReadBoolean();
                            int value = RoomMgr.WorldBossRoom.reviveMoney;
                            if (num3 == 2)
                            {
                                value = RoomMgr.WorldBossRoom.reFightMoney;
                            }
                            if (client.Player.MoneyDirect(value, false))
                            {
                                client.Player.LastEnterWorldBoss = DateTime.MinValue;
                                gSPacketIn.WriteByte((byte)eWorldBossPackageType.WORLDBOSS_PLAYER_REVIVE);
                                gSPacketIn.WriteInt(client.Player.PlayerId);
                                RoomMgr.WorldBossRoom.SendToALL(gSPacketIn);
                            }
                            break;
                        }
                    case (byte)eWorldBossPackageRecvType.BUFF_BUY:
                        {
                            //int addInjureBuffMoney = RoomMgr.WorldBossRoom.addInjureBuffMoney;
                            //int addInjureValue = RoomMgr.WorldBossRoom.addInjureValue;
                            int nBuff = packet.ReadInt();
                            int nBuy = 0;
                            for (int i = 0; i < nBuff; i++)
                            {
                                int bufId = packet.ReadInt();

                                WorldBossBuffInfo info = null;
                                //Console.WriteLine($"buffId: {bufId}");
                                if(!RoomMgr.WorldBossRoom.buyableBuffs.TryGetValue(bufId, out info) || info == null)
                                {
                                    continue;
                                }
                                BufferInfo appliedBuff = client.Player.FindFightBuff(info.Type);
                                if (appliedBuff != null && appliedBuff.ValidCount >= info.MaxCount)
                                {
                                    client.Out.SendMessage(eMessageType.Normal, LanguageMgr.GetTranslation($"Chỉ được mua tối đa {info.MaxCount} {info.Name}!"));
                                    continue;
                                }

                                AbstractBuffer abstractBuffer = BufferList.CreatePayBuffer((int)info.Type, info.Value, 5);
                                if (abstractBuffer == null)
                                {
                                    client.Out.SendMessage(eMessageType.Normal, LanguageMgr.GetTranslation($"Không tìm thấy {info.Name}!"));
                                    continue;
                                }

                                if (client.Player.MoneyDirect(info.Price, false))
                                {
                                    //client.Player.RemoveMoney(info.Price);
                                    abstractBuffer.Start(client.Player);
                                    client.Out.SendMessage(eMessageType.Normal, LanguageMgr.GetTranslation($"Đã mua {info.Name}, tốn {info.Price} xu!"));
                                }
                            }
                           
                            break;
                        }
                    default:
                        Console.WriteLine("WorldBossPackageType." + (WorldBossPackageType)b);
                        break;
                }
                return 0;
            }
        }
    }
}