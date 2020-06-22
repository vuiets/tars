using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewModdingAPI;

namespace TarsDrone.Framework.Core
{
	/// <summary>Base class for drone implementations</summary>
	internal abstract class BaseDrone : NPC, IDrone
	{
		/*********
		** Fields
		*********/
		/// <summary>Provides handy modding utilities.</summary>
		private readonly IModHelper Helper;

		/// <summary>Helps with logging.</summary>
		private readonly IMonitor Monitor;

		/// <summary>The mod configurations.</summary>
		private readonly ModConfig Config;

		/// <summary>?</summary>
		private float T;

		/****
		* Constants
		****/

		/// <summary>?</summary>
		private const float R = 80f;

		/// <summary>The x-offset from the drone's buddy.</summary>
		private const float OFFSET_X = 5f;

		/// <summary>The y-offset from the drone's buddy.</summary>
		private const float OFFSET_Y = 20f;

		/****
		** Accessor
		****/
		/// <summary>The velocity of the fired projectile.</summary>
		public float ProjectileVelocity { get; }

		/****
		** Overrides
		****/
		/// <summary>Whether the drone can socialize.</summary>
		public override bool CanSocialize => false;

		/*********
		** Public methods
		*********/
		/// <summary>Whether the drone can say a word or two.</summary>
		public override bool canTalk()
		{
			return false;
		}

		/// <summary>Emote performed by the drone.</summary>
		/// <param name="whichEmote">The emote.</param>>
		/// <param name="playSound">Sound played along with emote.</param>>
		/// <param name="nextEventCommand">Whether to perform the emote in the next event command.</param>>
		public override void doEmote(int whichEmote, bool playSound, bool nextEventCommand = true)
		{
			// do nothing
		}

		/// <summary>Draw the drone sprite.</summary>
		/// <param name="spriteBatch">The sprite batch being drawn.</param>>
		public override void draw(SpriteBatch spriteBatch)
		{
			base.draw(spriteBatch);
		}

		/// <summary>Update to perform on each tick.</summary>
		/// <param name="time">The current game time.</param>
		/// <param name="location">The current location.</param>
		public override void update(GameTime time, GameLocation location)
		{
			// update drone's position
			this.Move(time);

			if (this.Config.Active)
			{
				this.OnUpdate(time, location);
			}
		}

		/// <summary>Action performed on each update.</summary>
		/// <param name="time">The current game time.</param>
		/// <param name="location">The current location.</param>
		public abstract void OnUpdate(GameTime time, GameLocation location);

		/*********
		** Protected Methods
		*********/
		protected BaseDrone()
		{
		}

		/// <summary>Construct an instance.</summary>
		/// <param name="config">The mod configurations.</param>
		/// <param name="modHelper">Provides modding utilities.</param>
		/// <param name="monitor">Helps with logging.</param>
		/// <param name="npcOptions">Options to instantiate a npc.</param>
		protected BaseDrone(
			ModConfig config,
			IModHelper modHelper,
			IMonitor monitor,
			NPCOptions npcOptions
		)
		: base(
			npcOptions.Sprite,
			npcOptions.Position,
			npcOptions.FacingDirection,
			npcOptions.Name
		)
		{
			this.Config = config;
			this.Helper = modHelper;
			this.Monitor = monitor;

			// initialize drone properties
			this.speed = this.Config.RotationSpeed;
			this.ProjectileVelocity = this.Config.ProjectileVelocity;
		}


		/// <summary>Move the drone on every tick.</summary>
		/// <param name="time">The current game time.</param>
		protected virtual void Move(GameTime time)
		{
			// make drone fly around the player
			float newX = Game1.player.position.X + OFFSET_X + R * (float)Math.Cos(T * 2 * Math.PI);
			float newY = Game1.player.position.Y - OFFSET_Y + R * (float)Math.Sin(T * 2 * Math.PI);

			this.position.Set(new Vector2(newX, newY));
			this.T = (this.T + (float)time.ElapsedGameTime.TotalMilliseconds / (1000 * speed)) % 1;
		}
	}

	// TODO: find a place and name for NPCOptions
	/// <summary>Options to instantiate a npc.</summary>
	internal sealed class NPCOptions
	{
		/*********
		** Fields
		*********/
		/****
		** Accessors
		****/
		/// <summary>The sprite.</summary>
		public AnimatedSprite Sprite { get; }

		/// <summary>The position on the grid.</summary>
		public Vector2 Position { get; }

		/// <summary>Direction faced.</summary>
		public int FacingDirection { get; }

		/// <summary>Name of NPC.</summary>
		public string Name { get; }

		/// <summary>Constructs an instance.</summary>
		/// <param name="sprite">The sprite.</param>
		/// <param name="position">The position in the world.</param>
		/// <param name="facingDirection">Direction faced.</param>
		/// <param name="name">Name of NPC.</param>
		public NPCOptions(
			AnimatedSprite sprite,
			Vector2 position,
			int facingDirection,
			string name
		)
		{
			this.Sprite = sprite;
			this.FacingDirection = facingDirection;
			this.Position = position;
			this.Name = name;
		}
	}
}
