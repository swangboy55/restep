﻿using System.Collections.Generic;
using OpenTK;
using restep.Graphics;
using restep.Graphics.Shaders;

namespace restep.Framework
{
    /// <summary>
    /// Stores global runtime data for this program
    /// </summary>
    internal class RestepGlobals
    {
        //TODO: move this elsewhere
        #region ~base shader code~

        public static readonly string FXAA_VTX =
            @"#version 330
              precision highp float;
              layout (location = 0) in vec2 position;
              
              out vec2 texCoord;

              void main()
              {
                  gl_Position = vec4(position, 0.0, 1.0);
                  texCoord = (position + 1.0) / 2.0;
              }";

        public static readonly string FXAA_FRAG =
            @"
            #version 330
            precision highp float;
              
            out vec4 fragColor;
            in vec2 texCoord;

            uniform sampler2D diffuse;

            uniform vec3 frameBufferSize;
            uniform float FXAA_SPAN_MAX;
            uniform float FXAA_REDUCE_MUL;
            uniform float FXAA_REDUCE_MIN;

            void main( )
            {
	            vec4 temp;
	            vec2 frameBufSize = vec2( 1.0 / frameBufferSize.x, 1.0 / frameBufferSize.y );

                vec3 rgbNW = texture2D( diffuse, texCoord + ( vec2( -1.0, -1.0 ) * frameBufSize ) ).xyz;
                vec3 rgbNE = texture2D( diffuse, texCoord + ( vec2( 1.0, -1.0 ) * frameBufSize ) ).xyz;
                vec3 rgbSW = texture2D( diffuse, texCoord + ( vec2( -1.0, 1.0 ) * frameBufSize ) ).xyz;
                vec3 rgbSE = texture2D( diffuse, texCoord + ( vec2( 1.0, 1.0 ) * frameBufSize ) ).xyz;
                vec3 rgbM = texture2D( diffuse, texCoord ).xyz;

                vec3 luma = vec3( 0.299, 0.587, 0.114 );

                float lumaNW = dot( rgbNW, luma );
                float lumaNE = dot( rgbNE, luma );
                float lumaSW = dot( rgbSW, luma );
                float lumaSE = dot( rgbSE, luma );
                float lumaM  = dot( rgbM,  luma );
        
                float lumaMin = min( lumaM, min( min( lumaNW, lumaNE ), min( lumaSW, lumaSE ) ) );
                float lumaMax = max( lumaM, max( max( lumaNW, lumaNE ), max( lumaSW, lumaSE ) ) );
        
                vec2 dir;
                dir.x = -( ( lumaNW + lumaNE ) - ( lumaSW + lumaSE ) );
                dir.y =  ( ( lumaNW + lumaSW ) - ( lumaNE + lumaSE ) );
        
                float dirReduce = max( 
    	            ( lumaNW + lumaNE + lumaSW + lumaSE ) * ( 0.25 * FXAA_REDUCE_MUL ), FXAA_REDUCE_MIN );
          
                float rcpDirMin = 1.0 / ( min( abs( dir.x ), abs( dir.y ) ) + dirReduce );
        
                dir = min( vec2(  FXAA_SPAN_MAX,  FXAA_SPAN_MAX ),
                              max( vec2( -FXAA_SPAN_MAX, -FXAA_SPAN_MAX ),
                              dir * rcpDirMin ) ) * frameBufSize;
                
                vec3 rgbA = ( 1.0 / 2.0 ) * ( 
                        texture2D( diffuse, texCoord.xy + dir * ( 1.0 / 3.0 - 0.5 ) ).xyz +
                        texture2D( diffuse, texCoord.xy + dir * ( 2.0 / 3.0 - 0.5 ) ).xyz );
                vec3 rgbB = rgbA * ( 1.0 / 2.0 ) + ( 1.0 / 4.0 ) * ( 
                        texture2D( diffuse, texCoord.xy + dir * ( 0.0 / 3.0 - 0.5 ) ).xyz +
                        texture2D( diffuse, texCoord.xy + dir * ( 3.0 / 3.0 - 0.5 ) ).xyz );
                float lumaB = dot( rgbB, luma );

                if( ( lumaB < lumaMin ) || ( lumaB > lumaMax ) )
                    temp = vec4( rgbA, 1 );
                else
    	            temp = vec4( rgbB, 1 );
                fragColor.r = temp.r*.393 + temp.g*.769 + temp.b * .189;
	            fragColor.g = temp.r*.349 + temp.g*.686 + temp.b * .168;
	            fragColor.b = temp.r*.272 + temp.g*.534 + temp.b * .131;
	            fragColor.a = 1;
	            fragColor = temp;
            }";

        //cut down on CPU mat calculations by making mat3
        public static readonly string TEX_SHADER_VTX =
            @"#version 330
              precision highp float;
              layout (location = 0) in vec2 position;
              layout (location = 1) in vec2 texCoord;
              
              out vec2 tcoord0;

              uniform mat3 transform;
              uniform vec2 origin;
              
              void main()
              {
                  tcoord0 = texCoord;
                  vec3 positionTransform = transform * vec3((position - origin), 1.0);
                  gl_Position = vec4(positionTransform.xy, 0.0, 1.0);
              }";

        public static readonly string TEX_SHADER_FRAG =
            @"#version 330
              precision highp float;
              out vec4 fragColor;

              uniform sampler2D diffuse;
              uniform float transparency;
              
              in vec2 tcoord0;

              void main()
              {
                  fragColor = texture2D(diffuse, tcoord0);
                  fragColor.w *= transparency;
              }";

        public static readonly string COLOR_SHADER_VTX =
            @"#version 330
              precision highp float;
              layout (location = 0) in vec2 position;

              uniform mat3 transform;
              uniform vec2 origin;

              void main()
              {
                  vec3 positionTransform = transform * vec3((position - origin), 1.0);
                  gl_Position = vec4(positionTransform.xy, 0.0, 1.0);
              }";

        public static readonly string COLOR_SHADER_FRAG =
            @"#version 330
              precision highp float;
              out vec4 fragColor;

              uniform vec4 color;

              void main()
              {
                  fragColor = color;
              }";//              uniform vec4 colorA;mix(colorA, colorB, grdAt)
        //vec3 positionTransform = transform * vec3((position - origin), 1.0);

        #endregion

        /// <summary>
        /// List of shaders which will be used by all meshes (if enabled)
        /// </summary>
        public static List<Shader> LoadedShaders { get; private set; } = new List<Shader>();
        
        /// <summary>
        /// Size of the Content Area of the RestepWindow
        /// </summary>
        public static Vector2 ContentAreaSize
        {
            get
            {
                if(RestepWindow.Initialized)
                {
                    return new Vector2(RestepWindow.Instance.ClientSize.Width, RestepWindow.Instance.ClientSize.Height);
                }

                return new Vector2(0, 0);
            }
        }
    }
}
