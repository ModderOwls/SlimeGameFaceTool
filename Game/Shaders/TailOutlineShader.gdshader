//adapted from: randommomentania.com/2019/07/godot-color-mask-tutorial/.


shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture;

uniform vec4 masking_color : source_color;
uniform float masking_range = 0.1;

void fragment() {
	vec4 world_pixel = texture(screen_texture, SCREEN_UV);
	
	if (length(abs(masking_color - world_pixel)) <= masking_range)
	{
		COLOR.a = 0.0f;
	}
}