extends Camera2D

@export var shake_strength: float = 4.0
@export var shake_duration: float = 0.35

var _shake_time_left: float = 0.0
var _current_strength: float = 0.0


## Shake the camera. Call with no arguments to use the exported defaults.
func shake(strength: float = shake_strength, duration: float = shake_duration) -> void:
	_current_strength = strength
	_shake_time_left = duration


func _process(delta: float) -> void:
	if _shake_time_left <= 0.0:
		return

	_shake_time_left -= delta

	if _shake_time_left <= 0.0:
		offset = Vector2.ZERO
		return

	var fade := _shake_time_left / shake_duration
	var amount := _current_strength * fade
	offset = Vector2(randf_range(-amount, amount), randf_range(-amount, amount))
