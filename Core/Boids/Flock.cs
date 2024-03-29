using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace JadeFables.Core.Boids
{
    public class Flock : ComponentManager<Fish>
    {
        public Texture2D[] FlockTextures { get; set; }
        public int FishCount => Objects.Count;

        public float FlockScale;
        public int MaxFish;

        private const int SimulationDistance = 2500;

        public Flock(Texture2D[] texs, float Scale = 1, int MaxFlockSize = 60)
        {
            if (texs != null) FlockTextures = texs;
            else FlockTextures = new Texture2D[] { TextureAssets.MagicPixel.Value };

            FlockScale = Scale;
            MaxFish = MaxFlockSize;
        }

        internal void Populate(Vector2 position, int amount, float spread)
        {
            for (int i = 0; i < amount; i++)
            {
                if (Objects.Count < MaxFish)
                {
                    var fish = new Fish(this)
                    {
                        position = position + new Vector2(Main.rand.NextFloat(-spread, spread), Main.rand.NextFloat(-spread, spread)),
                        velocity = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1))
                    };

                    Objects.Add(fish);
                }
            }
        }

        protected override void OnUpdate()
        {
            foreach (Fish fish in Objects.ToArray())
            {
                if (fish != null)
                {
                    fish.AdjFish.Clear();

                    foreach (Fish adjfish in Objects)
                    {
                        if (!fish.Equals(adjfish) && Vector2.DistanceSquared(fish.position, adjfish.position) < Fish.Vision * Fish.Vision)
                            fish.AdjFish.Add(adjfish);
                    }

                    if (fish.CanDespawn || Vector2.DistanceSquared(fish.position, Main.LocalPlayer.Center) > SimulationDistance * SimulationDistance)
                        Objects.Remove(fish);
                }
            }
        }

        public void TryCatchFish(Rectangle hitbox, out List<Fish> caughtFish)
        {
            caughtFish = new List<Fish>();
            foreach (Fish fish in Objects.ToArray())
            {
                if (hitbox.Contains((int)fish.position.X, (int)fish.position.Y))
                {
                    Objects.Remove(fish);
                    caughtFish.Add(fish);
                }
            }
        }
    }
}
