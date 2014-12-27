#version 440

in vec3 vPosition;
in vec2 vUv;
in vec3 vColor;
out vec4 color;
out vec2 uv;

uniform mat4 modelview;
uniform ColorBlock
{
 vec4 color1;
 vec4 color2;
};

void main()
{
	uv = vUv;
	gl_Position = modelview * vec4 ( vPosition, 1.0 );
	color = vec4 ( vColor, 1.0 ) + color1 + color2;
}
