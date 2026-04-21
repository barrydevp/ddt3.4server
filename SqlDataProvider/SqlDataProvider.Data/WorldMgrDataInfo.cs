using System.Collections.Generic;
using ProtoBuf;

namespace SqlDataProvider.Data
{
	[ProtoContract]
	public class WorldMgrDataInfo
	{
		[ProtoMember(1)]
		public Dictionary<long, ShopFreeCountInfo> ShopFreeCount;
	}
}
