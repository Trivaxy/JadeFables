using JadeFables.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;

namespace JadeFables.Core.Boids
{
	public class Fish : Entity, IComponent
	{
		public Vector2 Acceleration { get; set; }
		public bool CanDespawn => Lifespan < 0 && SpawnTimer > SpawnTimerTicks;

		public const float Vision = 100;
		private const float MaxForce = 0.02f;
		private float MaxVelocity => !Wet() ? 6f : 2f;
		private const int SpawnTimerTicks = 100;
		private const int LifespanTicks = 2000;

		private int Frame = 0;
		private int TextureID = 0;
		private int SpawnTimer = 0;
		private int Lifespan = 0;

		private Flock parent;

		public List<Fish> AdjFish = new List<Fish>();

		public Fish(Flock flock, int texID = -1)
		{
			parent = flock;
			Frame = Main.rand.Next(9);
			TextureID = texID == -1 ? Main.rand.Next(flock.FlockTextures.Length) : 1;
			
			SpawnTimer = SpawnTimerTicks; // > 0 appearing CD, < 0 disappearing CD
			Lifespan = LifespanTicks;
		}

		Vector2 Limit(Vector2 vec, float val)
		{
			if (vec.LengthSquared() > val * val)
				return Vector2.Normalize(vec) * val;
			return vec;
		}

		public Vector2 AvoidTiles(int range) //WIP for Qwerty
		{
			Vector2 sum = new Vector2(0, 0);
			Point tilePos = position.ToTileCoordinates();

			const int TileRange = 2;

			for (int i = -TileRange; i < TileRange + 1; i++)
			{
				for (int j = -TileRange; j < TileRange + 1; j++)
				{
					if (WorldGen.InWorld(tilePos.X + i, tilePos.Y + j, 10))
					{
						Tile tile = Framing.GetTileSafely(tilePos.X + i, tilePos.Y + j);
						float pdist = Vector2.DistanceSquared(position, new Vector2(tilePos.X + i, tilePos.Y + j) * 16);
						if (pdist < range * range && pdist > 0 && ((tile.HasTile && Main.tileSolid[tile.TileType] && !tile.IsActuated) || tile.LiquidAmount < 100))
						{
							Vector2 d = position - new Vector2(tilePos.X + i, tilePos.Y + j) * 16;
							Vector2 norm = Vector2.Normalize(d);
							Vector2 weight = norm;
							sum += weight;
						}
					}
				}
			}

			if (sum != Vector2.Zero)
			{
				sum = Vector2.Normalize(sum) * MaxVelocity;
				Vector2 acc = sum - velocity;
				return Limit(acc, MaxForce);
			}
			return Vector2.Zero;
		}

		//Avoid you [Client Side]
		//TODO: Entity Pass, not client side maybe?
		public Vector2 AvoidHooman(int range)
		{
			float pdist = Vector2.DistanceSquared(position, Main.LocalPlayer.Center);
			Vector2 sum = new Vector2(0, 0);

			if (pdist < range * range && pdist > 0)
			{
				Vector2 d = position - Main.LocalPlayer.Center;
				Vector2 norm = Vector2.Normalize(d);
				Vector2 weight = norm;
				sum += weight;
			}

			if (sum != Vector2.Zero)
			{
				sum = Vector2.Normalize(sum) * MaxVelocity;
				Vector2 acc = sum - velocity;
				return Limit(acc, MaxForce);
			}
			return Vector2.Zero;
		}

		//Cant overlap
		public Vector2 Seperation(int range)
		{
			int count = 0;
			Vector2 sum = new Vector2(0, 0);
			for (int j = 0; j < AdjFish.Count; j++)
			{
				var OtherFish = AdjFish[j];
				float dist = Vector2.DistanceSquared(position, OtherFish.position);
				if (dist < range * range && dist > 0)
				{
					Vector2 d = position - OtherFish.position;
					Vector2 norm = Vector2.Normalize(d);
					Vector2 weight = norm / dist;
					sum += weight;
					count++;
				}
			}

			if (count > 0)
				sum /= count;

			if (sum != Vector2.Zero)
			{
				sum = Vector2.Normalize(sum) * MaxVelocity;
				Vector2 acc = sum - velocity;
				return Limit(acc, MaxForce);
			}
			return Vector2.Zero;
		}

		//Must face the same general direction
		public Vector2 Allignment(int range)
		{
			int count = 0;
			Vector2 sum = new Vector2(0, 0);
			for (int j = 0; j < AdjFish.Count; j++)
			{
				var OtherFish = AdjFish[j];
				float dist = Vector2.DistanceSquared(position, OtherFish.position);
				if (dist < range * range && dist > 0)
				{
					sum += OtherFish.velocity;
					count++;
				}
			}

			if (count > 0)
				sum /= count;

			if (sum != Vector2.Zero)
			{
				sum = Vector2.Normalize(sum) * MaxVelocity;
				Vector2 acc = sum - velocity;
				return Limit(acc, MaxForce);
			}
			return Vector2.Zero;
		}

		//Must stay close
		public Vector2 Cohesion(int range)
		{
			int count = 0;
			Vector2 sum = new Vector2(0, 0);
			for (int j = 0; j < AdjFish.Count; j++)
			{
				var OtherFish = AdjFish[j];
				float dist = Vector2.DistanceSquared(position, OtherFish.position);
				if (dist < range * range && dist > 0)
				{
					sum += OtherFish.position;
					count++;
				}
			}

			if (count > 0)
			{
				sum /= count;
				sum -= position;
				sum = Vector2.Normalize(sum) * MaxVelocity;
				Vector2 acc = sum - velocity;
				return Limit(acc, MaxForce);
			}
			return Vector2.Zero;
		}

		public void Draw(SpriteBatch spritebatch)
		{
			const int TOTALFRAMES = 9;
			Point point = position.ToTileCoordinates();
			Color lightColour = Lighting.GetColor(point.X, point.Y);
			
			//Reuse alpha transition for (de)spawning
			_ = Lifespan > 0 ? SpawnTimer-- : SpawnTimer++;
			float alpha = MathHelper.Clamp(1 - (SpawnTimer / (float) SpawnTimerTicks), 0f, 1f);
			Texture2D texture = parent.FlockTextures[TextureID];

			Rectangle source = new Rectangle(0, (texture.Height / TOTALFRAMES) * ((Frame / 4) % TOTALFRAMES), texture.Width, texture.Height / TOTALFRAMES);

			spritebatch.Draw(texture, position - Main.screenPosition, source,
				lightColour * alpha, velocity.ToRotation() + (float)Math.PI, new Vector2(texture.Width * 0.5f, (texture.Height / TOTALFRAMES) * 0.5f),
				parent.FlockScale, SpriteEffects.None, 0f);
		}

		public void ApplyForces()
		{
			velocity += Acceleration;
			velocity = Limit(velocity, MaxVelocity);
            velocity = TileCollision(position, velocity);
            position += velocity;
			Acceleration *= 0;
        }

		public void Update()
		{
			//If fish ded, reset SpawnTimer to fade away into oblivion
			if (Lifespan == 0) {
				SpawnTimer = 0;
			}

			//arbitrarily weight
			Acceleration += Seperation(25) * 1.5f;
			Acceleration += Allignment(50) * 1f;
			Acceleration += Cohesion(50) * 1f;
			Acceleration += AvoidHooman(50) * 4f;
			Acceleration += AvoidTiles(100) * 5f;

			if (!Wet())
			{
				Acceleration = new Vector2(0, 0.1f);
			}
			ApplyForces();

			if (Main.rand.NextBool(4000))
			{
				Projectile proj = Projectile.NewProjectileDirect(GetSource_Misc("Boidfish"), position, -Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.75f, 2f), ModContent.ProjectileType<AirBubble>(), 100, 0);
				proj.scale = Main.rand.NextFloat(0.1f, 0.3f);
			}
			Frame++;
			
			//Should lifespan only be decremented after (de)spawning transition?
			Lifespan--;
		}

        public Vector2 TileCollision(Vector2 pos, Vector2 vel)
        {
            Vector2 newVel = Collision.noSlopeCollision(pos - (new Vector2(8, 8)), vel, 16, 16, true, true);
            Vector2 ret = new Vector2(vel.X, vel.Y);
            if (Math.Abs(newVel.X) < Math.Abs(vel.X))
                ret.X *= 0;

			if (Math.Abs(newVel.Y) < Math.Abs(vel.Y))
			{
				if (!Wet())
				{
					if (ret.Y > 0)
					{
                        ret.X = Main.rand.NextFloat(-2, 2);
                        ret.Y *= -1;
						ret.Y = MathHelper.Clamp(ret.Y, -1, 1);
					}
				}
				else
                    ret.Y *= 0;
            }

            return ret;
        }

		public bool Wet()
		{
			Tile tile = Framing.GetTileSafely((int)position.X / 16, (int)position.Y / 16);
			return tile.LiquidAmount > 100;
		}
    }
}
