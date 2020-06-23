using System;
using TarsDrone.Framework.Config;

namespace TarsDrone
{
	/// <summary>The mod configurations.</summary>
	public class ModConfig
	{
		/*********
		** Fields
		*********/
		/****
		** Accessors
		****/
		/// <summary>Whether drone is active.</summary>
		public bool Active { get; set; }

		/// <summary>Shortcut to activate/deactivate drone.</summary>
		public string KeyboardShortcut { get; set; }

		/// <summary>Drone's speed</summary>
		public int RotationSpeed { get; set; }

		/// <summary>Damage to inflict on the target.</summary>
		public int Damage { get; set; }

		/// <summary>The velocity of the fired projectile.</summary>
		public int ProjectileVelocity { get; set; }

		// <summary>The built-in pod configurations</summary>
		// public BuiltInPodConfig BuiltInPods { get; set; }

		/// <summary>Constructs an instance</summary>
		public ModConfig()
		{
			// set configuration defaults
			this.Active = true;
			this.KeyboardShortcut = "F8";
			this.RotationSpeed = 2;
			this.Damage = -1;
			this.ProjectileVelocity = 16;
			// for pod defaults, check pod config classes
		}
	}
}
