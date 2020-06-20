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
			StarObject tileObj,
			Farmer buddy,
			Tool buddyTool,
			Item item,
			GameLocation location
		)
		{
			// check if object is in proximity
			if (!this
				.IsTileObjWithinBuddyThreshold(
					buddy,
					tileObj,
					location,
					3
				)
			)
				return false;

			// conjure an iridium pickaxe
			Tool tool = this.GetIridiumPickaxe();

			// break stones
			if(this.Config.BreakStones && this.IsStone(tileObj))
				return this.UseToolOnTile(
					tool,
					tileObj.TileLocation,
					buddy,
					location
				);

			// break mine containers, both boxes and barrels
			if (this.Config.BreakMineContainers && this.IsBreakableMineContainer(tileObj))
			{
				BreakableContainer container = (BreakableContainer) tileObj;
				return container.performToolAction(tool, location);
			}

			// clear weeds
			if(this.Config.ClearWeeds && this.IsWeed(tileObj))
				return this.UseToolOnTile(
					tool,
					tileObj.TileLocation,
					buddy,
					location
				);

			return false;
		}

		/// <summary>Interact with a NPC.</summary>
		/// <param name="npc">The npc in the vicinity.</param>
		/// <param name="buddy">The current player who owns this drone.</param>
		/// <param name="tool">The tool selected by the player (if any).</param>
		/// <param name="item">The item selected by the player (if any).</param>
		/// <param name="location">The current location.</param>
		public override bool Interact(
			NPC npc,
			Farmer buddy,
			Tool tool,
			Item item,
			GameLocation location
		)
		{
			return false;
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
	}
}
