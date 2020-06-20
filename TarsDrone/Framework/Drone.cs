using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewModdingAPI;
using TarsDrone.Framework.Config;
using TarsDrone.Framework.Core;
using TarsDrone.Framework.Pods;
using TarsDrone.Framework.Pods.Core;
using StarObject = StardewValley.Object;

namespace TarsDrone.Framework
{
	internal class Drone: BaseDrone
	{
		private readonly IModHelper Helper;
		private readonly IMonitor Monitor;
		private readonly ModConfig Config;
		private readonly IPod[] Pods;

		public Drone(
			ModConfig config,
			IModHelper modHelper,
			IMonitor monitor,
			NPCOptions npcOptions
		)
			: base(config, modHelper, monitor, npcOptions)
		{
			this.Config = config;
			this.Helper = modHelper;
			this.Monitor = monitor;
//			BuiltInPodConfig builtInPodConfig = this.Config.BuiltInPods;

			// attach pods
			this.Pods = new IPod[]
			{
				new MinePod(new MineConfig(), modHelper, monitor),
				new BattlePod(new BattleConfig(), modHelper, monitor),
			};
		}

		public override void OnUpdate(GameTime time, GameLocation location)
		{
			// get context
			Farmer buddy = Game1.player;
//			GameLocation presentLocation = Game1.currentLocation;
			Tool tool = buddy.CurrentTool;
			Item item = buddy.CurrentItem;

			// check stamina
			// if stamina is okay, mine
			this.Mine(
				buddy,
				tool,
				item,
				location
			);

			// then kill monsters
			this.Shoot(
				buddy,
				tool,
				item,
				location
			);

			// else ask buddy to eat
		}

		private void Mine(
			Farmer buddy,
			Tool tool,
			Item item,
			GameLocation location
		)
		{
			foreach (KeyValuePair<Vector2, StarObject> pair in location.objects.Pairs)
			{
				bool mined = false;

				foreach (IPod pod in this.Pods)
				{
					mined = pod
						.Act(
							pair.Value,
							buddy,
							tool,
							item,
							location
						);

					if(mined)
						break;
				}

				if(mined)
					break;
			}
		}

		private void Shoot(
			Farmer buddy,
			Tool tool,
			Item item,
			GameLocation location
		)
		{
			foreach (var npc in location.getCharacters())
			{
				bool attacked = false;

				foreach (IPod pod in this.Pods)
				{
					attacked = pod
						.Interact(
							npc,
							buddy,
							tool,
							item,
							location
						);

					if(attacked)
						break;
				}

				if(attacked)
					break;
			}
		}
	}
}
