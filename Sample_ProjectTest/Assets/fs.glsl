﻿#version 440

in vec2 uv;
in vec4 color;
out vec4 outputColor;

uniform sampler2D diffuse;

void main()
{
	vec4 col = texture ( diffuse, uv );
	outputColor = vec4 ( col.rgb, 1.0 );
}