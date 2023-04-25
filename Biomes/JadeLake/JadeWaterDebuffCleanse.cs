using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace JadeFables.Biomes.JadeLake
{
    public class JadeLakeDebuffCleanse : ModPlayer
    {
        public override void PostUpdate()
        {
            if (Player.InModBiome<JadeLakeBiome>() && Player.wet)
            {
                for (int i = 0; i < Player.MaxBuffs; i++)
                {
                    int type = Player.buffType[i];
                    if (Main.debuff[type] && !BuffID.Sets.NurseCannotRemoveDebuff[type] && type != BuffID.TheTongue && Player.buffTime[i] > 1)
                        Player.buffTime[i]--;
                }
            }
        }
    }
}