using UnityEngine;

namespace PaintIn3D
{
	public abstract class P3dCommand
	{
		public enum RenderAs
		{
			Quad,
			Mesh
		}

		public bool      Preview;
		public Matrix4x4 Matrix;

		public abstract bool RequireSwap
		{
			get;
		}

		public abstract RenderAs Execute(RenderTexture buffer, P3dChannel channel);
		public abstract void Pool();
	}
}