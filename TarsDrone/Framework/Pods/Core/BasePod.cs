using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewModdingAPI;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StarObject = StardewValley.Object;

namespace TarsDrone.Framework.Pods.Core
{	/// <summary>Base class for pod implementations</summary>
	internal abstract class BasePod: IPod
	{
		/*********
		** Fields
		*********/
		/// <summary>Provides handy modding utilities.</summary>
		protected IModHelper Helper { get; }

		/// <summary>Helps with logging.</summary>
		protected IMonitor Monitor { get; }

		/****
		* Constants
		****/
		/// <summary>Name of Stone object.</summary>
		private readonly string STONE = "Stone";

		/// <summary>Name of Weed object.</summary>
		private readonly string WEEDS = "Weeds";

		/****
		** State
		****/
		/// <summary>Whether the pod is still acting.</summary>
		protected bool IsWorking;

		/// <summary>Whether the pod has finished acting this tick.</summary>
		protected bool HasWorked;

		/*********
 		** Public methods
		*********/
		/// <summary>Get whether the tool is currently enabled.</summary>
		/// <param name="buddy">The current player who owns this drone.</param>
		/// <param name="tool">The tool selected by the player (if any).</param>
		/// <param name="item">The item selected by the player (if any).</param>
		/// <param name="location">The current location.</param>
		public abstract bool IsEnabled(
			Farmer buddy,
			Tool tool,
			Item item,
			GameLocation location
		);

		/// <summary>Perform an action.</summary>
		/// <param name="buddy">The current player who owns this drone.</param>
		/// <param name="tool">The tool selected by the player (if any).</param>
		/// <param name="item">The item selected by the player (if any).</param>
		/// <param name="location">The current location.</param>
		public abstract bool Act(
			Farmer buddy,
			Tool tool,
			Item item,
			GameLocation location
		);

		/// <summary>Interact with a NPC.</summary>
		/// <param name="buddy">The current player who owns this drone.</param>
		/// <param name="tool">The tool selected by the player (if any).</param>
		/// <param name="item">The item selected by the player (if any).</param>
		/// <param name="location">The current location.</param>
		public abstract bool Interact(
			Farmer buddy,
			Tool tool,
			Item item,
			GameLocation location
		);

		public virtual bool IsNpcWithinBuddyThreshold(NPC npc, int threshold)
		{
			return npc.withinPlayerThreshold(threshold);
		}

		public bool IsTileObjWithinBuddyThreshold(
			Farmer buddy,
			StarObject tileObj,
			GameLocation location,
			int threshold
		)
		{
			if (location != null && !location.farmers.Any())
				return false;

			Vector2 tileLocation1 = buddy.getTileLocation();
			Vector2 tileLocation2 = tileObj.TileLocation;

			return (double) Math.Abs(tileLocation2.X - tileLocation1.X) <= (double) threshold
			       && (double) Math.Abs(tileLocation2.Y - tileLocation1.Y) <= (double) threshold;
		}

		/// <summary>Method called when the tractor attachments have been activated for a location.</summary>
		/// <param name="location">The current tractor location.</param>
		public virtual void OnActivated(GameLocation location)
		{
			// do nothing
		}

		/*********
        ** Protected methods
        *********/
		/// <summary>Construct an instance.</summary>
		/// <param name="modHelper">Provides handy modding utilities.</param>
		/// <param name="monitor">Helps with logging.</param>
		protected BasePod(IModHelper modHelper, IMonitor monitor)
		{
			this.Helper = modHelper;
			this.Monitor = monitor;
		}

		/// <summary>Get whether a given object is a stone.</summary>
		/// <param name="tileObj">The world object on tile.</param>
		protected bool IsStone(StarObject tileObj)
		{
			return tileObj?.Name == STONE;
		}

		/// <summary>Get whether a given object is a breakable container - box or barrel</summary>
		/// <param name="tileObj">The world object on tile.</param>
		protected bool IsBreakableMineContainer(StarObject tileObj)
		{
			return tileObj is BreakableContainer;
		}

		/// <summary>Get whether a given object is a weed.</summary>
		/// <param name="tileObj">The world object on tile.</param>
		protected bool IsWeed(StarObject tileObj)
		{
			return !(tileObj is Chest) && tileObj?.Name == WEEDS;
		}

		/// <summary>Get whether a npc is a monster.</summary>
		/// <param name="npc">The non-player character in the world.</param>
		protected bool IsMonster(NPC npc)
		{
			return npc.IsMonster;
		}

		/// <summary>Use a tool on a tile.</summary>
		/// <param name="tool">The tool to use.</param>
		/// <param name="tile">The tile to affect.</param>
		/// <param name="buddy">The current player who owns this drone.</param>
		/// <param name="location">The current location.</param>
		/// <returns>Returns <c>true</c> for convenience when implementing tools.</returns>
		protected bool UseToolOnTile(
			Tool tool,
			Vector2 tile,
			Farmer buddy,
			GameLocation location
		)
		{
			// use tool on center of tile
			buddy.lastClick = this.GetToolPixelPosition(tile);
			tool.DoFunction(
				location,
				(int)buddy.lastClick.X,
				(int)buddy.lastClick.Y,
				0,
				buddy
			);
			return true;
		}

		/// <summary>Get the pixel position relative to the top-left corner of the map at which to use a tool.</summary>
		/// <param name="tile">The tile to affect.</param>
		private Vector2 GetToolPixelPosition(Vector2 tile)
		{
			return (tile * Game1.tileSize) + new Vector2(Game1.tileSize / 2f);
		}

		/// <summary>Fire a projectile.</summary>
		/// <param name="projectile">The self-propelling item with inherent collision behaviour.</param>
		/// <param name="location">The current location.</param>
		/// <returns>Returns <c>true</c> for convenience when implementing actions.</returns>
		protected bool Beam(
			BasicProjectile projectile,
			GameLocation location
		)
		{
			location.projectiles.Add(projectile);
			return true;
		}
	}
}
