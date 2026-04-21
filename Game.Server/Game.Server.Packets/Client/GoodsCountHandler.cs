using System.Collections.Generic;
using Game.Base.Packets;
using Game.Server.Managers;
using SqlDataProvider.Data;

namespace Game.Server.Packets.Client
{
	[PacketHandler((int)ePackageType.GOODS_COUNT, "物品强化")]
	public class GoodsCountHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			List<ShopFreeCountInfo> allShopFreeCount = WorldMgr.GetAllShopFreeCount();
			client.Out.SendShopGoodsCountUpdate(allShopFreeCount);
			return 0;
		}
	}
}
