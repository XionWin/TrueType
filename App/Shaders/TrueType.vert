#version 330 core

// the position variable has attribute position 0
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec4 aColor;
layout(location = 2) in vec2 aTexCoord;

out vec2 texCoord; // output a color to the fragment shader
out vec4 color; // output a color to the fragment shader

uniform vec3 aViewport; 

void main(void)
{
    gl_Position = vec4(aPos.x / aViewport.x * 2.0 - 1.0, 1.0 - aPos.y / aViewport.y * 2.0, 0.0, 1.0);
	texCoord = aTexCoord;
	color = aColor;
}