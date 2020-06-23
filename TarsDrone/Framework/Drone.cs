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
	/// <summary>Drone instruction.</summary>
	internal class Drone: BaseDrone
	{
		/*********
		** Fields
		*********/
		/// <summary>Provides handy modding utilities.</summary>
		private readonly IModHelper Helper;

		/// <summary>Helps with logging.</summary>
		private readonly IMonitor Monitor;

		/// <summary>The mod configurations.</summary>
		private readonly ModConfig Config;

		/// <summary>List of pods attached.</summary>
		private readonly IPod[] Pods;

		/*********
		** Public methods
		*********/
		/// <summary>Construct an instance.</summary>
		/// <param name="config">The mod configurations.</param>
		/// <param name="modHelper">Provides modding utilities.</param>
		/// <param name="monitor">Helps with logging.</param>
		/// <param name="npcOptions">Options to instantiate a npc.</param>
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
			// BuiltInPodConfig builtInPodConfig = this.Config.BuiltInPods;

			// attach pods
			this.Pods = new IPod[]
			{
				new MinePod(new MineConfig(), modHelper, monitor),
				new BattlePod(new BattleConfig(), modHelper, monitor),
			};
		}

		/// <summary>Action performed on each update.</summary>
		/// <param name="time">The current game time.</param>
		/// <param name="location">The current location.</param>
		public override void OnUpdate(GameTime time, GameLocation location)
		{
			// get context
			Farmer buddy = Game1.player;
			Tool tool = buddy.CurrentTool;
			Item item = buddy.CurrentItem;

			// check stamina
			// if stamina is okay activate all pods
			foreach (IPod pod in this.Pods)
			{
				// okay now mine
				pod
					.Act(
						buddy,
						tool,
						item,
						location
					);

				// then attack monsters
				pod
					.Interact(
						buddy,
						tool,
						item,
						location
					);
			}
			// else ask buddy to eat
		}
	}
}
