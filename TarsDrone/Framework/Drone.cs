
using Microsoft.Xna.Framework;
using StardewValley;
using StardewModdingAPI;
using TarsDrone.Framework.Core;

namespace TarsDrone.Framework
{
	internal class Drone: BaseDrone
	{
		public Drone(
			ModConfig config,
			IModHelper modHelper,
			IMonitor monitor,
			NPCOptions npcOptions)
			: base(config, modHelper, monitor, npcOptions)
		{
			// do nothing
		}

		public override void OnUpdate(GameTime time, GameLocation location)
		{
			// do nothing
		}
	}
}
