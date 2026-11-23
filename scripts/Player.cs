using Godot;

public partial class Player : CharacterBody2D
{
	public const float Speed = 130.0f;
	public const float JumpVelocity = -300.0f;

	/// <summary>How long the death slow-mo lasts before respawning (in game time).</summary>
	public const double RespawnDelay = 0.6;
	/// <summary>How long the player can't be hurt after respawning, so they can escape enemies.</summary>
	public const double InvincibilityTime = 1.2;

	private const double BlinkInterval = 0.2;

	public bool IsDead { get; private set; }
	public bool IsInvincible { get; private set; }

	private AnimatedSprite2D _animatedSprite;
	private CollisionShape2D _collisionShape;

	// The last spot the player stood on solid, non-moving ground. We respawn here.
	private Vector2 _lastSafePosition;

	public override void _Ready()
	{
		_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		_lastSafePosition = GlobalPosition;
	}

	public override void _PhysicsProcess(double delta)
	{
		// Vector2 is a struct in C#, so `Velocity.X = ...` won't compile.
		// Work on a copy and assign it back before moving.
		Vector2 velocity = Velocity;

		// While dead, ignore input and just fall (collision is off, so we drop through the floor).
		if (IsDead)
		{
			velocity.X = 0;
			velocity += GetGravity() * (float)delta;
			Velocity = velocity;
			MoveAndSlide();
			return;
		}

		// ==========================================
		// 1. VERTICAL MOVEMENT (Gravity & Jumping)
		// ==========================================
		if (IsOnFloor())
		{
			if (Input.IsActionJustPressed("ui_accept"))
			{
				velocity.Y = JumpVelocity;
			}
		}
		else
		{
			velocity += GetGravity() * (float)delta;
		}


		// ==========================================
		// 2. HORIZONTAL MOVEMENT (Input & Velocity)
		// ==========================================
		float direction = Input.GetAxis("ui_left", "ui_right");

		if (direction != 0)
		{
			velocity.X = direction * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X, 0, Speed);
		}


		// ==========================================
		// 3. VISUALS (Flipping & Animations)
		// ==========================================
		// Flip the sprite based on input direction
		if (direction != 0)
		{
			// If direction is negative (left), FlipH becomes true.
			// If positive (right), FlipH becomes false.
			_animatedSprite.FlipH = direction < 0;
		}

		// Determine which animation to play
		if (!IsOnFloor())
		{
			_animatedSprite.Play("jump");
		}
		else if (direction != 0)
		{
			_animatedSprite.Play("run");
		}
		else
		{
			_animatedSprite.Play("idle");
		}


		// ==========================================
		// 4. APPLY MOVEMENT
		// ==========================================
		Velocity = velocity;
		MoveAndSlide();


		// ==========================================
		// 5. REMEMBER SAFE GROUND (for respawning)
		// ==========================================
		if (IsOnSolidGround())
		{
			_lastSafePosition = GlobalPosition;
		}
	}

	/// <summary>
	/// Returns true when standing on ground that won't move away (tiles, static bodies).
	/// Moving platforms are skipped: by the time we respawn, the platform may be gone.
	/// </summary>
	private bool IsOnSolidGround()
	{
		if (!IsOnFloor())
		{
			return false;
		}

		var floorHit = new KinematicCollision2D();
		if (!TestMove(GlobalTransform, Vector2.Down, floorHit))
		{
			return false;
		}

		return floorHit.GetCollider() is not AnimatableBody2D;
	}

	// ==========================================
	// DEATH & RESPAWN
	// ==========================================
	public async void Die()
	{
		if (IsDead || IsInvincible)
		{
			return;
		}
		IsDead = true;

		// Deferred because this is called from a physics signal (BodyEntered).
		_collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		Engine.TimeScale = 0.5;
		ShakeCamera();

		await ToSignal(GetTree().CreateTimer(RespawnDelay), SceneTreeTimer.SignalName.Timeout);
		Respawn();
	}

	private void Respawn()
	{
		Engine.TimeScale = 1.0;
		GlobalPosition = _lastSafePosition;
		Velocity = Vector2.Zero;
		_collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
		IsDead = false;

		// Snap the camera to the respawn point instead of slowly panning back.
		GetViewport().GetCamera2D()?.ResetSmoothing();

		StartInvincibility();
	}

	private async void StartInvincibility()
	{
		IsInvincible = true;

		// Blink the sprite while invincible.
		Tween blink = CreateTween().SetLoops(Mathf.RoundToInt(InvincibilityTime / BlinkInterval));
		blink.TweenProperty(_animatedSprite, "modulate:a", 0.3f, BlinkInterval / 2);
		blink.TweenProperty(_animatedSprite, "modulate:a", 1.0f, BlinkInterval / 2);
		await ToSignal(blink, Tween.SignalName.Finished);

		IsInvincible = false;
	}

	private void ShakeCamera()
	{
		if (GetViewport().GetCamera2D() is GameCamera camera)
		{
			camera.Shake();
		}
	}
}
