using UnityEngine;
using System.Collections.Generic;

namespace PaintIn3D
{
	public static partial class P3dHelper
	{
		public const string HelpUrlPrefix = "https://bitbucket.org/Darkcoder/paint-in-3d/wiki/";

		public const string ComponentMenuPrefix = "Paint in 3D/P3D ";

		public static Quaternion NormalToCameraRotation(Vector3 normal, Camera optionalCamera = null)
		{
			var up     = Vector3.up;
			var camera = GetCamera(optionalCamera);

			if (camera != null)
			{
				up = camera.transform.up;
			}

			return Quaternion.LookRotation(-normal, up);
		}

		// Return the current camera, or the main camera
		public static Camera GetCamera(Camera camera = null)
		{
			if (camera == null || camera.isActiveAndEnabled == false)
			{
				camera = Camera.main;
			}

			return camera;
		}

		public static bool IndexInMask(int index, LayerMask mask)
		{
			mask &= 1 << index;

			return mask != 0;
		}

		public static bool CanReadPixels(TextureFormat format)
		{
			if (format == TextureFormat.RGBA32 || format == TextureFormat.ARGB32 || format == TextureFormat.RGB24 || format == TextureFormat.RGBAFloat || format == TextureFormat.RGBAHalf)
			{
				return true;
			}

			return false;
		}

		public static void ReadPixels(Texture2D texture2D, RenderTexture renderTexture)
		{
			var oldActive = RenderTexture.active;

			RenderTexture.active = renderTexture;

			if (CanReadPixels(texture2D.format) == true)
			{
				texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

				RenderTexture.active = oldActive;

				texture2D.Apply();
			}
			else
			{
				var buffer = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);

				buffer.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

				RenderTexture.active = oldActive;

				var pixels = buffer.GetPixels32();

				Object.DestroyImmediate(buffer);

				texture2D.SetPixels32(pixels);
				texture2D.Apply();
			}
		}

		public static bool Downsample(RenderTexture renderTexture, int steps, ref RenderTexture temporary)
		{
			if (steps > 0 && renderTexture != null)
			{
				// Perform initial downsample to get buffer
				var width             = renderTexture.width  / 2;
				var height            = renderTexture.height / 2;
				var depth             = renderTexture.depth;
				var format            = renderTexture.format;
				var halfRenderTexture = RenderTexture.GetTemporary(width, height, depth, format);

				P3dPainter.Replace.Blit(halfRenderTexture, renderTexture);

				// Ping pong downsamples
				for (var i = 1; i < steps; i++)
				{
					width        /= 2;
					height       /= 2;
					renderTexture = halfRenderTexture;

					halfRenderTexture = RenderTexture.GetTemporary(width, height, depth, format);

					P3dPainter.Replace.Blit(halfRenderTexture, renderTexture);

					RenderTexture.ReleaseTemporary(renderTexture);
				}

				temporary = halfRenderTexture;

				return true;
			}

			return false;
		}

		public static Texture2D GetReadableTexture(Texture texture, TextureFormat format = TextureFormat.ARGB32, bool mipMaps = false, int width = 0, int height = 0)
		{
			var newTexture = default(Texture2D);

			if (texture != null)
			{
				if (width <= 0)
				{
					width = texture.width;
				}

				if (height <= 0)
				{
					height = texture.height;
				}

				if (CanReadPixels(format) == true)
				{
					var renderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

					newTexture = new Texture2D(width, height, format, mipMaps);

					P3dPainter.Replace.Blit(renderTexture, texture);

					var oldActive = RenderTexture.active;

					RenderTexture.active = renderTexture;

					newTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);

					RenderTexture.active = oldActive;

					RenderTexture.ReleaseTemporary(renderTexture);

					newTexture.Apply();
				}
			}

			return newTexture;
		}

		public static byte[] GetPngData(RenderTexture renderTexture)
		{
			var data = default(byte[]);

			if (renderTexture != null)
			{
				var tempTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
				var oldActive   = RenderTexture.active;

				RenderTexture.active = renderTexture;

				tempTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

				RenderTexture.active = oldActive;

				data = tempTexture.EncodeToPNG();

				Destroy(tempTexture);
			}

			return data;
		}

		public static void SaveTexture(RenderTexture renderTexture, string saveName, bool save = true)
		{
			if (renderTexture != null)
			{
				var tempTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
				var oldActive   = RenderTexture.active;

				RenderTexture.active = renderTexture;

				tempTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

				RenderTexture.active = oldActive;

				SaveTexture(tempTexture, saveName, save);

				Destroy(tempTexture);
			}
		}

		public static void SaveTexture(Texture2D texture2D, string saveName, bool save = true)
		{
			if (texture2D != null)
			{
				var bytes  = texture2D.EncodeToPNG();
				var base64 = System.Convert.ToBase64String(bytes);

				PlayerPrefs.SetString(saveName, base64);

				if (save == true)
				{
					PlayerPrefs.Save();
				}
			}
		}

		public static Texture2D LoadTexture(Texture2D texture2D, string saveName)
		{
			if (PlayerPrefs.HasKey(saveName) == true)
			{
				var base64 = PlayerPrefs.GetString(saveName);

				if (string.IsNullOrEmpty(base64) == false)
				{
					var bytes = System.Convert.FromBase64String(base64);

					if (bytes != null && bytes.Length > 0)
					{
						var created = false;

						if (texture2D == null)
						{
							texture2D = new Texture2D(1, 1);
							created   = true;
						}

						if (texture2D.LoadImage(bytes) == false && created == true)
						{
							texture2D = Destroy(texture2D);
						}
					}
				}
			}

			return texture2D;
		}

		public static void SetPngData(RenderTexture renderTexture, byte[] data)
		{
			if (data != null && data.Length > 0)
			{
				var tempTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);

				tempTexture.LoadImage(data, true);

				P3dPainter.Replace.Blit(renderTexture, tempTexture);

				Destroy(tempTexture);
			}
		}

		public static void LoadTexture(RenderTexture renderTexture, string saveName)
		{
			if (PlayerPrefs.HasKey(saveName) == true)
			{
				var base64 = PlayerPrefs.GetString(saveName);

				if (string.IsNullOrEmpty(base64) == false)
				{
					var data = System.Convert.FromBase64String(base64);

					SetPngData(renderTexture, data);
				}
			}
		}

		public static bool TextureExists(string saveName)
		{
			return PlayerPrefs.HasKey(saveName);
		}

		public static void ClearTexture(string saveName, bool save = true)
		{
			if (PlayerPrefs.HasKey(saveName) == true)
			{
				PlayerPrefs.DeleteKey(saveName);

				if (save == true)
				{
					PlayerPrefs.Save();
				}
			}
		}

		public static Texture2D CreateTexture(int width, int height, TextureFormat format, bool mipMaps)
		{
			if (width > 0 && height > 0)
			{
				return new Texture2D(width, height, format, mipMaps);
			}

			return null;
		}

		// This method allows you to easily find a Material attached to a GameObject
		public static Material GetMaterial(GameObject gameObject, int materialIndex = 0)
		{
			if (gameObject != null && materialIndex >= 0)
			{
				var renderer = gameObject.GetComponent<Renderer>();

				if (renderer != null)
				{
					var materials = renderer.sharedMaterials;

					if (materialIndex < materials.Length)
					{
						return materials[materialIndex];
					}
				}
			}

			return null;
		}

		// This method allows you to easily duplicate a Material attached to a GameObject
		public static Material CloneMaterial(GameObject gameObject, int materialIndex = 0)
		{
			if (gameObject != null && materialIndex >= 0)
			{
				var renderer = gameObject.GetComponent<Renderer>();

				if (renderer != null)
				{
					var materials = renderer.sharedMaterials;

					if (materialIndex < materials.Length)
					{
						// Get existing material
						var material = materials[materialIndex];

						// Clone it
						material = Object.Instantiate(material);

						// Update array
						materials[materialIndex] = material;

						// Update materials
						renderer.sharedMaterials = materials;

						return material;
					}
				}
			}

			return null;
		}

		// This method allows you to add a material (layer) to a renderer at the specified material index, or -1 for the end (top)
		public static Material AddMaterial(Renderer renderer, Shader shader, int materialIndex = -1)
		{
			if (renderer != null)
			{
				var newMaterials = new List<Material>(renderer.sharedMaterials);
				var newMaterial  = new Material(shader);

				if (materialIndex <= 0)
				{
					materialIndex = newMaterials.Count;
				}

				newMaterials.Insert(materialIndex, newMaterial);

				renderer.sharedMaterials = newMaterials.ToArray();

				return newMaterial;
			}

			return null;
		}

		public static float Reciprocal(float a)
		{
			return a != 0.0f ? 1.0f / a : 0.0f;
		}

		public static int Mod(int a, int b)
		{
			var m = a % b;

			if (m < 0)
			{
				return m + b;
			}

			return m;
		}

		public static Vector3 Reciprocal3(Vector3 xyz)
		{
			xyz.x = Reciprocal(xyz.x);
			xyz.y = Reciprocal(xyz.y);
			xyz.z = Reciprocal(xyz.z);

			return xyz;
		}

		public static float DampenFactor(float dampening, float elapsed)
		{
	#if UNITY_EDITOR
			if (Application.isPlaying == false)
			{
				return 1.0f;
			}
	#endif
			return 1.0f - Mathf.Pow((float)System.Math.E, -dampening * elapsed);
		}

		public static Bounds GetMatrixBounds(Matrix4x4 matrix)
		{
			var a = matrix.MultiplyPoint(new Vector3(-1.0f, -1.0f, -1.0f));
			var b = matrix.MultiplyPoint(new Vector3( 1.0f, -1.0f, -1.0f));
			var c = matrix.MultiplyPoint(new Vector3(-1.0f, -1.0f,  1.0f));
			var d = matrix.MultiplyPoint(new Vector3( 1.0f, -1.0f,  1.0f));
			var e = matrix.MultiplyPoint(new Vector3(-1.0f,  1.0f, -1.0f));
			var f = matrix.MultiplyPoint(new Vector3( 1.0f,  1.0f, -1.0f));
			var g = matrix.MultiplyPoint(new Vector3(-1.0f,  1.0f,  1.0f));
			var h = matrix.MultiplyPoint(new Vector3( 1.0f,  1.0f,  1.0f));

			var bounds = new Bounds(a, Vector3.zero);

			bounds.Encapsulate(b);
			bounds.Encapsulate(c);
			bounds.Encapsulate(d);
			bounds.Encapsulate(e);
			bounds.Encapsulate(f);
			bounds.Encapsulate(g);
			bounds.Encapsulate(h);

			return bounds;
		}

		// This allows you to destroy a UnityEngine.Object in edit or play mode
		public static T Destroy<T>(T o)
			where T : Object
		{
	#if UNITY_EDITOR
			if (Application.isPlaying == false)
			{
				Object.DestroyImmediate(o, true); return null;
			}
	#endif

			Object.Destroy(o);

			return null;
		}
	}
}