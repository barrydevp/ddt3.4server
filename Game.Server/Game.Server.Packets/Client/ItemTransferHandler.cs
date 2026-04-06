using System;
using System.Text;
using Bussiness;
using Bussiness.Managers;
using Game.Base.Packets;
using Game.Server.GameUtils;
using Game.Server.Managers;
using SqlDataProvider.Data;

namespace Game.Server.Packets.Client
{
    [PacketHandler((int)ePackageType.ITEM_TRANSFER, "物品转移")]
    public class ItemTransferHandler : IPacketHandler
    {
        //修改:  XiaoJian
        //时间:  2020-11-7
        //描述:  物品转移<测试完成>   
        //状态： 正在使用

        public int HandlePacket(GameClient client, GSPacketIn packet)
        {
            GSPacketIn pkg = packet.Clone();
            pkg.ClearContext();
            new StringBuilder();
            int requiredGold = 10000;
            bool tranHole = packet.ReadBoolean();
            bool tranHoleFivSix = packet.ReadBoolean();
            ItemInfo itemZero = client.Player.StoreBag.GetItemAt(0);
            ItemInfo itemOne = client.Player.StoreBag.GetItemAt(1);
            if (itemZero != null && itemOne != null && itemZero.Template.CategoryID == itemOne.Template.CategoryID && itemOne.Count == 1 && itemZero.Count == 1)
            {
                if (client.Player.PlayerCharacter.Gold < requiredGold)
                {
                    client.Out.SendMessage(eMessageType.Normal, LanguageMgr.GetTranslation("ItemTransferHandler.NoGold"));
                    return 1;
                }
                client.Player.RemoveGold(requiredGold);
                StrengthenMgr.InheritTransferProperty(ref itemZero, ref itemOne, tranHole, tranHoleFivSix);
                int m_itemZero = OrginWeaponID(itemZero);
                int m_itemOne = OrginWeaponID(itemOne);
                ItemTemplateInfo newItemZero = null;
                ItemTemplateInfo newItemOne = null;
                if (m_itemZero > 0)
                {
                    newItemZero = ItemMgr.FindItemTemplate(GainWeaponID(itemZero.StrengthenLevel, m_itemZero));
                }
                if (m_itemOne > 0)
                {
                    newItemOne = ItemMgr.FindItemTemplate(GainWeaponID(itemOne.StrengthenLevel, m_itemOne));
                }
                //OLD if (TransferCondition(newItem, ordItem) && temOrd != null && temNew != null)
                //NEW: This new fix allows the transform to happen even if one of the tranformed items doesn't exists
                if (TransformCondition(itemOne, itemZero))
                {
                    if (newItemZero != null)
                    {
                        ItemInfo item = ItemInfo.CloneFromTemplate(newItemZero, itemZero);
                        ItemInfo.OpenHole(ref item);
                        if (item.isGold)
                        {
                            GoldEquipTemplateInfo info = GoldEquipMgr.FindGoldEquipByTemplate(newItemZero.TemplateID);
                            if (info != null)
                            {
                                ItemTemplateInfo iteminfoC = ItemMgr.FindItemTemplate(info.NewTemplateId);
                                if (iteminfoC != null)
                                {
                                    item.GoldEquip = iteminfoC;
                                }
                            }
                        }
                        client.Player.StoreBag.RemoveItemAt(0);
                        client.Player.StoreBag.AddItemTo(item, 0);
                    } else
                    {
                        client.Player.StoreBag.UpdateItem(itemZero);
                    }

                    if (newItemOne != null)
                    {
                        ItemInfo item2 = ItemInfo.CloneFromTemplate(newItemOne, itemOne);
                        ItemInfo.OpenHole(ref item2);
                        if (item2.isGold)
                        {
                            GoldEquipTemplateInfo goldEquipTemplateInfo2 = GoldEquipMgr.FindGoldEquipByTemplate(newItemOne.TemplateID);
                            if (goldEquipTemplateInfo2 != null)
                            {
                                ItemTemplateInfo itemTemplateInfo4 = ItemMgr.FindItemTemplate(goldEquipTemplateInfo2.NewTemplateId);
                                if (itemTemplateInfo4 != null)
                                {
                                    item2.GoldEquip = itemTemplateInfo4;
                                }
                            }
                        }
                        client.Player.StoreBag.RemoveItemAt(1);
                        client.Player.StoreBag.AddItemTo(item2, 1);
                    } else
                    {
                        client.Player.StoreBag.UpdateItem(itemOne);
                    }
                }
                else
                {
                    client.Player.StoreBag.UpdateItem(itemZero);
                    client.Player.StoreBag.UpdateItem(itemOne);
                }
                pkg.WriteByte(0);
                client.SendTCP(pkg);
            }
            else
            {
                client.Out.SendMessage(eMessageType.Normal, LanguageMgr.GetTranslation("itemtransferhandler.nocondition"));
            }
            return 0;
        }

        public bool TransformCondition(ItemInfo itemAtZero, ItemInfo itemAtOne)
        {
            if (itemAtZero.Template.CategoryID != 7 && itemAtOne.Template.CategoryID != 7)
            {
                return false;
            }
            if (itemAtZero.StrengthenLevel < 10 && itemAtOne.StrengthenLevel < 10)
            {
                return false;
            }
            return true;
        }

        private int OrginWeaponID(ItemInfo m_item)
        {
            StrengthenGoodsInfo info = StrengthenMgr.FindTransferInfo(m_item.TemplateID);
            if (info == null)
            {
                GoldEquipTemplateInfo goldEquip = GoldEquipMgr.FindGoldEquipOldTemplate(m_item.TemplateID);
                if (goldEquip != null)
                {
                    info = StrengthenMgr.FindTransferInfo(goldEquip.OldTemplateId);
                }
                else
                {
                    return m_item.TemplateID;
                }
            }

            if (info == null || info.Level < m_item.StrengthenLevel)
            {
                return m_item.TemplateID;
            }
            else
            {
                return info.OrginEquip;
            }
            #region Old
            /*StrengthenGoodsInfo strengthenGoodsInfo = StrengthenMgr.FindTransferInfo(m_item.TemplateID);
            if (strengthenGoodsInfo == null)
            {
                GoldEquipTemplateInfo info = GoldEquipMgr.FindGoldEquipOldTemplate(m_item.TemplateID);
                if (info == null)
                {
                    return 0;
                }
                strengthenGoodsInfo = StrengthenMgr.FindTransferInfo(info.OldTemplateId);
            }
            return strengthenGoodsInfo.OrginEquip;*/
            #endregion
        }

        private int GainWeaponID(int strengthenLEvel, int transId)
        {
            StrengthenGoodsInfo info;
            if(strengthenLEvel >= 10 && strengthenLEvel <= 15)
            {
                info = StrengthenMgr.FindTransferInfo(strengthenLEvel, transId);
                if (info == null)
                    return -1;
                return info.GainEquip;
            }
            else
            {
                info = StrengthenMgr.FindTransferInfo(transId);
                if (info == null)
                    return 0;
                if(info.Level < strengthenLEvel)
                {
                    return transId;
                }
                else
                {
                    return info.OrginEquip;
                }
            }
            #region Old
            /*if (m_item.StrengthenLevel >= 10)
            {
                return StrengthenMgr.FindTransferInfo(m_item.StrengthenLevel, m_item.TemplateID)?.GainEquip ?? (-1);
            }
            return StrengthenMgr.FindTransferInfo(m_item.TemplateID)?.OrginEquip ?? (-1);*/
            #endregion
        }
    }
}
