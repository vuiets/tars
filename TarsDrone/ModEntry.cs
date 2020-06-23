using System;
using StardewValley;
using StardewValley.Locations;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using TarsDrone.Framework;
using TarsDrone.Framework.Core;

namespace TarsDrone
{
	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod, IAssetLoader
	{
		/*********
        ** Fields
        *********/
		/// <summary>Provides modding utilities.</summary>
		private IModHelper ModHelper;

		/// <summary>The mod configurations.</summary>
		private ModConfig Config;

		/****
		** Constants
		****/
		/// <summary>Reference label of the drone's sprite.</summary>
		private readonly string SPRITE_NAME = "Sidekick/Drone";

		/// <summary>Sprite pixel width.</summary>
		private readonly int SPRITE_PIXEL_WIDTH = 12;

		/// <summary>Sprite pixel height.</summary>
		private readonly int SPRITE_PIXEL_HEIGHT = 12;

		/// <summary>Name of the drone.</summary>
		private readonly string DRONE_NAME = "Drone";

		/*********
        ** Public methods
        *********/
		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
	        // load config
	        this.ModHelper = helper;
			this.Config = this.LoadConfig();

	        // hook up events
			helper.Events.GameLoop.Saving += this.OnGameSaveCreated;
			helper.Events.Input.ButtonPressed += this.OnButtonPressed;
			helper.Events.Player.Warped += this.OnPlayerWarped;
		}

		/// <summary>Get whether this instance can load the initial version of the given asset.</summary>
		/// <param name="asset">Basic metadata about the asset being loaded.</param>
		public bool CanLoad<T>(IAssetInfo asset)
		{
			return asset.AssetNameEquals("Sidekick/Drone");
		}

		/// <summary>Load a matched asset.</summary>
		/// <param name="asset">Basic metadata about the asset being loaded.</param>
		public T Load<T>(IAssetInfo asset)
		{
			if (asset.AssetNameEquals("Sidekick/Drone"))
				return this.ModHelper.Content.Load<T>("Assets/cute_robot_sprite.png");

			throw new InvalidOperationException($"Unexpected asset '{asset.AssetName}'.");
		}

		/*********
        ** Private methods
        *********/
		/****
        ** Event handlers
        ****/
		/// <summary>The method invoked when a game is saved.</summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event data.</param>
		private void OnGameSaveCreated(object sender, SavingEventArgs e)
		{
			// remove the drone
			this.RemoveDrone();
		}

		/// <summary>The method invoked when the player presses a button.</summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event data.</param>
		private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (!Context.IsPlayerFree || Game1.currentMinigame != null)
				return;

			if (e.Button == (SButton)Enum.Parse(
				typeof(SButton),
				this.Config.KeyboardShortcut,
				true
			))
			{
				if (this.Config.Active)
					this.DeactivateDrone();
				else
					this.ActivateDrone();
			}
		}

		/// <summary>The method invoked when the player is warped.</summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event data.</param>
		private void OnPlayerWarped(object sender, WarpedEventArgs e)
		{
			if (!e.IsLocalPlayer || Game1.CurrentEvent != null || !this.Config.Active)
				return;

			this.AddDrone();
		}

		/****
		** Generic helpers
		****/
		/// <summary>Read the config file.</summary>
		/// <returns>The config.</returns>
		private ModConfig LoadConfig()
		{
			// load config
			return this.ModHelper.ReadConfig<ModConfig>();
		}

		/// <summary>Activate drone and its pods.</summary>
		private void ActivateDrone()
		{
			this.AddDrone();
			this.Config.Active = true;
			this.ModHelper.WriteConfig(Config);
			Game1.addHUDMessage(new HUDMessage("Drone activated.", 4));
		}

		/// <summary>Deactivate drone and its pods.</summary>
		private void DeactivateDrone()
		{
			this.RemoveDrone();
			this.Config.Active = false;
			this.ModHelper.WriteConfig(Config);
			Game1.showRedMessage("Drone deactivated");
		}

		/// <summary>Add drone to the world.</summary>
		private void AddDrone()
		{
			if (Game1.currentLocation is DecoratableLocation)
				return;

			if (Game1.getCharacterFromName("Drone") == null)
				Game1.currentLocation
					.addCharacter(CreateDrone());
			else
				Game1.warpCharacter(
					Game1.getCharacterFromName("Drone"),
					Game1.currentLocation,
					Game1.player.Position
				);
		}

		/// <summary>Remove drone from the world.</summary>
		private void RemoveDrone()
		{
			if (Game1.getCharacterFromName(this.DRONE_NAME) is NPC drone)
				Game1.removeThisCharacterFromAllLocations(drone);
		}

		/// <summary>Create a drone instance, along with it's sprite.</summary>
		private Drone CreateDrone()
		{
			// create drone's sprite
			AnimatedSprite droneSprite = new AnimatedSprite(
				this.SPRITE_NAME,
				1,
				this.SPRITE_PIXEL_WIDTH,
				this.SPRITE_PIXEL_HEIGHT
			);
			// options to instantiate drone
			NPCOptions droneOptions = new NPCOptions(
				droneSprite,
				Game1.player.Position,
				1,
				this.DRONE_NAME
			);

			// instantiate drone
			return new Drone(
				this.Config,
				this.Helper,
				this.Monitor,
				droneOptions
			);
		}
	}
}
