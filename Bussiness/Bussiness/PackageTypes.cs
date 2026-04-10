namespace Bussiness
{
    public enum eWorldBossPackageType
    {
        OPEN = 0,
        OVER = 1,
        CANENTER = 2,
        ENTER = 3,
        WORLDBOSS_EXIT = 4,
        WORLDBOSS_BLOOD_UPDATE = 5,
        MOVE = 6,
        WORLDBOSS_PLAYERSTAUTSUPDATE = 7,
        WORLDBOSS_FIGHTOVER = 8,
        WORLDBOSS_ROOM_CLOSE = 9,
        WORLDBOSS_RANKING = 10,
        WORLDBOSS_PLAYER_REVIVE = 11,
        WORLDBOSS_BUYBUFF = 12
    }

    public enum eWorldBossPackageRecvType
    {
        ENTER_WORLDBOSSROOM = 32,
        LEAVE_ROOM = 33,
        ADDPLAYERS = 34,
        MOVE = 35,
        STAUTS = 36,
        REQUEST_REVIVE = 37,
        BUFF_BUY = 38
    }

    public enum eServerCmdType
    {
        WORLDBOSS_ALLOVER = 78,
        WORLDBOSS_UPDATE_BLOOD = 79,
        WORLDBOSS_UPDATE_INFO = 80,
        WORLDBOSS_UPDATE_RANK = 81,
        WORLDBOSS_FIGHTOVER = 82,
        WORLDBOSS_CLOSE = 83,
        WORLDBOSS_REDUCE_BLOOD = 84,
        WORLDBOSS_PRIVATE_INFO = 85,
        WORLDBOSS_SHOW_RANK = 86,
    }
}