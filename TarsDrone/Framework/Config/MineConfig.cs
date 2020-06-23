namespace TarsDrone.Framework.Config
{
	public class MineConfig
	{
		/// <summary>Whether to break stones.</summary>
		public bool BreakStones { get; set; } = true;

		/// <summary>Whether to break containers in the mine.</summary>
		public bool BreakMineContainers { get; set; } = true;

		/// <summary>Whether to clear weeds.</summary>
		public bool ClearWeeds { get; set; } = true;
	}
}
