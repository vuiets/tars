using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using StardewModdingAPI;
using TarsDrone.Framework.Config;
using TarsDrone.Framework.Pods.Core;
using StarObject = StardewValley.Object;

namespace TarsDrone.Framework.Pods
{
	internal class MinePod: BasePod
	{
		/*********
		** Fields
		*********/
		/// <summary>The attachment settings.</summary>
		private readonly MineConfig Config;

		/****
		** State
		****/
		private StarObject Target;

		/*********
		** Public methods
		*********/
		/// <summary>Construct an instance.</summary>
		/// <param name="config">The attachment settings.</param>
		/// <param name="modHelper">Fetches metadata about loaded mods.</param>
		/// <param name="monitor">Simplifies access to private code.</param>
		public MinePod(
			MineConfig config,
			IModHelper modHelper,
			IMonitor monitor)
			: base(modHelper, monitor)
		{
			this.Config = config;
			this.Target = null;
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

		/// <summary>Act on the objects in close proximity.</summary>
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
			try
			{
				return this.Scan(
					buddy,
					buddyTool,
					item,
					location
				);
			}
			catch (Exception actException)
			{
				this.Monitor.Log(actException.StackTrace, LogLevel.Error);
				this.ResetState();
				return false;
			}
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
			return false;
		}

		private bool Scan(
			Farmer buddy,
			Tool buddyTool,
			Item item,
			GameLocation location
		)
		{
			if (!this.IsWorking)
			{
				foreach (KeyValuePair<Vector2, StarObject> pair in location.objects.Pairs)
				{
					this.Target = pair.Value;

					// check if object is in proximity
					if (!this
						.IsTileObjWithinBuddyThreshold(
							buddy,
							this.Target,
							location,
							3
						)
					)
						continue;

					this.IsWorking = true;
					break;
				}

				if (this.IsWorking && this.Target != null)
				{
					this.Mine(
						this.Target,
						buddy,
						buddyTool,
						item,
						location
					);

					// pod acted in this tick
					return true;
				}
			}

			// pod din't act this tick
			return false;
		}

		private void Mine(
			StarObject tileObj,
			Farmer buddy,
			Tool buddyTool,
			Item item,
			GameLocation location
		)
		{
			if (!this.HasWorked)
			{
				// conjure an iridium pickaxe
				Tool tool = this.GetIridiumPickaxe();

				// break stones
				if(this.Config.BreakStones && this.IsStone(tileObj))
					this.UseToolOnTile(
						tool,
						tileObj.TileLocation,
						buddy,
						location
					);

				// TODO Fix break mine containers, both boxes and barrels
				//if (this.Config.BreakMineContainers && this.IsBreakableMineContainer(tileObj))
				//{
				//	Tool club = this.GetClubWeapon();
				//	BreakableContainer container = (BreakableContainer) tileObj;
				//	tileObj.performToolAction(club, location);
				//}

				// clear weeds
				if(this.Config.ClearWeeds && this.IsWeed(tileObj))
					this.UseToolOnTile(
						tool,
						tileObj.TileLocation,
						buddy,
						location
					);

				this.HasWorked = true;
			}

			this.ResetState();
		}

		private Tool GetIridiumPickaxe()
		{
			Tool pickaxe = new Pickaxe();

			// upgrade pickaxe to iridium
			this.Helper.Reflection
				.GetField<NetInt>(pickaxe, "upgradeLevel")
				.SetValue(new NetInt(4));

			return pickaxe;
		}

		private Tool GetClubWeapon()
		{
			Tool club = new MeleeWeapon(MeleeWeapon.defenseSword);

			return club;
		}

		private void ResetState()
		{
			this.IsWorking = false;
			this.HasWorked = false;
			this.Target = null;
		}
	}
}
