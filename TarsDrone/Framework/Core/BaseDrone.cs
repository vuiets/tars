using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewModdingAPI;

namespace TarsDrone.Framework.Core
{
	internal abstract class BaseDrone : NPC, IDrone
	{
		/*********
		** Fields
		*********/
		private readonly IModHelper Helper;
		private readonly ModConfig Config;
		private readonly IMonitor Monitor;
		private float T;
		private int Damage;
		private readonly float ProjectileVelocity;

		/****
		* Constants
		****/
		private const float R = 80f;
		private const float OFFSET_X = 5f;
		private const float OFFSET_Y = 20f;

		public override bool CanSocialize => false;

		public override bool canTalk()
		{
			return false;
		}

		public override void doEmote(int whichEmote, bool playSound, bool nextEventCommand = true)
		{
			// do nothing
		}

		public override void update(GameTime time, GameLocation location)
		{
			// update drone's position
			this.Move(time);

			if (this.Config.Active)
			{
				this.OnUpdate(time, location);
			}
		}

		public abstract void OnUpdate(GameTime time, GameLocation location);

		/*********
		** Protected Methods
		*********/
		protected BaseDrone(ModConfig config, IModHelper modHelper, IMonitor monitor)
		{
			this.Config = config;
			this.Helper = modHelper;
			this.Monitor = monitor;

			// initialize drone properties
			this.speed = this.Config.RotationSpeed;
			this.Damage = this.Config.Damage;
			this.ProjectileVelocity = this.Config.ProjectileVelocity;
		}

		protected virtual void Move(GameTime time)
		{
			// make drone fly around the player
			float newX = Game1.player.position.X + OFFSET_X + R * (float)Math.Cos(T * 2 * Math.PI);
			float newY = Game1.player.position.Y - OFFSET_Y + R * (float)Math.Sin(T * 2 * Math.PI);

			this.position.Set(new Vector2(newX, newY));
			this.T = (this.T + (float)time.ElapsedGameTime.TotalMilliseconds / (1000 * speed)) % 1;
		}
	}
}
