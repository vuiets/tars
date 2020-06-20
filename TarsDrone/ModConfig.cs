using System;
using TarsDrone.Framework.Config;

namespace TarsDrone
{
	public class ModConfig
	{
		public bool Active { get; set; }
		public string KeyboardShortcut { get; set; }
		public int RotationSpeed { get; set; }
		public int Damage { get; set; }
		public int ProjectileVelocity { get; set; }

//		public BuiltInPodConfig BuiltInPods { get; set; }

		public ModConfig()
		{
			this.Active = true;
			this.KeyboardShortcut = "F8";
			this.RotationSpeed = 2;
			this.Damage = -1;
			this.ProjectileVelocity = 16;
		}
	}
}
