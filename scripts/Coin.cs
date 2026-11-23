using Godot;

public partial class Coin : Area2D
{
	// Connected to the BodyEntered signal in coin.tscn.
	private void OnBodyEntered(Node2D body)
	{
		if (body is Player)
		{
			QueueFree();
		}
	}
}
