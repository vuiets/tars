
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
			IMonitor monitor)
			: base(config, modHelper, monitor)
		{
			// do nothing
		}

		public override void OnUpdate(GameTime time, GameLocation location)
		{
			// do nothing
		}
	}
}
