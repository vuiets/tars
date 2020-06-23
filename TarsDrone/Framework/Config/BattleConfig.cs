namespace TarsDrone.Framework.Config
{
	public class BattleConfig
	{
		/// <summary>Whether to attack monsters.</summary>
		public bool AttackMonsters { get; set; } = true;

		/// <summary>Damage to inflict on monsters. -1 is one-hit KO.</summary>
		public int Damage { get; set; } = -1;
	}
}
