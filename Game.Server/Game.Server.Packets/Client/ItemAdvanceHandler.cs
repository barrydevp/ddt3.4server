using Bussiness;
using Bussiness.Managers;
using Game.Base.Packets;
using Game.Server.GameUtils;
using Game.Server.Managers;
using SqlDataProvider.Data;
using System;
using System.Text;

namespace Game.Server.Packets.Client
{
	[PacketHandler((int)ePackageType.ITEM_ADVANCE, "物品强化")]
	public class ItemAdvanceHandler : IPacketHandler
	{
		public static ThreadSafeRandom random = new ThreadSafeRandom();

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			//client.Out.SendMessage(eMessageType.Normal, "Tính năng bị khóa!");
			//return 0;
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			//packet.ReadBoolean();
			//packet.ReadBoolean();
            int Place = packet.ReadInt();  
            int BagType = packet.ReadInt();
            //int templateID = packet.ReadInt();
            int stonePlace = packet.ReadInt();
            int stongBagType = packet.ReadInt();
            //int stoneId = packet.ReadInt();
            GSPacketIn gSPacketIn = new GSPacketIn((int)ePackageType.ITEM_ADVANCE, client.Player.PlayerCharacter.ID);
            PlayerInventory itemBag = client.Player.GetInventory((eBageType)BagType);
            PlayerInventory stoneBag = client.Player.GetInventory((eBageType)stongBagType);
            ItemInfo mainItem = itemBag.GetItemAt(Place);
            ItemInfo stoneItem = stoneBag.GetItemAt(stonePlace);

			int strengthenLevel = mainItem.StrengthenLevel;
			int num;
			int result;
			if (stoneItem == null || mainItem == null || stoneItem.Count <= 0)
			{
				client.Out.SendMessage(eMessageType.BIGBUGLE_NOTICE, LanguageMgr.GetTranslation("Đặt đá tăng cấp và trang bị cần tăng cấp vào!", new object[0]));
				num = 0;
			}
			else if (strengthenLevel >= 15)
			{
				client.Out.SendMessage(eMessageType.BIGBUGLE_NOTICE, LanguageMgr.GetTranslation("Level đã đạt cấp độ cao nhất, không thể thăng cấp!", new object[0]));
				num = 0;
			}
			else
			{
				int count = 1;
				string text = "";
				if (mainItem != null && mainItem.Template.CanStrengthen && mainItem.Template.CategoryID < 18 && mainItem.Count == 1)
				{
					flag = (flag || mainItem.IsBinds);
					stringBuilder.Append(string.Concat(new object[]
					{
						mainItem.ItemID,
						":",
						mainItem.TemplateID,
						","
					}));
					if (stoneItem.TemplateID < 11150 && stoneItem.TemplateID > 11154)
					{
						client.Out.SendMessage(eMessageType.BIGBUGLE_NOTICE, LanguageMgr.GetTranslation("Đặt đá tăng cấp vào!", new object[0]));
						num = 0;
						result = num;
						return result;
					}
					flag = (flag || stoneItem.IsBinds);
					string text2 = text;
					text = string.Concat(new string[]
					{
						text2,
						",",
						stoneItem.ItemID.ToString(),
						":",
						stoneItem.Template.Name
					});
					int num2 = (stoneItem.Template.Property2 < 10) ? 10 : stoneItem.Template.Property2;
					stringBuilder.Append("true");
					bool flag2 = false;
					int num3 = ItemAdvanceHandler.random.Next(50000);
					//double num4 = (double)(itemInfo.StrengthenExp / strengthenLevel);
					bool ActiveUpgrade = false;
					if (mainItem.StrengthenExp >= random.Next(4000,7500) && mainItem.StrengthenLevel == 12)
						ActiveUpgrade = true;
					if (mainItem.StrengthenExp >= random.Next(12500,17500) && mainItem.StrengthenLevel == 13)
						ActiveUpgrade = true;
					if (mainItem.StrengthenExp >= random.Next(25000,35000) && mainItem.StrengthenLevel == 14)
						ActiveUpgrade = true;
					if (ActiveUpgrade)
					{
						mainItem.IsBinds = flag;
						mainItem.StrengthenLevel++;
						mainItem.StrengthenExp = 0;
						gSPacketIn.WriteByte(0);
						gSPacketIn.WriteInt(num2);
						flag2 = true;
						StrengthenGoodsInfo strengthenGoodsInfo = StrengthenMgr.FindStrengthenGoodsInfo(mainItem.StrengthenLevel, mainItem.TemplateID);
						if (strengthenGoodsInfo != null && mainItem.Template.CategoryID == 7 && strengthenGoodsInfo.GainEquip > mainItem.TemplateID)
						{
							ItemTemplateInfo itemTemplateInfo = ItemMgr.FindItemTemplate(strengthenGoodsInfo.GainEquip);
							if (itemTemplateInfo != null)
							{
								ItemInfo newMainItem = ItemInfo.CloneFromTemplate(itemTemplateInfo, mainItem);
								itemBag.RemoveItemAt(Place);
								itemBag.AddItemTo(newMainItem, Place);
								mainItem = newMainItem;

                                if (mainItem.Place < 31)
                                {
                                    client.Player.EquipBag.UpdatePlayerProperties();
                                }
                            }
						}
					}
					else
					{
						mainItem.StrengthenExp += num2;
						gSPacketIn.WriteByte(1);
						gSPacketIn.WriteInt(num2);
					}
					stoneBag.RemoveCountFromStack(stoneItem, count);
					itemBag.UpdateItem(mainItem);
					client.Out.SendTCP(gSPacketIn);
					if (flag2 && mainItem.ItemID > 0)
					{
						string msg = LanguageMgr.GetTranslation("ItemStrengthenHandler.congratulation2", client.Player.PlayerCharacter.NickName, mainItem.TemplateID, mainItem.StrengthenLevel - 12);
						GSPacketIn sysNotice = WorldMgr.SendSysNotice(eMessageType.SYS_TIP_NOTICE, msg, mainItem.ItemID, mainItem.TemplateID, null);
						GameServer.Instance.LoginServer.SendPacket(sysNotice);
					}
					stringBuilder.Append(mainItem.StrengthenLevel);
				}
				else
				{
					client.Out.SendMessage(eMessageType.GM_NOTICE, LanguageMgr.GetTranslation("ItemStrengthenHandler.Content1", new object[0]) + stoneItem.Template.Name + LanguageMgr.GetTranslation("ItemStrengthenHandler.Content2", new object[0]));
				}
				//if (mainItem.Place < 31)
				//{
				//	client.Player.EquipBag.UpdatePlayerProperties();
				//}
				num = 0;
			}
			result = num;
			return result;
		}
	}
}
