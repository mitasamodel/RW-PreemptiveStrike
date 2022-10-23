using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace PreemptiveStrike
{
    //Lt.Bob Test for better performance
    public class PESMapComponent : MapComponent
    {
        public HashSet<Building> PESDetectors = new HashSet<Building>();
        public PESMapComponent (Map map) : base(map)
        {
        }

        public void AddDetectorToMap(Building thing)
        {
            if (!PESDetectors.Contains(thing))
                PESDetectors.Add(thing);
        }
        public void RemoveDetectorToMap(Building thing)
        {
            if (PESDetectors.Contains(thing))
                PESDetectors.Remove(thing);
        }
    }
}

namespace PreemptiveStrike.Things
{
    class TowerBuildingBase : Building
    {
        public virtual int PatrolPointCnt => 1;
        public virtual int PatrolIntervalTick => 10000000;
        public virtual int RotateIntervalTick => 300;

        public virtual IntVec3 GetPatrolPos(int x)
        {
            return InteractionCell;
        }

        public virtual Rot4 GetPatrolRot(int x)
        {
            return Rot4.Random;
        }

        //Lt.Bob Test for better performance
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            PESMapComponent mapComp = map.GetComponent<PESMapComponent>();
            if (mapComp != null)
                mapComp.AddDetectorToMap(this);
            base.SpawnSetup(map, respawningAfterLoad);
        }
        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            PESMapComponent mapComp = this.Map.GetComponent<PESMapComponent>();
            if (mapComp != null)
                mapComp.RemoveDetectorToMap(this);
            base.DeSpawn(mode);
        }
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            PESMapComponent mapComp = this.Map.GetComponent<PESMapComponent>();
            if (mapComp != null)
                mapComp.RemoveDetectorToMap(this);
            base.Destroy(mode);
        }
    }
}
