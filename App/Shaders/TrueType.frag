#version 330

in vec4 color;
in vec2 texCoord;

uniform sampler2D aTexture;

out vec4 outputColor;

void main()
{
	outputColor = vec4(color.xyz, texture(aTexture, texCoord).w * color.w);
}