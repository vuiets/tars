using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StarObject = StardewValley.Object;

namespace TarsDrone.Framework.Pods.Core
{
	internal interface IPod
	{
		/*********
		** Public methods
		*********/
		/// <summary>Get whether the tool is currently enabled.</summary>
		/// <param name="buddy">The current player who owns this drone.</param>
		/// <param name="tool">The tool selected by the player (if any).</param>
		/// <param name="item">The item selected by the player (if any).</param>
		/// <param name="location">The current location.</param>
		bool IsEnabled(
			Farmer buddy,
			Tool tool,
			Item item,
			GameLocation location
		);

		/// <summary>Act on the given tile.</summary>
		/// <param name="tile">The tile to modify.</param>
		/// <param name="tileObj">The object on the tile.</param>
		/// <param name="tileFeature">The feature on the tile.</param>
		/// <param name="buddy">The current player who owns this drone.</param>
		/// <param name="tool">The tool selected by the player (if any).</param>
		/// <param name="item">The item selected by the player (if any).</param>
		/// <param name="location">The current location.</param>
		bool Act(
			Vector2 tile,
			StarObject tileObj,
			TerrainFeature tileFeature,
			Farmer buddy,
			Tool tool,
			Item item,
			GameLocation location
		);

		/// <param name="npc">The non-player character in vicinity.</param>
		/// <param name="threshold">The number of tiles in proximity.</param>
		bool IsNpcWithinBuddyThreshold(NPC npc, int threshold);

		/// <param name="buddy">The current player who owns this drone.</param>
		/// <param name="tileObj">The object on the tile.</param>
		/// <param name="location">The current location.</param>
		/// <param name="threshold">The number of tiles in proximity.</param>
		bool IsTileObjWithinBuddyThreshold(
			Farmer buddy,
			StarObject tileObj,
			GameLocation location,
			int threshold
		);

		/// <summary>Method called when the pod attachments have been activated for a location.</summary>
		/// <param name="location">The current drone location.</param>
		void OnActivated(GameLocation location);
	}
}
