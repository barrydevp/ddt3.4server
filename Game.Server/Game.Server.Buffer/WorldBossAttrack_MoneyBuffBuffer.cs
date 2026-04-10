using Game.Server.GameObjects;
using SqlDataProvider.Data;

namespace Game.Server.Buffer
{
    public class WorldBossAttrack_MoneyBuffBuffer : AbstractBuffer
    {
        public static int MAX_COUNT = 20;

        public WorldBossAttrack_MoneyBuffBuffer(BufferInfo buffer) : base(buffer)
        {
        }

        public override void Start(GamePlayer player)
        {
            if (player.BufferList.GetOfType(typeof(WorldBossAttrack_MoneyBuffBuffer)) is WorldBossAttrack_MoneyBuffBuffer buffer)
            {
                buffer.Info.ValidDate = m_info.ValidDate;
                player.BufferList.UpdateBuffer(buffer);
                BufferInfo appliedBuff = null;
                for (int i = 0; i < player.FightBuffs.Count; i++)
                {
                    if (player.FightBuffs[i].Type == m_info.Type)
                    {
                        appliedBuff = player.FightBuffs[i];
                        if (appliedBuff.ValidCount < MAX_COUNT)
                        {
                            player.FightBuffs[i].Value += m_info.Value;
                            player.FightBuffs[i].ValidCount++;
                            break;
                        }
                    }
                }
                // in case the buff is not found, add a new one
                if (appliedBuff == null) player.FightBuffs.Add(Info);
            }
            else
            {
                base.Start(player);
                player.FightBuffs.Add(Info);
            }
        }

        public override void Stop()
        {
            m_player.FightBuffs.Remove(Info);
            base.Stop();
        }
    }
}