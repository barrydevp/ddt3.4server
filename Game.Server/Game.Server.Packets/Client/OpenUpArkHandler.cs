using Bussiness;
using Bussiness.Managers;
using Game.Base.Packets;
using Game.Server.GameUtils;
using Game.Server.Managers;
using log4net;
using SqlDataProvider.Data;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace Game.Server.Packets.Client
{
    [PacketHandler((int)ePackageType.ITEM_OPENUP, "打开物品")]
    public class OpenUpArkHandler : IPacketHandler
    {
        public static readonly ILog log = LogManager.GetLogger("FlashErrorLogger");

        public int HandlePacket(GameClient client, GSPacketIn packet)
        {
            int bageType = (int)packet.ReadByte();
            int slot = packet.ReadInt();
            int count = packet.ReadInt();

            PlayerInventory inventory = client.Player.GetInventory((eBageType)bageType);
            ItemInfo itemAt = inventory.GetItemAt(slot);

            if (itemAt == null || !itemAt.IsValidItem())
                return 0;

            if (client.Player.PlayerCharacter.Grade < itemAt.Template.NeedLevel)
            {
                client.Player.SendMessage(LanguageMgr.GetTranslation("GameServer.OpenUp.MsgNoLevel"));
                return 0;
            }

            if (itemAt.Template.CategoryID == 11 && itemAt.Template.Property1 == 6)
            {
                return tryOpenItemBox(client, inventory, itemAt, count);
            }

            if (itemAt.Template.CategoryID == 72)
            {
                return tryOpenExplorerManual(client, inventory, itemAt, count);
            }

            client.Player.SendMessage(LanguageMgr.GetTranslation("GameServer.OpenUp.MsgCantOpen"));

            return 1;
        }

        private int tryOpenItemBox(GameClient client, PlayerInventory inventory, ItemInfo itemAt, int count)
        {
            if (count < 1 || count > itemAt.Count)
            {
                count = itemAt.Count;
            }

            if (!inventory.RemoveCountFromStack(itemAt, count))
            {
                return 0;
            }


            var dictRewards = new Dictionary<int, ItemInfo>();
            var rand = new Random();

            int money = 0, gold = 0, giftToken = 0, medal = 0, exp = 0, hardCurrency = 0, leagueMoney = 0, usableScore = 0, prestige = 0, honor = 0;
            var openedItems = new List<ItemInfo>();

            for (int i = 0; i < count; i++)
            {
                ItemBoxMgr.CreateItemBox(itemAt, openedItems, ref gold, ref money, ref giftToken, ref medal, ref exp, ref hardCurrency, ref leagueMoney, ref usableScore, ref prestige, ref honor);
            }

            foreach (ItemInfo item in openedItems)
            {
                if (!dictRewards.ContainsKey(item.TemplateID))
                    dictRewards.Add(item.TemplateID, item);
                else
                    dictRewards[item.TemplateID].Count += item.Count;
            }

            var currencyMsgs = new List<string>();

            if (money != 0)
            {
                currencyMsgs.Add($"{money}{LanguageMgr.GetTranslation("OpenUpArkHandler.Money")}");
                client.Player.AddMoney(money);
            }
            if (gold != 0)
            {
                currencyMsgs.Add($"{gold}{LanguageMgr.GetTranslation("OpenUpArkHandler.Gold")}");
                client.Player.AddGold(gold);
            }
            if (giftToken != 0)
            {
                currencyMsgs.Add($"{giftToken}{LanguageMgr.GetTranslation("OpenUpArkHandler.GiftToken")}");
                client.Player.AddGiftToken(giftToken);
            }
            if (medal != 0)
            {
                currencyMsgs.Add($"{medal}{LanguageMgr.GetTranslation("OpenUpArkHandler.Medal")}");
                client.Player.AddMedal(medal);
            }
            if (exp != 0)
            {
                currencyMsgs.Add(AddExp(client, exp));
            }
            if (hardCurrency != 0)
            {
                currencyMsgs.Add($"{hardCurrency}{LanguageMgr.GetTranslation("OpenUpArkHandler.hardCurrency")}");
                client.Player.AddHardCurrency(hardCurrency);
            }
            if (leagueMoney != 0)
            {
                currencyMsgs.Add($"{leagueMoney}{LanguageMgr.GetTranslation("OpenUpArkHandler.leagueMoney")}");
                client.Player.AddLeagueMoney(leagueMoney);
            }
            if (usableScore != 0)
            {
                currencyMsgs.Add($"{usableScore}{LanguageMgr.GetTranslation("OpenUpArkHandler.useableScore")}");
            }
            if (prestige != 0)
            {
                currencyMsgs.Add($"{prestige}{LanguageMgr.GetTranslation("OpenUpArkHandler.prestge")}");
            }
            if (honor != 0)
            {
                currencyMsgs.Add($"{honor}{LanguageMgr.GetTranslation("OpenUpArkHandler.Honor")}");
                client.Player.AddHonor(honor);
            }

            if (currencyMsgs.Count > 0)
            {
                client.Player.SendMessage($"{LanguageMgr.GetTranslation("OpenUpArkHandler.Start")}{string.Join(", ", currencyMsgs)}");
            }

            return deliverDictionaryRewards(client, itemAt, dictRewards);
        }

        //private void OpenSpecialItem(GameClient client, int templateId, Random rand)
        //{
        //    switch (templateId)
        //    {
        //        // Rương Xu
        //        case 52000621: SendBonusMoney(client, rand.Next(500, 1000), "Trung Cấp"); break;
        //        case 52000622: SendBonusMoney(client, rand.Next(5000, 10000), "Cao Cấp"); break;
        //        case 11917:
        //            client.Player.AddHonor(200);
        //            client.Player.SendMessage("Bạn nhận được 200 tinh hoa vinh dự");
        //            break;
        //        // Rương Random
        //        case 52000633: SendRandomItem(client, rand.Next(30, 50), GameProperties.AwardItemBox); break;
        //        case 52000634: SendRandomItem(client, rand.Next(50, 100), GameProperties.AwardItemBox); break;
        //        case 52000635: SendRandomItem(client, rand.Next(100, 200), GameProperties.AwardItemBox); break;
        //        case 52000636: SendRandomItem(client, rand.Next(200, 400), GameProperties.AwardItemBoxI); break;
        //        case 52000637: SendRandomItem(client, rand.Next(350, 500), GameProperties.AwardItemBoxI); break;
        //    }
        //}

        private string AddExp(GameClient client, int expNum)
        {
            if (client.Player.Level == LevelMgr.MaxLevel)
            {
                int merit = expNum / 500;
                if (merit > 0)
                {
                    client.Player.AddOffer(merit);
                    return $"kinh nghiệm quy đổi thành {merit} công trạng do max level";
                }
                return "";
            }

            client.Player.AddGP(expNum, false, false);
            return $"{expNum}{LanguageMgr.GetTranslation("OpenUpArkHandler.Exp")}";
        }

        private int deliverDictionaryRewards(GameClient client, ItemInfo itemAt, Dictionary<int, ItemInfo> dictRewards)
        {
            if (dictRewards.Count <= 0)
            {
                return 0;
            }

            //var itemMsgs = new List<string>();
            List<ItemInfo> mailedRewards = new List<ItemInfo>();

            var packet = new GSPacketIn((int)ePackageType.ITEM_OPENUP, client.Player.PlayerCharacter.ID);
            packet.WriteString(itemAt.Template.Name);
            packet.WriteInt(dictRewards.Count);

            foreach (var item in dictRewards.Values)
            {
                packet.WriteInt(item.TemplateID);
                packet.WriteInt(item.Count);
                packet.WriteBoolean(item.IsBinds);
                packet.WriteInt(item.ValidDate);
                packet.WriteInt(item.StrengthenLevel);
                packet.WriteInt(item.AttackCompose);
                packet.WriteInt(item.DefendCompose);
                packet.WriteInt(item.AgilityCompose);
                packet.WriteInt(item.LuckCompose);

                //itemMsgs.Add($"{item.Template.Name}x{item.Count}");

                foreach (var splitItem in ItemMgr.SpiltGoodsMaxCount(item))
                {
                    if (!client.Player.StoreBag.AddItem(splitItem))
                    {
                        mailedRewards.Add(splitItem);
                    }
                    if (item.IsTips)
                    {
                        string msg = LanguageMgr.GetTranslation("GameServer.OpenUpArkNoticeServer", client.Player.PlayerCharacter.NickName, itemAt.Template.Name, splitItem.TemplateID, splitItem.Count);
                        GSPacketIn noticePkt = WorldMgr.SendSysNotice(eMessageType.ChatNormal, msg, splitItem.ItemID, splitItem.TemplateID, null);
                        GameServer.Instance.LoginServer.SendPacket(noticePkt);
                    }
                }
            }

            packet.WriteInt(0);
            packet.WriteInt(itemAt.Template.CategoryID);
            packet.WriteInt(0);

            client.Player.SendTCP(packet);

            //if (itemMsgs.Count > 0)
            //{
            //    client.Player.SendMessage($"{LanguageMgr.GetTranslation("OpenUpArkHandler.Start")}{string.Join(", ", itemMsgs)}");
            //}

            if (mailedRewards.Count > 0)
            {
                client.Player.SendItemsToMail(mailedRewards, "Vật phẩm gửi về thư do mở quà trong khi túi đầy.", "Mở túi quà đầy gửi về thư", eMailType.BuyItem);
            }

            return 1;
        }

        private int tryOpenExplorerManual(GameClient client, PlayerInventory inventory, ItemInfo itemAt, int count)
        {
            if (!inventory.RemoveCountFromStack(itemAt, count))
                return 0;

            int value = 1;
            int type = 0;
            int max = -1;
            int chapter = -1;

            switch (itemAt.TemplateID)
            {
                // Tiền thám hiểm trực tiếp
                case 1120657: value = 3200; type = 1; break;
                case 1120656: value = 17500; type = 1; break;
                case 1120655: value = 96000; type = 1; break;

                // Sổ tay thám hiểm (Random theo loại)
                case 1120637: type = 2; max = 9; break;
                case 1120638: type = 2; max = 16; break;
                case 1120639: type = 2; max = 25; break;

                // Sổ tay thám hiểm (Chap 1-5 loại nhỏ)
                case 1120640: type = 2; max = 9; chapter = 1001; break;
                case 1120641: type = 2; max = 9; chapter = 1002; break;
                case 1120642: type = 2; max = 9; chapter = 1003; break;
                case 1120643: type = 2; max = 9; chapter = 1004; break;
                case 1120644: type = 2; max = 9; chapter = 1005; break;

                // Sổ tay thám hiểm (Chap 1-5 loại vừa)
                case 1120645: type = 2; max = 16; chapter = 1001; break;
                case 1120646: type = 2; max = 16; chapter = 1002; break;
                case 1120647: type = 2; max = 16; chapter = 1003; break;
                case 1120648: type = 2; max = 16; chapter = 1004; break;
                case 1120649: type = 2; max = 16; chapter = 1005; break;

                // Sổ tay thám hiểm (Chap 1-5 loại lớn)
                case 1120650: type = 2; max = 25; chapter = 1001; break;
                case 1120651: type = 2; max = 25; chapter = 1002; break;
                case 1120652: type = 2; max = 25; chapter = 1003; break;
                case 1120653: type = 2; max = 25; chapter = 1004; break;
                case 1120654: type = 2; max = 25; chapter = 1005; break;
            }

            if (type == 2)
            {
                getJamps(client, count, max, chapter, ref value);
                client.Player.EquipBag.UpdatePlayerProperties();
                count = 1;
            }

            int jampsCurrency = value * count;
            client.Player.AddJampsCurrency(jampsCurrency);

            if (type == 1 || type == 2)
            {
                client.Player.SendMessage($"Bạn nhận được {jampsCurrency} điểm thám hiểm.");
            }

            return 1;
        }

        private void getJamps(GameClient client, int num, int max, int chapter, ref int value)
        {
            var unlockedChapters = new List<int>();
            var manualInfo = client.Player.PlayerCharacter.explorerManualInfo;

            for (int index = 0; index < num; ++index)
            {
                string msg = "";
                var chapterItem = chapter == -1 ? JampsManualMgr.getRandomChapter() : JampsManualMgr.getChapter(chapter);
                var randomPage = JampsManualMgr.getRandomPageFromChapter(chapterItem.ID, max);

                var page = new PagesInfo { activate = false, pageID = randomPage.ID };

                // Processing probability rolls
                if (JampsManualMgr.randNumber(1, 100) <= 60)
                {
                    value += 40;
                }
                else if (JampsManualMgr.randNumber(1, 100) <= 30 || manualInfo.activesPage.Count == 0)
                {
                    if (manualInfo.addPage(page))
                    {
                        msg = $"chúc mừng bạn nhận được sổ thám hiểm:{randomPage.Name}";
                        if (!unlockedChapters.Contains(chapterItem.ID))
                            unlockedChapters.Add(chapterItem.ID);
                    }
                    else
                    {
                        value += 60;
                    }
                }
                else if (JampsManualMgr.randNumber(1, 100) <= 15)
                {
                    var jampsDebris = chapter == -1
                        ? JampsManualMgr.getRandomDebrisFromPages(manualInfo.activesPage)
                        : JampsManualMgr.getRandomDebrisFromPages(manualInfo.activesPage, chapter);

                    var debris = new DebrisInfo
                    {
                        date = DateTime.Now,
                        ID = jampsDebris.ID,
                        pageID = jampsDebris.PageID,
                        chapterID = JampsManualMgr.getChapterIDFromDebrisID(jampsDebris.ID)
                    };

                    if (debris.chapterID == -1)
                    {
                        msg = $"không đạt{jampsDebris.Describe}";
                    }
                    else if (manualInfo.addDebris(debris))
                    {
                        msg = $"chúc mừng bạn nhận được sổ thám hiểm:{jampsDebris.Describe}";
                        if (!unlockedChapters.Contains(debris.chapterID))
                            unlockedChapters.Add(debris.chapterID);
                    }
                }
                else
                {
                    value += 25;
                }

                if (!string.IsNullOrEmpty(msg))
                    client.Player.SendMessage(msg);
            }

            var pkg = new GSPacketIn((short)63);
            pkg.WriteString("Sổ tay của Người khám phá");
            pkg.WriteInt(0);
            pkg.WriteInt(num);
            pkg.WriteInt(72);
            pkg.WriteInt(unlockedChapters.Count);

            foreach (int val in unlockedChapters)
                pkg.WriteInt(val);

            client.SendTCP(pkg);
        }
    }
}