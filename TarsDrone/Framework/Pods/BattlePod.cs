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
	/// <summary>Pod to protect the drone's buddy.</summary>
	internal class BattlePod: BasePod
	{
		/*********
		** Fields
		*********/
		/// <summary>The attachment settings.</summary>
		private readonly BattleConfig Config;

		/// <summary>Damage to inflict on the target.</summary>
		private int Damage;

		/// <summary>The target of the attack.</summary>
		private Monster Target;

		/****
		** Constants
		****/
		/// <summary>Maximum damage that can be inflicted.</summary>
		private readonly int MAX_DAMAGE = -1;
		/// <summary>Sound on collision.</summary>
		private const string COLLISION_SOUND = "hitEnemy";
		/// <summary>Sound on firing.</summary>
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
				// get all NPCs in the location
				foreach (var npc in location.getCharacters())
				{
					// check whether NPC is a monster
					// also, if monster is in the vicinity of buddy
					if (!this.IsMonster(npc) || !npc.withinPlayerThreshold(3))
						continue;

					// if yes, get to work
					this.IsWorking = true;
					this.Target = (Monster) npc;
					break;
				}

				if (this.IsWorking && this.Target != null)
				{
					// protect the buddy
					this.Shoot(
						this.Target,
						location
					);

					// say pod acted in this tick
					return true;
				}

			}

			// say pod din't interact this tick
			return false;
		}

		/*********
		** Private methods
		*********/
		/// <summary>Shoot the monster.</summary>
		/// <param name="monster">The monster targeted by the pod.</param>
		/// <param name="location">The current location.</param>
		private void Shoot(
			Monster monster,
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

			// reset state variables
			this.ResetState();
		}

		/// <summary>Decide the damage to do on the monster.</summary>
		/// <param name="monster">The monster targeted by the pod.</param>
		private void SetDamage(Monster monster)
		{
			if (this.Damage == MAX_DAMAGE)
				this.Damage = monster.Health;
		}

		/// <summary>Define the behaviour on collision with monster.</summary>
		/// <param name="monster">The monster targeted by the pod.</param>
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

					// reduce rock crabs to mortal beings
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

					// inflict damage on monster
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

		/// <summary>Calculate the projectile velocity.</summary>
		/// <param name="monster">The monster targeted by the pod.</param>
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

		/// <summary>Ready the projectiles.</summary>
		/// <param name="location">The current location.</param>
		/// <param name="velocity">The projectile velocity.</param>
		/// <param name="collisionBehavior">The behaviour on collision.</param>
		private BasicProjectile PrepareCannon(
			GameLocation location,
			Vector2 velocity,
			CollisionBehavior collisionBehavior
		)
		{
			// TODO Once again access owner drone properties in pod
			Drone ownerDrone = (Drone)Game1
				.getCharacterFromName("Drone");

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

		/// <summary>Reset all state variables.</summary>
		private void ResetState()
		{
			this.IsWorking = false;
			this.HasWorked = false;
			this.Target = null;
		}
	}
}
