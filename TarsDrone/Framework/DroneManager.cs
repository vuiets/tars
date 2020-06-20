using System.Linq;
using System.Collections.Generic;
using StardewValley;
using TarsDrone.Framework.Pods.Core;

namespace TarsDrone.Framework
{
	internal sealed class DroneManager
	{
		/// <summary>The mod settings.</summary>
		private ModConfig Config;

		/// <summary>The drone pods to apply.</summary>
		private IPod[] Pods;

		/// <summary>Construct an instance.</summary>
		/// <param name="config">The mod configurations.</param>
		public DroneManager(ModConfig config)
		{
			this.Config = config;
		}

		///<summary></summary>
		private IEnumerable<IPod> GetEnabledPods(
			Farmer buddy,
			Tool tool,
			Item item,
			GameLocation location
		)
		{
			foreach (IPod pod in this.Pods)
			{
				if (pod
					.IsEnabled(
						buddy,
						tool,
						item,
						location
					)
				)
					yield return pod;
			}
		}

		/// <summary>Perform all configured pod actions</summary>
		private void UpdatePodEffects()
		{
			// gather context
			Farmer buddy = Game1.player;
			GameLocation location = Game1.currentLocation;
			Tool tool = buddy.CurrentTool;
			Item item = buddy.CurrentItem;

			// get active pods
			IPod[] pods = this
				.GetEnabledPods(
					buddy,
					tool,
					item,
					location
				)
				.ToArray();
			if(!pods.Any())
				return;

			// TODO: perform active pod actions
		}
	}
}
