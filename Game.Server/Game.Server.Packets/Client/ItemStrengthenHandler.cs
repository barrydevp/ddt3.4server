using System;
using System.Collections.Generic;
using Bussiness;
using Bussiness.Managers;
using Game.Base.Packets;
using Game.Server.Managers;
using SqlDataProvider.Data;

namespace Game.Server.Packets.Client
{
	[PacketHandler((int)ePackageType.ITEM_STRENGTHEN, "物品强化")]
	public class ItemStrengthenHandler : IPacketHandler
	{
		public static int countConnect;

		private static RandomSafe random;

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			GSPacketIn packet2 = packet.Clone();
			packet2.ClearContext();
			bool isConsortia = packet.ReadBoolean();
			List<ItemInfo> stoneItems = new List<ItemInfo>();
			ItemInfo mainItem = client.Player.StoreBag.GetItemAt(5);
			ItemInfo luckItem = null;
			ItemInfo protectItem = null;
			bool isProtected = false;
			double originalPoint = 0.0;
			double luckPoint = 0.0;
			double vipProb = 0.0;
			double consortiaProb = 0.0;
			if (mainItem != null && mainItem.Template.CanStrengthen && mainItem.Count == 1)
			{
				if (mainItem.StrengthenLevel >= 12)
				{
					client.Out.SendMessage(eMessageType.GM_NOTICE, "Vật phẩm này đã đạt cấp cường hóa tối đa!");
					return 0;
				}
				bool isBind = mainItem.IsBinds;
				ItemInfo itemAt1 = client.Player.StoreBag.GetItemAt(0);
				if (itemAt1 != null && itemAt1.Template.CategoryID == 11 && (itemAt1.Template.Property1 == 2 || itemAt1.Template.Property1 == 35) && !stoneItems.Contains(itemAt1))
				{
					stoneItems.Add(itemAt1);
					originalPoint += StrengthenMgr.RateItems[itemAt1.Template.Level - 1];
				}
				ItemInfo itemAt2 = client.Player.StoreBag.GetItemAt(1);
				if (itemAt2 != null && itemAt2.Template.CategoryID == 11 && (itemAt2.Template.Property1 == 2 || itemAt2.Template.Property1 == 35) && !stoneItems.Contains(itemAt2))
				{
					stoneItems.Add(itemAt2);
					originalPoint += StrengthenMgr.RateItems[itemAt2.Template.Level - 1];
				}
				ItemInfo itemAt3 = client.Player.StoreBag.GetItemAt(2);
				if (itemAt3 != null && itemAt3.Template.CategoryID == 11 && (itemAt3.Template.Property1 == 2 || itemAt3.Template.Property1 == 35) && !stoneItems.Contains(itemAt3))
				{
					stoneItems.Add(itemAt3);
					originalPoint += StrengthenMgr.RateItems[itemAt3.Template.Level - 1];
				}
				if (client.Player.StoreBag.GetItemAt(4) != null)
				{
					luckItem = client.Player.StoreBag.GetItemAt(4);
					if (luckItem != null && luckItem.Template.CategoryID == 11 && luckItem.Template.Property1 == 3)
					{
						luckPoint += originalPoint + (double)(luckItem.Template.Property2 / 100);
					}
					else
					{
						luckItem = null;
					}
				}
				if (client.Player.StoreBag.GetItemAt(3) != null)
				{
					protectItem = client.Player.StoreBag.GetItemAt(3);
					if (protectItem != null && protectItem.Template.CategoryID == 11 && protectItem.Template.Property1 == 7)
					{
						isProtected = true;
					}
					else
					{
						protectItem = null;
					}
				}
				double originalProb = originalPoint * 100.0 / (double)StrengthenMgr.GetNeedRate(mainItem);
				double luckProb = luckPoint * 100.0 / (double)StrengthenMgr.GetNeedRate(mainItem);
				if ((itemAt1 != null && itemAt1.IsBinds) || (itemAt2 != null && itemAt2.IsBinds) || (itemAt3 != null && itemAt3.IsBinds) || (luckItem != null && luckItem.IsBinds) || (protectItem != null && protectItem.IsBinds))
				{
					isBind = true;
				}
				if (isConsortia)
				{
					ConsortiaInfo consortiaInfo = ConsortiaMgr.FindConsortiaInfo(client.Player.PlayerCharacter.ConsortiaID);
					ConsortiaEquipControlInfo consortiaEuqipRiches = new ConsortiaBussiness().GetConsortiaEuqipRiches(client.Player.PlayerCharacter.ConsortiaID, 0, 2);
					if (consortiaInfo == null)
					{
						client.Out.SendMessage(eMessageType.GM_NOTICE, LanguageMgr.GetTranslation("ItemStrengthenHandler.Fail"));
					}
					else if (client.Player.PlayerCharacter.Riches < consortiaEuqipRiches.Riches)
					{
						client.Out.SendMessage(eMessageType.BIGBUGLE_NOTICE, LanguageMgr.GetTranslation("ItemStrengthenHandler.FailbyPermission"));
					}
					else
					{
						consortiaProb = originalProb * (0.1 * (double)consortiaInfo.SmithLevel);
					}
				}
				if (client.Player.PlayerCharacter.typeVIP > 0)
				{
					vipProb += StrengthenMgr.VIPStrengthenEx * originalProb;
				}
				if (stoneItems.Count >= 1)
				{
					mainItem.StrengthenTimes++;
					mainItem.IsBinds = isBind;
					client.Player.StoreBag.ClearBag();
					double totalProb = Math.Floor((originalProb + luckProb + consortiaProb + vipProb) * 100.0);
					//Console.WriteLine("originalProb: " + originalProb + " luckProb: " + luckProb + " consortiaProb: " + consortiaProb + " vipProb: " + vipProb + " totalProb: " + totalProb);
                    int randomUp = random.Next(10000);
					if (totalProb > (double)randomUp)
					{
						packet2.WriteByte(0);
						packet2.WriteBoolean(val: true);
						mainItem.StrengthenLevel++;
						StrengthenGoodsInfo strengthenGoodsInfo = StrengthenMgr.FindStrengthenGoodsInfo(mainItem.StrengthenLevel, mainItem.TemplateID);
						if (strengthenGoodsInfo != null && mainItem.Template.CategoryID == 7 && strengthenGoodsInfo.GainEquip > mainItem.TemplateID)
						{
							ItemTemplateInfo itemTemplate2 = ItemMgr.FindItemTemplate(strengthenGoodsInfo.GainEquip);
							if (itemTemplate2 != null)
							{
								ItemInfo itemInfo3 = ItemInfo.CloneFromTemplate(itemTemplate2, mainItem);
								client.Player.StoreBag.RemoveItemAt(5);
								mainItem = itemInfo3;
							}
						}
						ItemInfo.OpenHole(ref mainItem);
						client.Player.StoreBag.AddItemTo(mainItem, 5);
						client.Player.OnItemStrengthen(mainItem.Template.CategoryID, mainItem.StrengthenLevel);
						client.Player.SaveIntoDatabase();
						if (mainItem.StrengthenLevel >= 7)
						{
							GameServer.Instance.LoginServer.SendPacket(WorldMgr.SendSysNotice(eMessageType.ChatNormal, LanguageMgr.GetTranslation("ItemStrengthenHandler.congratulation", client.Player.ZoneName, client.Player.PlayerCharacter.NickName, mainItem.TemplateID, mainItem.StrengthenLevel), mainItem.ItemID, mainItem.TemplateID, null));
						}
						/*if (mainItem.Template.CategoryID == 7 && client.Player.Extra.CheckNoviceActiveOpen(NoviceActiveType.STRENGTHEN_WEAPON_ACTIVE))
						{
							client.Player.Extra.UpdateEventCondition(2, mainItem.StrengthenLevel);
						}*/
					}
					else
					{
						packet2.WriteByte(1);
						packet2.WriteBoolean(val: false);
						if (!isProtected)
						{
							if (mainItem.Template.Level == 3)
							{
								mainItem.StrengthenLevel = ((mainItem.StrengthenLevel < 5) ? mainItem.StrengthenLevel : (mainItem.StrengthenLevel - 1));
                                if (mainItem.Template.CategoryID == 7)
                                {
                                    StrengthenGoodsInfo strengthenGoodInfo = StrengthenMgr.FindRealStrengthenGoodInfo(mainItem.StrengthenLevel, mainItem.TemplateID);
                                    if (strengthenGoodInfo != null && mainItem.TemplateID != strengthenGoodInfo.GainEquip)
                                    {
                                        ItemTemplateInfo itemTemplate = ItemMgr.FindItemTemplate(strengthenGoodInfo.GainEquip);
                                        if (itemTemplate != null)
                                        {
                                            ItemInfo itemInfo4 = ItemInfo.CloneFromTemplate(itemTemplate, mainItem);
                                            client.Player.StoreBag.RemoveItemAt(5);
                                            mainItem = itemInfo4;
                                        }
                                    }
                                }

                                client.Player.StoreBag.AddItemTo(mainItem, 5);
							}
							else
							{
								mainItem.Count--;
								client.Player.StoreBag.AddItemTo(mainItem, 5);
							}
						}
						else
						{
							client.Player.StoreBag.AddItemTo(mainItem, 5);
						}
						ItemInfo.OpenHole(ref mainItem);
						client.Player.SaveIntoDatabase();
					}
					client.Out.SendTCP(packet2);
					if (mainItem.Place < 31)
					{
						client.Player.EquipBag.UpdatePlayerProperties();
					}
				}
				else
				{
					client.Out.SendMessage(eMessageType.GM_NOTICE, LanguageMgr.GetTranslation("ItemStrengthenHandler.Content1") + 1 + LanguageMgr.GetTranslation("ItemStrengthenHandler.Content2"));
				}
			}
			else
			{
				client.Out.SendMessage(eMessageType.GM_NOTICE, LanguageMgr.GetTranslation("ItemStrengthenHandler.Success"));
			}
			return 0;
		}

		static ItemStrengthenHandler()
		{
			countConnect = 0;
			random = new RandomSafe();
		}
	}
}
