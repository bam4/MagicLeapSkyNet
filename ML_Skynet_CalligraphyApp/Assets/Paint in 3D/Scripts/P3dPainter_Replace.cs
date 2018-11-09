using UnityEngine;
using System.Collections.Generic;

namespace PaintIn3D
{
	public static partial class P3dPainter
	{
		public class Replace : P3dCommand
		{
			[System.NonSerialized]
			private static Material cachedMaterial;

			[System.NonSerialized]
			private static Texture cachedTexture;

			[System.NonSerialized]
			private static Color cachedColor;

			[System.NonSerialized]
			private Material material;

			[System.NonSerialized]
			private Texture texture;

			[System.NonSerialized]
			private Color color;

			public override bool RequireSwap
			{
				get
				{
					return false;
				}
			}

			static Replace()
			{
				cachedMaterial = BuildMaterial("Hidden/Paint in 3D/Replace");
			}

			public static void SetMaterial(Texture texture)
			{
				cachedTexture = texture;
				cachedColor   = Color.white;
			}

			public static void SetMaterial(Texture texture, Color color, float opacity = 1.0f)
			{
				color.a *= opacity;

				cachedTexture = texture;
				cachedColor   = color;
			}

			public static void Blit(RenderTexture renderTexture, Texture texture)
			{
				cachedMaterial.SetTexture(P3dShader._Texture, texture);
				cachedMaterial.SetColor(P3dShader._Color, Color.white);

				Blit(cachedMaterial, renderTexture);
			}

			public static void Blit(RenderTexture renderTexture, Texture texture, Color color, float opacity = 1.0f)
			{
				color.a *= opacity;

				cachedMaterial.SetTexture(P3dShader._Texture, texture);
				cachedMaterial.SetColor(P3dShader._Color, color);

				Blit(cachedMaterial, renderTexture);
			}

			private static void Blit(Material cachedMaterial, RenderTexture renderTexture)
			{
				var oldActive = RenderTexture.active;

				RenderTexture.active = renderTexture;

				cachedMaterial.SetPass(0);

				Graphics.DrawMeshNow(P3dQuad.Mesh, Matrix4x4.identity, 0);

				RenderTexture.active = oldActive;
			}

			public override RenderAs Execute(RenderTexture buffer, P3dChannel channel)
			{
				material.SetTexture(P3dShader._Texture, texture);
				material.SetTexture(P3dShader._Buffer, buffer);
				material.SetColor(P3dShader._Color, color);
				material.SetPass(0);

				return RenderAs.Mesh;
			}

			public override void Pool()
			{
				pool.Add(this); poolCount++;
			}

			public static void CopyTo(Replace command)
			{
				command.material = cachedMaterial;
				command.texture  = cachedTexture;
				command.color    = cachedColor;
			}

			public static void Submit(P3dPaintableTexture paintableTexture, bool preview)
			{
				var command = paintableTexture.AddCommand(pool, ref poolCount, preview);

				CopyTo(command);
			}

			private static int poolCount;

			private static List<Replace> pool = new List<Replace>();
		}
	}
}