namespace TarsDrone.Framework.Config
{
	internal class BuiltInPodConfig
	{
		/// <summary>Configuration for the Battle pod.</summary>
		public BattleConfig Battle { get; set; } = new BattleConfig();

		/// <summary>Configuration for the Mine pod.</summary>
		public MineConfig Mine { get; set; } = new MineConfig();
	}
}
