using Bussiness;
using Bussiness.CenterService;
using Bussiness.Managers;
using Game.Base;
using Game.Base.Packets;
using Game.Server;
using Game.Server.Buffer;
using Game.Server.GameObjects;
using Game.Server.Managers;
using Game.Server.Packets;
using SqlDataProvider.Data;
using System;
using System.Collections.Generic;

namespace Game.Server.Rooms
{
    public class WorldBossBuffInfo
    {
        public int ID { get; set; }

        public BuffType Type { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        public int Value { get; set; }

        public int MaxCount { get; set; }

        public string Description { get; set; }

        public WorldBossBuffInfo(int id, BuffType type, string name, int price, int value, int maxCount, string description)
        {
            this.ID = id;
            this.Type = type;
            this.Name = name;
            this.Price = price;
            this.Value = value;
            this.MaxCount = maxCount;
            this.Description = description;
        }
    }

    public class BaseWorldBossRoom
    {
        public Dictionary<int, WorldBossBuffInfo> buyableBuffs = new Dictionary<int, WorldBossBuffInfo>();

        private Dictionary<int, GamePlayer> m_list;

        private long MAX_BLOOD = (long)350000000;

        private long m_blood;

        private string m_name;

        private string m_bossResourceId;

        private DateTime m_begin_time;

        private DateTime m_end_time;

        private int m_currentPVE;

        private bool m_fightOver;

        private bool m_roomClose;

        private bool m_worldOpen;

        private int m_fight_time;

        private bool m_die;

        public int playerDefaultPosX = 265;

        public int playerDefaultPosY = 1030;

        public int ticketID = 11573;

        public int need_ticket_count;

        public int timeCD = 15;

        public int reviveMoney = 500;

        public int reFightMoney = 600;

        public int addInjureBuffMoney = 100;

        public int addInjureValue = 200;

        public DateTime Begin_time
        {
            get
            {
                return this.m_begin_time;
            }
        }

        public long Blood
        {
            get
            {
                return this.m_blood;
            }
            set
            {
                this.m_blood = value;
            }
        }

        public string BossResourceId
        {
            get
            {
                return this.m_bossResourceId;
            }
        }

        public int CurrentPVE
        {
            get
            {
                return this.m_currentPVE;
            }
        }

        public DateTime End_time
        {
            get
            {
                return this.m_end_time;
            }
        }

        public int Fight_time
        {
            get
            {
                return this.m_fight_time;
            }
        }

        public bool FightOver
        {
            get
            {
                return this.m_fightOver;
            }
        }

        public bool IsDie
        {
            get
            {
                return this.m_die;
            }
            set
            {
                this.m_die = value;
            }
        }

        public long MaxBlood
        {
            get
            {
                return this.MAX_BLOOD;
            }
        }

        public string Name
        {
            get
            {
                return this.m_name;
            }
        }

        public bool RoomClose
        {
            get
            {
                return this.m_roomClose;
            }
        }

        public bool WorldbossOpen
        {
            get
            {
                return this.m_worldOpen;
            }
        }

        public BaseWorldBossRoom()
        {
            this.m_list = new Dictionary<int, GamePlayer>();
            this.m_name = "Boss";
            this.m_bossResourceId = "0";
            this.m_currentPVE = 0;
            this.buyableBuffs.Add(1, new WorldBossBuffInfo(
                1, 
                BuffType.WorldBossAttrack_MoneyBuff, 
                "Tăng Sát Thương", 
                100, 
                200,
                WorldBossAttrack_MoneyBuffBuffer.MAX_COUNT,
                "Sát thương cơ bản tăng 200"
            ));
            this.buyableBuffs.Add(2, new WorldBossBuffInfo(
                2,
                BuffType.WorldBossHP_MoneyBuff,
                "Tăng Máu",
                100,
                5000,
                WorldBossHP_MoneyBuffBuffer.MAX_COUNT,
                "Máu tăng 5000"
            ));
        }

        public bool AddPlayer(GamePlayer player)
        {
            bool flag = false;
            lock (this.m_list)
            {
                if (!this.m_list.ContainsKey(player.PlayerId))
                {
                    player.IsInWorldBossRoom = true;
                    this.m_list.Add(player.PlayerId, player);
                    flag = true;
                    this.ShowRank();
                    this.SendPrivateInfoToCenter(player.PlayerCharacter.NickName);
                }
            }
            if (flag)
            {
                GSPacketIn gSPacketIn = new GSPacketIn((int)ePackageType.WORLD_BOSS);
                gSPacketIn.WriteByte((byte)eWorldBossPackageType.ENTER);
                gSPacketIn.WriteInt(player.PlayerCharacter.Grade);
                gSPacketIn.WriteInt(player.PlayerCharacter.Hide);
                gSPacketIn.WriteInt(player.PlayerCharacter.Repute);
                gSPacketIn.WriteInt(player.PlayerCharacter.ID);
                gSPacketIn.WriteString(player.PlayerCharacter.NickName);
                gSPacketIn.WriteByte(player.PlayerCharacter.typeVIP);
                gSPacketIn.WriteInt(player.PlayerCharacter.VIPLevel);
                gSPacketIn.WriteBoolean(player.PlayerCharacter.Sex);
                gSPacketIn.WriteString(player.PlayerCharacter.Style);
                gSPacketIn.WriteString(player.PlayerCharacter.Colors);
                gSPacketIn.WriteString(player.PlayerCharacter.Skin);
                gSPacketIn.WriteInt(player.X);
                gSPacketIn.WriteInt(player.Y);
                gSPacketIn.WriteInt(player.PlayerCharacter.FightPower);
                gSPacketIn.WriteInt(player.PlayerCharacter.Win);
                gSPacketIn.WriteInt(player.PlayerCharacter.Total);
                gSPacketIn.WriteInt(player.PlayerCharacter.Offer);
                gSPacketIn.WriteByte(player.States);
                this.SendToALL(gSPacketIn);
            }
            return flag;
        }

        public void FightOverAll()
        {
            GSPacketIn gSPacketIn = new GSPacketIn((int)eServerCmdType.WORLDBOSS_FIGHTOVER);
            GameServer.Instance.LoginServer.SendPacket(gSPacketIn);
        }

        public GamePlayer[] GetPlayersSafe()
        {
            GamePlayer[] gamePlayerArray = null;
            lock (this.m_list)
            {
                gamePlayerArray = new GamePlayer[this.m_list.Count];
                this.m_list.Values.CopyTo(gamePlayerArray, 0);
            }
            if (gamePlayerArray != null)
            {
                return gamePlayerArray;
            }
            return new GamePlayer[0];
        }

        public void ReduceBlood(int value)
        {
            GSPacketIn gSPacketIn = new GSPacketIn((int)eServerCmdType.WORLDBOSS_REDUCE_BLOOD);
            gSPacketIn.WriteInt(value);
            GameServer.Instance.LoginServer.SendPacket(gSPacketIn);
        }

        public bool RemovePlayer(GamePlayer player)
        {
            bool flag = false;
            lock (this.m_list)
            {
                flag = this.m_list.Remove(player.PlayerId);
                GSPacketIn gSPacketIn = new GSPacketIn(102);
                gSPacketIn.WriteByte(4);
                gSPacketIn.WriteInt(player.PlayerId);
                this.SendToALL(gSPacketIn);
            }
            if (flag)
            {
                player.Out.SendSceneRemovePlayer(player);
            }
            return true;
        }

        public void WorldBossFightOver()
        {
            m_fightOver = true;

            GSPacketIn gSPacketIn = new GSPacketIn((int)ePackageType.WORLD_BOSS);
            gSPacketIn.WriteByte((byte)eWorldBossPackageType.WORLDBOSS_FIGHTOVER);
            gSPacketIn.WriteBoolean(m_die);
            this.SendToALLPlayers(gSPacketIn);
        }

        public void SendAllOver()
        {
            GSPacketIn gSPacketIn = new GSPacketIn((int)ePackageType.WORLD_BOSS);
            gSPacketIn.WriteByte((byte)eWorldBossPackageType.OVER);
            this.SendToALLPlayers(gSPacketIn);
        }

        public void SendPrivateInfoToCenter(string name)
        {
            GSPacketIn gSPacketIn = new GSPacketIn(85);
            gSPacketIn.WriteString(name);
            GameServer.Instance.LoginServer.SendPacket(gSPacketIn);
        }

        public void SendPrivateInfo(string name, int damage, int honor)
        {
            GSPacketIn gSPacketIn = new GSPacketIn((int)ePackageType.WORLD_BOSS);
            gSPacketIn.WriteByte(22);
            gSPacketIn.WriteInt(damage);
            gSPacketIn.WriteInt(honor);
            GamePlayer[] playersSafe = this.GetPlayersSafe();
            for (int i = 0; i < (int)playersSafe.Length; i++)
            {
                GamePlayer gamePlayer = playersSafe[i];
                if (gamePlayer.PlayerCharacter.NickName == name)
                {
                    gamePlayer.Out.SendTCP(gSPacketIn);
                    return;
                }
            }
        }

        public void SendRoomClose()
        {
            GSPacketIn gSPacketIn = new GSPacketIn((int)ePackageType.WORLD_BOSS);
            gSPacketIn.WriteByte((byte)eWorldBossPackageType.WORLDBOSS_ROOM_CLOSE);
            this.SendToALLPlayers(gSPacketIn);
        }

        public void SendToALL(GSPacketIn packet)
        {
            this.SendToALL(packet, null);
        }

        public void SendToALL(GSPacketIn packet, GamePlayer except)
        {
            GamePlayer[] gamePlayerArray = null;
            lock (this.m_list)
            {
                gamePlayerArray = new GamePlayer[this.m_list.Count];
                this.m_list.Values.CopyTo(gamePlayerArray, 0);
            }
            if (gamePlayerArray != null)
            {
                GamePlayer[] gamePlayerArray1 = gamePlayerArray;
                for (int i = 0; i < (int)gamePlayerArray1.Length; i++)
                {
                    GamePlayer gamePlayer = gamePlayerArray1[i];
                    if (gamePlayer != null && gamePlayer != except)
                    {
                        gamePlayer.Out.SendTCP(packet);
                    }
                }
            }
        }

        public void SendToALLPlayers(GSPacketIn packet)
        {
            GamePlayer[] allPlayers = WorldMgr.GetAllPlayers();
            for (int i = 0; i < (int)allPlayers.Length; i++)
            {
                allPlayers[i].SendTCP(packet);
            }
        }

        public void SendUpdateBlood(GSPacketIn packet)
        {
            long num = packet.ReadLong();
            this.m_blood = packet.ReadLong();
            GSPacketIn gSPacketIn = new GSPacketIn((int)ePackageType.WORLD_BOSS);
            gSPacketIn.WriteByte((byte)eWorldBossPackageType.WORLDBOSS_BLOOD_UPDATE);
            gSPacketIn.WriteBoolean(false);
            gSPacketIn.WriteLong(num);
            gSPacketIn.WriteLong(this.m_blood);
            Console.WriteLine("Boss Blood Update:" + this.m_blood);
            this.SendToALL(gSPacketIn);
        }

        public void ShowRank()
        {
            GSPacketIn gSPacketIn = new GSPacketIn((int)eServerCmdType.WORLDBOSS_SHOW_RANK);
            GameServer.Instance.LoginServer.SendPacket(gSPacketIn);
        }

        public void UpdateRank(int damage, int honor, string nickName)
        {
            GSPacketIn gSPacketIn = new GSPacketIn((int)eServerCmdType.WORLDBOSS_UPDATE_RANK);
            gSPacketIn.WriteInt(damage);
            gSPacketIn.WriteInt(honor);
            gSPacketIn.WriteString(nickName);
            GameServer.Instance.LoginServer.SendPacket(gSPacketIn);
        }

        public void UpdateWorldBoss(GSPacketIn pkg)
        {
            long newMAX_BLOOD = pkg.ReadLong();
            long newBlood = pkg.ReadLong();
            string newName = pkg.ReadString();
            string newBossResourceId = pkg.ReadString();
            int newCurrentPVE = pkg.ReadInt();
            bool newFightOver = pkg.ReadBoolean();
            bool newRoomClose = pkg.ReadBoolean();
            DateTime newBeginTime = pkg.ReadDateTime();
            DateTime newEndTime = pkg.ReadDateTime();
            int newFightTime = pkg.ReadInt();
            bool newIsOpen = pkg.ReadBoolean();
            
            bool oldIsOpen = this.m_worldOpen;
            bool oldFightOver = this.m_fightOver;
            bool oldDie = this.m_die;

            this.m_begin_time = newBeginTime;
            this.m_end_time = newEndTime;
            this.m_begin_time = newBeginTime;
            this.m_fight_time = newFightTime;
            //this.m_die = newFightOver;
            this.m_fightOver = newFightOver;
            this.m_roomClose = newRoomClose;
            this.MAX_BLOOD = newMAX_BLOOD;
            this.m_blood = newBlood;
            this.m_name = newName;
            this.m_bossResourceId = newBossResourceId;
            this.m_currentPVE = newCurrentPVE;
            this.m_worldOpen = newIsOpen;

            //if (isOpenLast && this.m_fightOver && !this.m_die)
            //{
            //    this.FightOverAll();
            //    this.m_die = true;
            //}

            if (!oldIsOpen)
            {
                if (newIsOpen)
                {
                    GamePlayer[] allPlayers = WorldMgr.GetAllPlayers();
                    for (int i = 0; i < (int)allPlayers.Length; i++)
                    {
                        GamePlayer gamePlayer = allPlayers[i];
                        gamePlayer.Out.SendOpenWorldBoss(gamePlayer.X, gamePlayer.Y);
                    }
                }
            }
        }

        public void UpdateWorldBossRankCrosszone(GSPacketIn packet)
        {
            GSPacketIn gSPacketIn = new GSPacketIn((int)ePackageType.WORLD_BOSS);
            gSPacketIn.WriteByte((byte)eWorldBossPackageType.WORLDBOSS_RANKING);
            bool isEndRank = packet.ReadBoolean();
            int num = packet.ReadInt();
            gSPacketIn.WriteBoolean(isEndRank);
            gSPacketIn.WriteInt(num);
            for (int i = 0; i < num; i++)
            {
                int num1 = packet.ReadInt();
                string str = packet.ReadString();
                int num2 = packet.ReadInt();
                gSPacketIn.WriteInt(num1);
                gSPacketIn.WriteString(str);
                gSPacketIn.WriteInt(num2);
                if (this.m_fightOver && isEndRank)
                {
                    SendWorldBossRankAward(str, num1);
                }
            }
            if (isEndRank)
            {
                this.SendToALLPlayers(gSPacketIn);
                return;
            }
            this.SendToALL(gSPacketIn);
        }

        private void SendWorldBossRankAward(string name, int rank)
        {
            List<ItemInfo> items = new List<ItemInfo>();
            WorldBossTopTenAwardInfo goods = AwardMgr.GetWorldBossAwardByID(rank);
            ItemTemplateInfo temp = ItemMgr.FindItemTemplate(goods.TemplateID);
            if (temp == null)
                return;
            ItemInfo item = ItemInfo.CreateFromTemplate(temp, 1, 102);
            item.IsBinds = goods.IsBinds;
            item.ValidDate = goods.Validate;
            item.StrengthenLevel = goods.StrengthenLevel;
            item.AttackCompose = goods.AttackCompose;
            item.DefendCompose = goods.DefendCompose;
            item.LuckCompose = goods.LuckCompose;
            item.AgilityCompose = goods.AgilityCompose;
            items.Add(item);
            PlayerBussiness pb = new PlayerBussiness();
            PlayerInfo info = pb.GetUserSingleByNickName(name);
            pb.SendItemsToMail(items, info.ID, GameServer.Instance.Configuration.ZoneId, $"Quà chiến đấu với Boss Tà Diệm Long bạn đã đạt được hạng {rank}!.", "Quà Boss Thế Giới");
            CenterServiceClient cs = new CenterServiceClient();
            cs.MailNotice(info.ID);
        }

        public void ViewOtherPlayerRoom(GamePlayer player)
        {
            GamePlayer[] playersSafe = this.GetPlayersSafe();
            for (int i = 0; i < (int)playersSafe.Length; i++)
            {
                GamePlayer gamePlayer = playersSafe[i];
                if (gamePlayer != player)
                {
                    GSPacketIn gSPacketIn = new GSPacketIn((int)ePackageType.WORLD_BOSS);
                    gSPacketIn.WriteByte((byte)eWorldBossPackageType.ENTER);
                    gSPacketIn.WriteInt(gamePlayer.PlayerCharacter.Grade);
                    gSPacketIn.WriteInt(gamePlayer.PlayerCharacter.Hide);
                    gSPacketIn.WriteInt(gamePlayer.PlayerCharacter.Repute);
                    gSPacketIn.WriteInt(gamePlayer.PlayerCharacter.ID);
                    gSPacketIn.WriteString(gamePlayer.PlayerCharacter.NickName);
                    gSPacketIn.WriteByte(gamePlayer.PlayerCharacter.typeVIP);
                    gSPacketIn.WriteInt(gamePlayer.PlayerCharacter.VIPLevel);
                    gSPacketIn.WriteBoolean(gamePlayer.PlayerCharacter.Sex);
                    gSPacketIn.WriteString(gamePlayer.PlayerCharacter.Style);
                    gSPacketIn.WriteString(gamePlayer.PlayerCharacter.Colors);
                    gSPacketIn.WriteString(gamePlayer.PlayerCharacter.Skin);
                    gSPacketIn.WriteInt(gamePlayer.X);
                    gSPacketIn.WriteInt(gamePlayer.Y);
                    gSPacketIn.WriteInt(gamePlayer.PlayerCharacter.FightPower);
                    gSPacketIn.WriteInt(gamePlayer.PlayerCharacter.Win);
                    gSPacketIn.WriteInt(gamePlayer.PlayerCharacter.Total);
                    gSPacketIn.WriteInt(gamePlayer.PlayerCharacter.Offer);
                    gSPacketIn.WriteByte(gamePlayer.States);
                    player.SendTCP(gSPacketIn);
                }
            }
        }

        public void WorldBossClose()
        {
            this.m_worldOpen = false;
            GamePlayer[] playersSafe = this.GetPlayersSafe();
            for (int i = 0; i < (int)playersSafe.Length; i++)
            {
                this.RemovePlayer(playersSafe[i]);
            }
        }
    }
}