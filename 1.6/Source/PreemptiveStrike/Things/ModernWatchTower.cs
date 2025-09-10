using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PreemptiveStrike.Things
{
	class ModernWatchTower : TowerBuildingBase
	{
		protected override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			base.DrawAt(drawLoc);
			Vector3 drawPos = drawLoc;
			drawPos.y += 5.905f;
			//drawPos.z += 1.1f;
			//drawPos.x += 0.5f;
			Quaternion quaternion = Quaternion.identity;
			Color white = Color.white;
			Material material = Textures.FenceGraphics.MatAt(base.Rotation, null);
			if (white != material.color)
			{
				material = MaterialPool.MatFrom((Texture2D)material.mainTexture, ShaderDatabase.Transparent, white);
			}
			//Graphic coloredVersion = GraphicsCache.BathOver.GetColoredVersion(base.Graphic.Shader, this.DrawColor, this.DrawColorTwo);
			//Graphics.DrawMesh(coloredVersion.MeshAt(base.Rotation), drawPos, quaternion, coloredVersion.MatAt(base.Rotation, null), 0);
			//drawPos.y -= 0.01f;
			Graphics.DrawMesh(Textures.FenceGraphics.MeshAt(base.Rotation), drawPos, quaternion, material, 0);
		}

		/*Patrol Points*/
		public override int PatrolPointCnt => 4;
		public override int PatrolIntervalTick => 400;
		public override int RotateIntervalTick => 100;

		public override IntVec3 GetPatrolPos(int x)
		{
			IntVec3[] Pos = new IntVec3[] {
				InteractionCell,
				InteractionCell + new IntVec3(1, 0, 0),
				InteractionCell,
				InteractionCell + new IntVec3(-1, 0, 0),
			};
			return Pos[x];
		}

		public override Rot4 GetPatrolRot(int x)
		{
			Rot4[] PatrolPosRotException = new Rot4[] { Rot4.North, Rot4.West, Rot4.North, Rot4.East };
			return RotateToRandomeExcept(PatrolPosRotException[x]);
		}

		private Rot4 RotateToRandomeExcept(Rot4 exception)
		{
			List<Rot4> avl = new List<Rot4>();
			if (exception != Rot4.North)
				avl.Add(Rot4.North);
			if (exception != Rot4.South)
				avl.Add(Rot4.South);
			if (exception != Rot4.West)
				avl.Add(Rot4.West);
			if (exception != Rot4.East)
				avl.Add(Rot4.East);
			return avl.RandomElement();
		}
	}
}
