using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Tools;
using StardewModdingAPI;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using TarsDrone.Framework.Pods.Core;
using TarsDrone.Framework.Config;
using StarObject = StardewValley.Object;
using CollisionBehavior = StardewValley.Projectiles.BasicProjectile.onCollisionBehavior;

namespace TarsDrone.Framework.Pods
{
	internal class BattlePod: BasePod
	{
		/*********
		** Fields
		*********/
		/// <summary>The attachment settings.</summary>
		private readonly BattleConfig Config;

		private int Damage;

		/****
		** State
		****/
		private Monster Target;

		/****
		** Constants
		****/
		private readonly int MAX_DAMAGE = -1;
		private const string COLLISION_SOUND = "hitEnemy";
		private const string FIRING_SOUND = "daggerswipe";

		/*********
		** Public methods
		*********/
		/// <summary>Construct an instance.</summary>
		/// <param name="config">The attachment settings.</param>
		/// <param name="modHelper">Fetches metadata about loaded mods.</param>
		/// <param name="monitor">Simplifies access to private code.</param>
		public BattlePod(
			BattleConfig config,
			IModHelper modHelper,
			IMonitor monitor)
			: base(modHelper, monitor)
		{
			this.Config = config;
			this.Damage = config.Damage;
		}

		/// <summary>Get whether the pod is currently enabled.</summary>
		/// <param name="buddy">The current player who owns this drone.</param>
		/// <param name="tool">The tool selected by the player (if any).</param>
		/// <param name="item">The item selected by the player (if any).</param>
		/// <param name="location">The current location.</param>
		public override bool IsEnabled(
			Farmer buddy,
			Tool tool,
			Item item,
			GameLocation location
		)
		{
			return true;
		}

		/// <summary>Act on the given tile.</summary>
		/// <param name="tileObj">The object on the tile.</param>
		/// <param name="buddy">The current player who owns this drone.</param>
		/// <param name="buddyTool">The tool selected by the player (if any).</param>
		/// <param name="item">The item selected by the player (if any).</param>
		/// <param name="location">The current location.</param>
		public override bool Act(
			Farmer buddy,
			Tool buddyTool,
			Item item,
			GameLocation location
		)
		{
			return false;
		}

		/// <summary>Interact with a NPC.</summary>
		/// <param name="npc">The npc in the vicinity.</param>
		/// <param name="buddy">The current player who owns this drone.</param>
		/// <param name="tool">The tool selected by the player (if any).</param>
		/// <param name="item">The item selected by the player (if any).</param>
		/// <param name="location">The current location.</param>
		public override bool Interact(
			Farmer buddy,
			Tool tool,
			Item item,
			GameLocation location
		)
		{
			if (!this.IsWorking)
			{
				foreach (var npc in location.getCharacters())
				{
					if (!this.IsMonster(npc) || !npc.withinPlayerThreshold(3))
						continue;

					this.IsWorking = true;
					this.Target = (Monster) npc;
					break;
				}

				if (this.IsWorking && this.Target != null)
				{
					this.Shoot(
						this.Target,
						buddy,
						tool,
						item,
						location
					);

					// pod acted in this tick
					return true;
				}

			}

			// pod din't interact this tick
			return false;
		}

		private void Shoot(
			Monster monster,
			Farmer buddy,
			Tool tool,
			Item item,
			GameLocation location
		)
		{
			if (!this.HasWorked)
			{
				// set the damage
				this.SetDamage(monster);

				// define collision behaviour
				var collisionEffects = this.DefineCollisionBehaviour(monster);

				// figure out the cannon velocity
				Vector2 velocity = GetVelocityToward(monster);

				// ready the cannon
				BasicProjectile cannon = this.PrepareCannon(location, velocity, collisionEffects);

				// hurl the cannon and pod
				this.Beam(
					cannon,
					location
				);

				this.HasWorked = true;
			}

			this.ResetState();
		}

		private void SetDamage(Monster npc)
		{
			if (this.Damage == MAX_DAMAGE)
				this.Damage = npc.Health;
		}

		private CollisionBehavior DefineCollisionBehaviour(Monster monster)
		{
			return new CollisionBehavior(
				delegate(
						GameLocation loc,
						int x,
						int y,
						Character who
					)
				{
					Tool currentTool = null;
					Farmer buddy = !(who is Farmer) ? Game1.player : (Farmer)who;

					// remember tool
					if (Game1.player.CurrentTool != null)
						currentTool = Game1.player.CurrentTool;

					// reduce immortal armored bugs to mortal beings
					if (monster is Bug bug && bug.isArmoredBug)
						this.Helper.Reflection
							.GetField<NetBool>(bug, "isArmoredBug")
							.SetValue(new NetBool(false));

					// reduce rockcrabs to mortal beings
					if (monster is RockCrab rockCrab)
					{
						if (Game1.player.CurrentTool != null
						    && currentTool != null
						    && Game1.player.CurrentTool is Pickaxe)
							// TODO What is melee weapon with sprite index 4?
							Game1.player.CurrentTool = new MeleeWeapon(4);

						this.Helper.Reflection
							.GetField<NetBool>(rockCrab, "shellGone")
							.SetValue(new NetBool(true));
						this.Helper.Reflection
							.GetField<NetInt>(rockCrab, "shellHealth")
							.SetValue(new NetInt(0));
					}

					// hurt monster
					loc.damageMonster(
						monster.GetBoundingBox(),
						this.Damage,
						this.Damage + 1,
						true,
						buddy
					);

					// return previously held tool to buddy's hand
					if (Game1.player.CurrentTool != null && currentTool != null)
						Game1.player.CurrentTool = currentTool;
				}
			);
		}

		private Vector2 GetVelocityToward(Monster monster)
		{
			// TODO Access owner drone properties in pod
			Drone podParentDrone = (Drone)Game1
				.getCharacterFromName("Drone");

			return Utility.getVelocityTowardPoint(
				podParentDrone.Position,
				monster.Position,
				podParentDrone.ProjectileVelocity
			);
		}

		private BasicProjectile PrepareCannon(
			GameLocation location,
			Vector2 velocity,
			CollisionBehavior collisionBehavior
		)
		{
			// TODO Once again access owner drone properties in pod
			Drone ownerDrone = (Drone)Game1.getCharacterFromName("Drone");

			return new BasicProjectile(
				this.Damage,
				Projectile.shadowBall,
				0,
				0,
				0,
				velocity.X,
				velocity.Y,
				ownerDrone.Position,
				COLLISION_SOUND,
				firingSound: FIRING_SOUND,
				explode: false,
				damagesMonsters: true,
				location: location,
				firer: ownerDrone,
				spriteFromObjectSheet: false,
				collisionBehavior: collisionBehavior
			)
			{
				IgnoreLocationCollision = (Game1.currentLocation.currentEvent != null)
			};
		}

		private void ResetState()
		{
			this.IsWorking = false;
			this.HasWorked = false;
			this.Target = null;
		}
	}
}
