using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Projectiles;
using StardewValley.Monsters;
using StardewValley.Tools;
using StardewModdingAPI;
using Netcode;

using StarObject = StardewValley.Object;
using CollisionBehavior = StardewValley.Projectiles.BasicProjectile.onCollisionBehavior;

namespace GeraltDrone
{
	public class Drone : NPC
	{
		/*********
		** Fields
		*********/
		private readonly IModHelper Helper;
		private float T;
		private Monster Target;
		private int Damage;
		private readonly float ProjectileVelocity;
		private BasicProjectile CannonBall;

		private StarObject TileTarget;
		private int HakiPower;

		/****
        ** Constants
        ****/
		private const float R = 80f;
		private const float OFFSET_X = 5f;
		private const float OFFSET_Y = 20f;
		private const string COLLISION_SOUND = "hitEnemy";
		private const string FIRING_SOUND = "daggerswipe";

		private const string STONE = "Stone";

		/****
        ** State
        ****/
		private bool Throwing;
		private bool Thrown;

		private bool Destroying;
		private bool Destroyed;

		public Drone()
		{
		}

		public Drone(
			int speed,
			int damage,
			float projectileVelocity,
			IModHelper helper
		) : base(
			new AnimatedSprite(
				"Sidekick/Drone",
				1,
				12,
				12
			),
			Game1.player.Position,
			1,
			"Drone"
		)
		{
			this.speed = speed;
			this.hideShadow.Value = true;
			this.Damage = damage;
			this.ProjectileVelocity = projectileVelocity;
			this.Helper = helper;

			this.HakiPower = damage;
		}

		public override bool CanSocialize => false;

		public override bool canTalk()
		{
			return false;
		}

		public override void doEmote(int whichEmote, bool playSound, bool nextEventCommand = true)
		{
		}

		public override void update(GameTime time, GameLocation location)
		{
			float newX = Game1.player.position.X + OFFSET_X + R * (float)Math.Cos(T * 2 * Math.PI);
			float newY = Game1.player.position.Y - OFFSET_Y + R * (float)Math.Sin(T * 2 * Math.PI);

			this.position.Set(new Vector2(newX, newY));
			T = (T + (float)time.ElapsedGameTime.TotalMilliseconds / (1000 * speed)) % 1;

			this.ProtectPlayer(time, location);
			this.MineStones(time, location);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
		}

		/*********
        ** Private methods
        *********/
		private void MineStones(GameTime time, GameLocation location)
		{
			if (!this.Destroying)
			{
				foreach (KeyValuePair<Vector2, StarObject> pair in location.objects.Pairs)
				{
					// start by shooting and breaking stones; later breakable containers
					if(pair.Value?.name != STONE) continue;

					this.Destroying = true;
					this.TileTarget = (StarObject) pair.Value;

					break;
				}

				if (this.Destroying && this.TileTarget.name == STONE)
					this.ShootTheTileObject(time, location, this.TileTarget);
			}
		}

		private void ShootTheTileObject(GameTime time, GameLocation location, StarObject tileObject)
		{
			if (!this.Destroyed)
			{
				if (this.HakiPower == -1)
					this.HakiPower = tileObject.getHealth();

				CollisionBehavior collisionBehavior = StoneCollisionBehaviour(tileObject);
				Vector2 velocityTowardTile = Utility.getVelocityTowardPoint(
					Position,
					tileObject.TileLocation,
					this.ProjectileVelocity
				);

				this.CannonBall = this.GetCannonBall(
					location,
					velocityTowardTile,
					collisionBehavior
				);

				location.projectiles.Add(this.CannonBall);
				this.Destroyed = true;
			}

			this.ResetActionProps();
		}

		private CollisionBehavior StoneCollisionBehaviour(StarObject tileObject)
		{
			return new CollisionBehavior(
				delegate(GameLocation loc, int x, int y, Character who)
				{
					Tool currentTool = null;

					if (Game1.player.CurrentTool != null && Game1.player.CurrentTool is Tool)
						currentTool = Game1.player.CurrentTool;

					// collision checks and logic
					loc.destroyObject(
						tileObject.TileLocation,
						!(who is Farmer) ? Game1.player : who as Farmer
					);

					if (Game1.player.CurrentTool != null
					    && Game1.player.CurrentTool is Tool
					    && currentTool != null)
						Game1.player.CurrentTool = currentTool;
				}
			);
		}

		private void ResetActionProps()
		{
			if (this.Destroyed && this.CannonBall is BasicProjectile && this.CannonBall.destroyMe)
			{
				this.Destroying = false;
				this.Destroyed = false;
				this.CannonBall = null;
			}
		}

		private void ProtectPlayer(GameTime time, GameLocation location)
		{
			if (!this.Throwing)
			{
				foreach (var npc in Game1.currentLocation.getCharacters())
				{
					if (!npc.IsMonster || !npc.withinPlayerThreshold(3)) continue;

					this.Throwing = true;
					this.Target = (Monster)npc;

					break;
				}

			}

			if (this.Throwing && this.Target.IsMonster)
				this.ShootTheMonster(time, location, this.Target);
		}

		private void ShootTheMonster(GameTime time, GameLocation location, Monster monster)
		{
			if (!this.Thrown)
			{
				if (this.Damage == -1)
					this.Damage = monster.Health;

				var collisionBehavior = new CollisionBehavior(
					delegate (GameLocation loc, int x, int y, Character who)
					{
						Tool currentTool = null;

						if (Game1.player.CurrentTool != null && Game1.player.CurrentTool is Tool)
							currentTool = Game1.player.CurrentTool;

						if (monster is Bug bug && bug.isArmoredBug)
							this.Helper.Reflection
								.GetField<NetBool>(bug, "isArmoredBug")
								.SetValue(new NetBool(false));

						if (monster is RockCrab rockCrab)
						{
							if (Game1.player.CurrentTool != null
							    && Game1.player.CurrentTool is Tool
							    && currentTool != null
							    && Game1.player.CurrentTool is Pickaxe)
								Game1.player.CurrentTool = new MeleeWeapon(4);

							this.Helper.Reflection
								.GetField<NetBool>(rockCrab, "shellGone")
								.SetValue(new NetBool(true));
							this.Helper.Reflection
								.GetField<NetInt>(rockCrab, "shellHealth")
								.SetValue(new NetInt(0));
						}

						loc.damageMonster(
							monster.GetBoundingBox(),
							this.Damage,
							this.Damage + 1,
							true,
							!(who is Farmer) ? Game1.player : who as Farmer
						);

						if (Game1.player.CurrentTool != null
						    && Game1.player.CurrentTool is Tool
						    && currentTool != null)
							Game1.player.CurrentTool = currentTool;
					}
				);
				Vector2 velocityTowardMonster = this.GetVelocityTowardMonster(monster);

				this.CannonBall = this.GetCannonBall(
					location,
					velocityTowardMonster,
					collisionBehavior
				);

				location.projectiles.Add(this.CannonBall);
				this.Thrown = true;
			}

			this.ResetAttackProps();
		}

		private Vector2 GetVelocityTowardMonster(Monster monster)
		{
			return Utility.getVelocityTowardPoint(
				Position,
				monster.Position,
				this.ProjectileVelocity
			);
		}

		private BasicProjectile GetCannonBall(
			GameLocation location,
			Vector2 velocity,
			CollisionBehavior collisionBehavior
		)
		{
			return new BasicProjectile(
				this.Damage,
				Projectile.shadowBall,
				0,
				0,
				0,
				velocity.X,
				velocity.Y,
				this.position,
				COLLISION_SOUND,
				firingSound: FIRING_SOUND,
				explode: false,
				damagesMonsters: true,
				location: location,
				firer: this,
				spriteFromObjectSheet: false,
				collisionBehavior: collisionBehavior
			)
			{
				IgnoreLocationCollision = (Game1.currentLocation.currentEvent != null)
			};
		}

		private void ResetAttackProps()
		{
			if (this.Thrown && this.CannonBall is BasicProjectile && this.CannonBall.destroyMe)
			{
				this.Throwing = false;
				this.Thrown = false;
				this.CannonBall = null;
			}
		}
	}
}
