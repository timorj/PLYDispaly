﻿#version 330 core
layout(location = 0) in vec3 lightPos;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;


void main(void){
	gl_Position = vec4(lightPos.xyz,1.0)* model * view * projection;
}
