extends CharacterBody2D

@onready var animated_sprite: AnimatedSprite2D = $AnimatedSprite2D

const SPEED: float = 130.0
const JUMP_VELOCITY: float = -300.0

func _physics_process(delta: float) -> void:
	
	# ==========================================
	# 1. VERTICAL MOVEMENT (Gravity & Jumping)
	# ==========================================
	if is_on_floor():
		if Input.is_action_just_pressed("ui_accept"):
			velocity.y = JUMP_VELOCITY
	else:
		velocity += get_gravity() * delta


	# ==========================================
	# 2. HORIZONTAL MOVEMENT (Input & Velocity)
	# ==========================================
	var direction := Input.get_axis("ui_left", "ui_right")
	
	if direction != 0:
		velocity.x = direction * SPEED
	else:
		velocity.x = move_toward(velocity.x, 0, SPEED)


	# ==========================================
	# 3. VISUALS (Flipping & Animations)
	# ==========================================
	# Flip the sprite based on input direction
	if direction != 0:
		# If direction is negative (left), flip_h becomes true.
		# If positive (right), flip_h becomes false.
		animated_sprite.flip_h = (direction < 0)
	
	# Determine which animation to play
	if not is_on_floor():
		animated_sprite.play("jump")
	elif direction != 0:
		animated_sprite.play("run")
	else:
		animated_sprite.play("idle")


	# ==========================================
	# 4. APPLY MOVEMENT
	# ==========================================
	move_and_slide()
