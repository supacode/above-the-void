using Godot;

public partial class GameCamera : Camera2D
{
	/// <summary>How far (in world pixels) the camera jolts at the start of a shake.</summary>
	[Export] public float ShakeStrength { get; set; } = 4.0f;
	/// <summary>How long the shake lasts, in seconds (affected by Engine.TimeScale).</summary>
	[Export] public float ShakeDuration { get; set; } = 0.35f;

	private float _shakeTimeLeft;
	private float _currentDuration;
	private float _currentStrength;

	/// <summary>Shake the camera using the exported defaults.</summary>
	public void Shake() => Shake(ShakeStrength, ShakeDuration);

	/// <summary>Shake the camera with a custom strength and duration.</summary>
	public void Shake(float strength, float duration)
	{
		_currentStrength = strength;
		_currentDuration = duration;
		_shakeTimeLeft = duration;
	}

	public override void _Process(double delta)
	{
		if (_shakeTimeLeft <= 0.0f)
		{
			return;
		}

		_shakeTimeLeft -= (float)delta;

		if (_shakeTimeLeft <= 0.0f)
		{
			Offset = Vector2.Zero;
			return;
		}

		// Fade the shake out linearly so it settles smoothly instead of stopping abruptly.
		float fade = _shakeTimeLeft / _currentDuration;
		float amount = _currentStrength * fade;
		// `Offset` is used (not `Position`) so the camera's framing is untouched
		// and the shake still works when the camera is pressed against its limits.
		Offset = new Vector2(
			(float)GD.RandRange(-amount, amount),
			(float)GD.RandRange(-amount, amount));
	}
}
