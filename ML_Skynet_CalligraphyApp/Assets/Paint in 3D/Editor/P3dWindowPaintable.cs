﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PaintIn3D
{
	public class P3dWindowPaintable
	{
		public GameObject Root;

		private Mesh bakedMesh;

		private Mesh lastMesh;

		private Matrix4x4 lastMatrix;

		//private Bounds lastBounds;

		public List<P3dWindowPaintableTexture> PaintableTextures = new List<P3dWindowPaintableTexture>();

		public P3dWindowPaintable(GameObject newRoot)
		{
			Root = newRoot;
		}

		public bool CanPaint()
		{
			for (var i = PaintableTextures.Count - 1; i >= 0; i--)
			{
				if (PaintableTextures[i].Locked == true)
				{
					return true;
				}
			}

			return false;
		}

		public bool CanUndo()
		{
			for (var i = PaintableTextures.Count - 1; i >= 0; i--)
			{
				if (PaintableTextures[i].CanUndo() == true)
				{
					return true;
				}
			}

			return false;
		}

		public bool CanWrite()
		{
			for (var i = PaintableTextures.Count - 1; i >= 0; i--)
			{
				if (PaintableTextures[i].CanWrite() == true)
				{
					return true;
				}
			}

			return false;
		}

		public bool NeedsWrite()
		{
			for (var i = PaintableTextures.Count - 1; i >= 0; i--)
			{
				if (PaintableTextures[i].NeedsWrite() == true)
				{
					return true;
				}
			}

			return false;
		}

		public bool CanRedo()
		{
			for (var i = PaintableTextures.Count - 1; i >= 0; i--)
			{
				if (PaintableTextures[i].CanRedo() == true)
				{
					return true;
				}
			}

			return false;
		}

		public void Undo()
		{
			for (var i = PaintableTextures.Count - 1; i >= 0; i--)
			{
				PaintableTextures[i].Undo();
			}
		}

		public void Write()
		{
			for (var i = PaintableTextures.Count - 1; i >= 0; i--)
			{
				PaintableTextures[i].Write();
			}
		}

		public void Redo()
		{
			for (var i = PaintableTextures.Count - 1; i >= 0; i--)
			{
				PaintableTextures[i].Redo();
			}
		}

		public bool Raycast(Ray ray, ref RaycastHit hit)
		{
			var skinnedMeshRenderer = Root.GetComponent<SkinnedMeshRenderer>();

			if (skinnedMeshRenderer != null)
			{
				if (skinnedMeshRenderer.sharedMesh != null)
				{
					if (bakedMesh == null)
					{
						bakedMesh = new Mesh();
					}

					var scaling    = P3dHelper.Reciprocal3(Root.transform.lossyScale);
					var localScale = Root.transform.localScale;

					Root.transform.localScale = Vector3.one;

					skinnedMeshRenderer.BakeMesh(bakedMesh);

					Root.transform.localScale = localScale;

					lastMesh   = bakedMesh;
					lastMatrix = Root.transform.localToWorldMatrix;
					//lastBounds = skinnedMeshRenderer.bounds;

					var matrix = lastMatrix * Matrix4x4.Scale(scaling);

					if (P3dIntersect.IntersectRayMesh(ray, bakedMesh, matrix, out hit) == true)
					{
						return true;
					}
				}
			}
			else
			{
				var meshRenderer = Root.GetComponent<MeshRenderer>();

				if (meshRenderer != null)
				{
					var meshFilter = Root.GetComponent<MeshFilter>();

					if (meshFilter != null)
					{
						var mesh = meshFilter.sharedMesh;

						if (mesh != null)
						{
							lastMesh   = mesh;
							lastMatrix = Root.transform.localToWorldMatrix;
							//lastBounds = meshRenderer.bounds;

							if (P3dIntersect.IntersectRayMesh(ray, mesh, lastMatrix, out hit) == true)
							{
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		public void Paint(P3dGroup group, P3dCommand command)
		{
			//if (bounds.Intersects(lastBounds) == true)
			{
				for (var i = PaintableTextures.Count - 1; i >= 0; i--)
				{
					var paintableTexture = PaintableTextures[i];
					var renderTexture    = paintableTexture.PreparePaint(); // Prepare the paint regardless, to sync undo states

					if (paintableTexture.Group == group)
					{
						var oldActive = RenderTexture.active;
						var buffer    = default(RenderTexture);

						RenderTexture.active = renderTexture;

						if (command.RequireSwap == true)
						{
							buffer = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, renderTexture.depth, renderTexture.format);

							P3dPainter.Replace.Blit(buffer, renderTexture);
						}

						if (command.Execute(buffer, paintableTexture.Channel) == P3dCommand.RenderAs.Mesh)
						{
							Graphics.DrawMeshNow(lastMesh, lastMatrix, paintableTexture.MaterialIndex);
						}
						else
						{
							Graphics.DrawMeshNow(P3dQuad.Mesh, Matrix4x4.identity, 0);
						}

						RenderTexture.active = oldActive;

						if (buffer != null)
						{
							RenderTexture.ReleaseTemporary(buffer);
						}
					}
				}
			}
		}

		public void Apply()
		{
			for (var i = PaintableTextures.Count - 1; i >= 0; i--)
			{
				PaintableTextures[i].Apply();
			}
		}

		public void Revert()
		{
			for (var i = PaintableTextures.Count - 1; i >= 0; i--)
			{
				PaintableTextures[i].Revert();
			}
		}

		public void Unlock()
		{
			for (var i = PaintableTextures.Count - 1; i >= 0; i--)
			{
				PaintableTextures[i].Unlock();
			}
		}
	}
}