using Bussiness;
using Game.Base.Packets;
using SqlDataProvider.Data;
using System;

namespace Game.Server.Packets.Client
{
	//[PacketHandler((int)ePackageType.LINKREQUEST_GOODS, "物品比较")]
	public class ItemCompareHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			//Console.WriteLine("ItemCompare");
            if (packet.ReadInt() != 2)
			{
				return 0;
			}
            string itemIDStr = packet.ReadString();
            int itemID = Int32.Parse(itemIDStr);
			//Console.WriteLine("ItemCompare: " + itemID);
			using (PlayerBussiness playerBussiness = new PlayerBussiness())
			{
				ItemInfo userItemSingle = playerBussiness.GetUserItemSingle(itemID);
				//Console.WriteLine("ItemCompare userItemSingle: " + (userItemSingle != null ? userItemSingle.ItemID.ToString() : "null"));
				if (userItemSingle != null)
				{
					GSPacketIn packet2 = new GSPacketIn((int)ePackageType.LINKREQUEST_GOODS, client.Player.PlayerCharacter.ID);
					packet2.WriteInt(userItemSingle.TemplateID);
					packet2.WriteInt(userItemSingle.ItemID);
					packet2.WriteInt(userItemSingle.StrengthenLevel);
					packet2.WriteInt(userItemSingle.AttackCompose);
					packet2.WriteInt(userItemSingle.AgilityCompose);
					packet2.WriteInt(userItemSingle.LuckCompose);
					packet2.WriteInt(userItemSingle.DefendCompose);
					packet2.WriteInt(userItemSingle.ValidDate);
					packet2.WriteBoolean(userItemSingle.IsBinds);
					packet2.WriteBoolean(userItemSingle.IsJudge);
					packet2.WriteBoolean(userItemSingle.IsUsed);
					if (userItemSingle.IsUsed)
					{
						packet2.WriteString(userItemSingle.BeginDate.ToString());
					}
					packet2.WriteInt(userItemSingle.Hole1);
					packet2.WriteInt(userItemSingle.Hole2);
					packet2.WriteInt(userItemSingle.Hole3);
					packet2.WriteInt(userItemSingle.Hole4);
					packet2.WriteInt(userItemSingle.Hole5);
					packet2.WriteInt(userItemSingle.Hole6);
					packet2.WriteString(userItemSingle.Template.Hole);
					client.Out.SendTCP(packet2);
				}
				return 1;
			}
		}
	}
}
