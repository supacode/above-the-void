extends CharacterBody2D

@onready var animated_sprite: AnimatedSprite2D = $AnimatedSprite2D
@onready var collision_shape: CollisionShape2D = $CollisionShape2D

const SPEED: float = 130.0
const JUMP_VELOCITY: float = -300.0

## How long the death slow-mo lasts before respawning (in game time).
const RESPAWN_DELAY: float = 0.6
## How long the player can't be hurt after respawning, so they can escape enemies.
const INVINCIBILITY_TIME: float = 1.2

var is_dead: bool = false
var is_invincible: bool = false

# The last spot the player stood on solid, non-moving ground. We respawn here.
var _last_safe_position: Vector2


func _ready() -> void:
	_last_safe_position = global_position


func _physics_process(delta: float) -> void:

	# While dead, ignore input and just fall (collision is off, so we drop through the floor).
	if is_dead:
		velocity.x = 0
		velocity += get_gravity() * delta
		move_and_slide()
		return

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


	# ==========================================
	# 5. REMEMBER SAFE GROUND (for respawning)
	# ==========================================
	if _is_on_solid_ground():
		_last_safe_position = global_position


## Returns true when standing on ground that won't move away (tiles, static bodies).
## Moving platforms are skipped: by the time we respawn, the platform may be gone.
func _is_on_solid_ground() -> bool:
	if not is_on_floor():
		return false
	var floor_hit := KinematicCollision2D.new()
	if not test_move(global_transform, Vector2.DOWN, floor_hit):
		return false
	return not floor_hit.get_collider() is AnimatableBody2D


# ==========================================
# DEATH & RESPAWN
# ==========================================
func die() -> void:
	if is_dead or is_invincible:
		return
	is_dead = true

	# Deferred because this is called from a physics signal (body_entered).
	collision_shape.set_deferred("disabled", true)
	Engine.time_scale = 0.5
	_shake_camera()

	await get_tree().create_timer(RESPAWN_DELAY).timeout
	_respawn()


func _respawn() -> void:
	Engine.time_scale = 1.0
	global_position = _last_safe_position
	velocity = Vector2.ZERO
	collision_shape.set_deferred("disabled", false)
	is_dead = false

	# Snap the camera to the respawn point instead of slowly panning back.
	var camera := get_viewport().get_camera_2d()
	if camera:
		camera.reset_smoothing()

	_start_invincibility()


func _start_invincibility() -> void:
	is_invincible = true

	# Blink the sprite while invincible.
	var blink := create_tween().set_loops(int(INVINCIBILITY_TIME / 0.2))
	blink.tween_property(animated_sprite, "modulate:a", 0.3, 0.1)
	blink.tween_property(animated_sprite, "modulate:a", 1.0, 0.1)
	await blink.finished

	is_invincible = false


func _shake_camera() -> void:
	var camera := get_viewport().get_camera_2d()
	if camera and camera.has_method("shake"):
		camera.shake()
