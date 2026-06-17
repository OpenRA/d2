#version {VERSION}
#ifdef GL_ES
precision mediump float;
#endif

uniform sampler2D WorldTexture;
uniform sampler2D MaskTexture;
uniform float Style;
uniform float BlurOffset;
uniform float MaskChannel;

in vec2 vTexCoord;
out vec4 fragColor;

float MaskValue(vec4 texel)
{
	if (MaskChannel < 0.5) return texel.r;
	if (MaskChannel < 1.5) return texel.g;
	if (MaskChannel < 2.5) return texel.b;
	return texel.a;
}

void main()
{
	vec4 mask = texture(MaskTexture, vTexCoord);
	if (MaskValue(mask) <= 0.0)
		discard;

	ivec2 worldSize = textureSize(WorldTexture, 0);
	ivec2 dst = ivec2(gl_FragCoord.xy);
	ivec2 src = clamp(dst + ivec2(int(BlurOffset), 0), ivec2(0), worldSize - ivec2(1));

	// OpenDUNE's blur draw uses the original sprite as a binary mask and copies
	// pixels from a positive horizontal framebuffer offset inside that mask.
	// The style uniform is kept active so one shader can serve the Sand/Sonic
	// renderer instances configured in world.yaml.
	fragColor = texelFetch(WorldTexture, src, 0) + vec4(0.0 * Style);
}
