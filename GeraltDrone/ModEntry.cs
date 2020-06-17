using System;
using StardewValley;
using StardewValley.Locations;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace GeraltDrone
{
	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod, IAssetLoader
	{
		/*********
        ** Fields
        *********/
		/// <summary>Provides modding utilities.</summary>
		private IModHelper ModHelper;

		/****
	    ** Configuration
	    ****/
		/// <summary>The mod configuration.</summary>
		private ModConfig Config;

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
				return this.ModHelper.Content.Load<T>("Assets/drone_sprite_robot.png", ContentSource.ModFolder);

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
				if (Config.Active)
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

		private void ActivateDrone()
		{
			this.AddDrone();
			this.Config.Active = true;
			this.ModHelper.WriteConfig(Config);
			Game1.addHUDMessage(new HUDMessage("Drone activated.", 4));
		}

		private void DeactivateDrone()
		{
			this.RemoveDrone();
			this.Config.Active = false;
			this.ModHelper.WriteConfig(Config);
			Game1.showRedMessage("Drone deactivated");
		}

		private void AddDrone()
		{
			if (Game1.currentLocation is DecoratableLocation)
				return;

			if (Game1.getCharacterFromName("Drone") == null)
				Game1.currentLocation.addCharacter(
					new Drone(
						this.Config.RotationSpeed,
						this.Config.Damage,
						(float)this.Config.ProjectileVelocity,
						this.ModHelper
					)
				);
			else
				Game1.warpCharacter(
					Game1.getCharacterFromName("Drone"),
					Game1.currentLocation,
					Game1.player.Position
				);
		}

		private void RemoveDrone()
		{
			if (Game1.getCharacterFromName("Drone") is NPC drone)
				Game1.removeThisCharacterFromAllLocations(drone);
		}
	}
}
