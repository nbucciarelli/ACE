using System;
using System.Collections.Generic;
using ACE.Server.WorldObjects;

namespace ACE.Server.Riptide.Managers
{
    public interface IPlayerManager
    {
        IEnumerable<Player> GetPlayersInRange(WorldObject o, float range);
    }

    internal class RiptidePlayerManager: IPlayerManager
    {
        internal RiptidePlayerManager()
        {
        }

        public IEnumerable<Player> GetPlayersInRange(WorldObject o, float range)
        {
            List<Player> result = new List<Player>();
            var isDungeon = o.CurrentLandblock != null && o.CurrentLandblock.PhysicsLandblock != null && o.CurrentLandblock.PhysicsLandblock.IsDungeon;

            var rangeSquared = range * range;

            foreach (var player in o.PhysicsObj.ObjMaint.GetKnownPlayersValuesAsPlayer())
            {
                if (isDungeon && o.Location.Landblock != player.Location.Landblock)
                    continue;

                if (o.Visibility && !player.Adminvision)
                    continue;

                //var dist = Vector3.Distance(Location.ToGlobal(), player.Location.ToGlobal());
                //var distSquared = Vector3.DistanceSquared(Location.ToGlobal(), player.Location.ToGlobal());
                var distSquared = o.Location.SquaredDistanceTo(player.Location);
                if (distSquared <= rangeSquared)
                    result.Add(player);
            }
            return result;
        }
    }
}
