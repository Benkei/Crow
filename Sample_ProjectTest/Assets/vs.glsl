#version 330

in vec3 vPosition;
in vec2 vUv;
in vec3 vColor;
out vec4 color;
out vec2 uv;
uniform mat4 modelview;

void main()
{
	uv = vUv;
	gl_Position = modelview * vec4 ( vPosition, 1.0 );
	color = vec4 ( vColor, 1.0 );
}
