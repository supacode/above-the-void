using Godot;

public partial class Killzone : Area2D
{
	// Connected to the BodyEntered signal in killzone.tscn.
	// The player handles its own death, camera shake and respawn.
	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			player.Die();
		}
	}
}
