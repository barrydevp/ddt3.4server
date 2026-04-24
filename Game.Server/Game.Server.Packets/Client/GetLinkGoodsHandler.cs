using System;
using Bussiness;
using Game.Base.Packets;
using SqlDataProvider.Data;

namespace Game.Server.Packets.Client
{
    [PacketHandler((int)ePackageType.LINKREQUEST_GOODS, "物品比较")]
    public class GetLinkGoodsHandler : IPacketHandler
    {
        public int HandlePacket(GameClient client, GSPacketIn packet)
        {
            //Console.WriteLine("GetLinkGoods");
            int type = packet.ReadInt();
            string itemIDStr = packet.ReadString();
            int itemID = Int32.Parse(itemIDStr);
            GSPacketIn outPkg = new GSPacketIn((int)ePackageType.LINKREQUEST_GOODS, client.Player.PlayerCharacter.ID);
            string nickName = client.Player.PlayerCharacter.NickName;
            using (PlayerBussiness bussiness = new PlayerBussiness())
            {
                outPkg.WriteInt(type);
                switch (type)
                {
                    case 4:
                        outPkg.WriteString(nickName);
                        outPkg.WriteInt(0);
                        outPkg.WriteInt(0);
                        outPkg.WriteInt(0);
                        outPkg.WriteInt(0);
                        outPkg.WriteInt(0);
                        client.Out.SendTCP(outPkg);
                        return 0;
                    case 5:
                        packet.ReadString();
                        outPkg.WriteString(nickName);
                        outPkg.WriteInt(0);
                        outPkg.WriteInt(0);
                        outPkg.WriteInt(0);
                        outPkg.WriteInt(0);
                        outPkg.WriteInt(0);
                        outPkg.WriteInt(0);
                        outPkg.WriteInt(0);
                        outPkg.WriteInt(0);
                        client.Out.SendTCP(outPkg);
                        return 0;
                    default:
                        {
                            ItemInfo userItemSingle = bussiness.GetUserItemSingle(itemID);
                            //Console.WriteLine("userItemSingle: " + (userItemSingle != null ? userItemSingle.ItemID.ToString() : "null"));
                            if (userItemSingle != null)
                            {
                                outPkg.WriteString(nickName);
                                outPkg.WriteInt(userItemSingle.TemplateID);
                                outPkg.WriteInt(userItemSingle.ItemID);
                                outPkg.WriteInt(userItemSingle.StrengthenLevel);
                                outPkg.WriteInt(userItemSingle.AttackCompose);
                                outPkg.WriteInt(userItemSingle.AgilityCompose);
                                outPkg.WriteInt(userItemSingle.LuckCompose);
                                outPkg.WriteInt(userItemSingle.DefendCompose);
                                outPkg.WriteInt(userItemSingle.ValidDate);
                                outPkg.WriteBoolean(userItemSingle.IsBinds);
                                outPkg.WriteBoolean(userItemSingle.IsJudge);
                                outPkg.WriteBoolean(userItemSingle.IsUsed);
                                if (userItemSingle.IsUsed)
                                {
                                    outPkg.WriteString(userItemSingle.BeginDate.ToString());
                                }
                                outPkg.WriteInt(userItemSingle.Hole1);
                                outPkg.WriteInt(userItemSingle.Hole2);
                                outPkg.WriteInt(userItemSingle.Hole3);
                                outPkg.WriteInt(userItemSingle.Hole4);
                                outPkg.WriteInt(userItemSingle.Hole5);
                                outPkg.WriteInt(userItemSingle.Hole6);
                                outPkg.WriteString(userItemSingle.Template.Hole);
                                outPkg.WriteString(userItemSingle.Template.Pic);
                                outPkg.WriteInt(userItemSingle.RefineryLevel);
                                outPkg.WriteDateTime(DateTime.Now);
                                outPkg.WriteByte((byte)userItemSingle.Hole5Level);
                                outPkg.WriteInt(userItemSingle.Hole5Exp);
                                outPkg.WriteByte((byte)userItemSingle.Hole6Level);
                                outPkg.WriteInt(userItemSingle.Hole6Exp);
                                outPkg.WriteBoolean(userItemSingle.IsGold);
                                if (userItemSingle.IsGold)
                                {
                                    outPkg.WriteInt(userItemSingle.goldValidDate);
                                    outPkg.WriteDateTime(userItemSingle.goldBeginTime);
                                }
                                client.Out.SendTCP(outPkg);
                            }
                            return 1;
                        }
                }
            }
        }
    }
}
