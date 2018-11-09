using UnityEngine;
using System.Collections.Generic;

namespace PaintIn3D
{
	public static partial class P3dPainter
	{
		public class Fill : P3dCommand
		{
			[System.NonSerialized]
			private static Material[] cachedMaterials;

			[System.NonSerialized]
			private static bool[] cachedSwaps;

			[System.NonSerialized]
			private static Material cachedMaterial;

			[System.NonSerialized]
			private static bool cachedSwap;

			[System.NonSerialized]
			private static Texture cachedTexture;

			[System.NonSerialized]
			private static Color cachedColor;

			[System.NonSerialized]
			private static float cachedOpacity;

			[System.NonSerialized]
			private Material material;

			[System.NonSerialized]
			private bool swap;

			[System.NonSerialized]
			private Texture texture;

			[System.NonSerialized]
			private Color color;

			[System.NonSerialized]
			private float opacity;

			public override bool RequireSwap
			{
				get
				{
					return swap;
				}
			}

			static Fill()
			{
				cachedMaterials = BuildMaterialsBlendModes("Hidden/Paint in 3D/Fill");
				cachedSwaps     = BuildSwapBlendModes();
			}

			public static void SetMaterial(P3dBlendMode blendMode, Texture texture)
			{
				cachedMaterial = cachedMaterials[(int)blendMode];
				cachedSwap     = cachedSwaps[(int)blendMode];
				cachedTexture  = texture;
				cachedColor    = Color.white;
				cachedOpacity  = 1.0f;
			}

			public static void SetMaterial(P3dBlendMode blendMode, Texture texture, Color color, float opacity)
			{
				cachedMaterial = cachedMaterials[(int)blendMode];
				cachedSwap     = cachedSwaps[(int)blendMode];
				cachedTexture  = texture;
				cachedColor    = color;
				cachedOpacity  = opacity;
			}

			public static bool Blit(P3dBlendMode blendMode, ref RenderTexture renderTexture, Texture texture)
			{
				cachedMaterial = cachedMaterials[(int)blendMode];
				cachedSwap     = cachedSwaps[(int)blendMode];
				cachedMaterial.SetTexture(P3dShader._Texture, texture);
				cachedMaterial.SetColor(P3dShader._Color, Color.white);
				cachedMaterial.SetFloat(P3dShader._Opacity, 1.0f);

				return Blit(cachedMaterial, ref renderTexture);
			}

			public static bool Blit(P3dBlendMode blendMode, ref RenderTexture renderTexture, Texture texture, Color color, float opacity = 1.0f)
			{
				cachedMaterial = cachedMaterials[(int)blendMode];
				cachedSwap     = cachedSwaps[(int)blendMode];
				cachedMaterial.SetTexture(P3dShader._Texture, texture);
				cachedMaterial.SetColor(P3dShader._Color, color);
				cachedMaterial.SetFloat(P3dShader._Opacity, opacity);

				return Blit(cachedMaterial, ref renderTexture);
			}

			private static bool Blit(Material cachedMaterial, ref RenderTexture renderTexture)
			{
				var oldActive = RenderTexture.active;
				var swapped   = false;

				if (cachedSwap == true)
				{
					var swap = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, renderTexture.depth, renderTexture.format);

					RenderTexture.active = swap;

					cachedMaterial.SetTexture(P3dShader._Buffer, renderTexture);

					cachedMaterial.SetPass(0);

					Graphics.DrawMeshNow(P3dQuad.Mesh, Matrix4x4.identity, 0);

					RenderTexture.ReleaseTemporary(renderTexture);

					swapped       = true;
					renderTexture = swap;
				}
				else
				{
					RenderTexture.active = renderTexture;

					cachedMaterial.SetPass(0);

					Graphics.DrawMeshNow(P3dQuad.Mesh, Matrix4x4.identity, 0);
				}

				RenderTexture.active = oldActive;

				return swapped;
			}

			public override RenderAs Execute(RenderTexture buffer, P3dChannel channel)
			{
				material.SetTexture(P3dShader._Buffer, buffer);
				material.SetTexture(P3dShader._Texture, texture);
				material.SetTexture(P3dShader._Buffer, buffer);
				material.SetColor(P3dShader._Color, color);
				material.SetFloat(P3dShader._Opacity, opacity);
				material.SetPass(0);

				return RenderAs.Mesh;
			}

			public override void Pool()
			{
				pool.Add(this); poolCount++;
			}

			public static void CopyTo(Fill command)
			{
				command.material = cachedMaterial;
				command.swap     = cachedSwap;
				command.texture  = cachedTexture;
				command.color    = cachedColor;
				command.opacity  = cachedOpacity;
			}

			public static void Submit(P3dPaintableTexture paintableTexture, bool preview)
			{
				var command = paintableTexture.AddCommand(pool, ref poolCount, preview);

				CopyTo(command);
			}

			private static int poolCount;

			private static List<Fill> pool = new List<Fill>();
		}
	}
}