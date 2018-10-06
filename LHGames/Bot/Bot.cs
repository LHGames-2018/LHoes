using System;
using System.Collections.Generic;
using LHGames.Helper;

namespace LHGames.Bot
{
    internal class Bot
    {
        internal IPlayer PlayerInfo { get; set; }
        private int _currentDirection = 1;

        public static Dictionary<int, int> UpgradeCosts = new Dictionary<int, int>()
    {
        {0, 0 },
        {1, 10000 },
        {2, 15000 },
        {3, 25000 },
        {4, 50000 },
        {5, 100000 }
    };

        internal Bot() { }

        /// <summary>
        /// Gets called before ExecuteTurn. This is where you get your bot's state.
        /// </summary>
        /// <param name="playerInfo">Your bot's current state.</param>
        internal void BeforeTurn(IPlayer playerInfo)
        {
            PlayerInfo = playerInfo;
        }

        /// <summary>
        /// Implement your bot here.
        /// </summary>
        /// <param name="map">The gamemap.</param>
        /// <param name="visiblePlayers">Players that are visible to your bot.</param>
        /// <returns>The action you wish to execute.</returns>
        internal string ExecuteTurn(Map map, IEnumerable<IPlayer> visiblePlayers)
        {
            string action = "";
            bool isFull = PlayerInfo.CarriedResources >= PlayerInfo.CarryingCapacity;
            List<Tile> possibleResources = new List<Tile>();
            foreach (Tile tile in map.GetVisibleTiles())
            {
                double distance = Point.Distance(PlayerInfo.HouseLocation, tile.Position);//MathF.Sqrt(MathF.Pow(PlayerInfo.HouseLocation.X - tile.Position.X, 2)
                    //+ MathF.Pow(PlayerInfo.HouseLocation.Y - tile.Position.Y, 2));
                if (tile.TileType == TileContent.Resource && distance < 8f)
                {
                    possibleResources.Add(tile);
                }
            }

            // Sort resources
            possibleResources.Sort((a, b) => Point.Distance(a.Position, PlayerInfo.Position).CompareTo(Point.Distance(b.Position, PlayerInfo.Position)));

            Point adjacentResource = GetAdjacentResource(map);

            // prioritize this and upgrade
            if(PlayerInfo.Position == PlayerInfo.HouseLocation)
            {
                int collectingLevel = PlayerInfo.GetUpgradeLevel(UpgradeType.CollectingSpeed);
                int carryingLevel = PlayerInfo.GetUpgradeLevel(UpgradeType.CarryingCapacity);
                if (collectingLevel <= carryingLevel && collectingLevel < 5 
                    && UpgradeCosts[collectingLevel + 1] <= PlayerInfo.TotalResources)
                {
                    return AIHelper.CreateUpgradeAction(UpgradeType.CollectingSpeed);
                }
                else if (carryingLevel < 5 && UpgradeCosts[carryingLevel + 1] <= PlayerInfo.TotalResources)
                {
                    return AIHelper.CreateUpgradeAction(UpgradeType.CarryingCapacity);
                }
            }

            if (!isFull && adjacentResource == null && possibleResources.Count > 0)
            {        
                if (possibleResources.Count > 0)
                    action = GoTo(possibleResources[0].Position, map, true);
                else
                {
                    Console.Out.WriteLine("Oups, no action for resource");
                    action = AIHelper.CreateEmptyAction();
                }
            }
            else if(!isFull && adjacentResource != null)
            {
                action = AIHelper.CreateCollectAction(adjacentResource);
            }
            else if(isFull || possibleResources.Count == 0)
            {
                action = GoTo(PlayerInfo.HouseLocation, map, true);
            }

            var data = StorageHelper.Read<TestClass>("Test");
            Console.WriteLine(data?.Test);
            return action;
        }

        private Point GetAdjacentResource(Map map)
        {
            if (map.GetTileAt(PlayerInfo.Position.X + 1, PlayerInfo.Position.Y) == TileContent.Resource)
                return new Point(1, 0);
            else if (map.GetTileAt(PlayerInfo.Position.X - 1, PlayerInfo.Position.Y) == TileContent.Resource)
                return new Point(-1, 0);
            else if (map.GetTileAt(PlayerInfo.Position.X, PlayerInfo.Position.Y + 1) == TileContent.Resource)
                return new Point(0, 1);
            else if (map.GetTileAt(PlayerInfo.Position.X, PlayerInfo.Position.Y - 1) == TileContent.Resource)
                return new Point(0, -1);
            else
                return null;
        }

        private string CollectRessource(Point direction)
        {
            return AIHelper.CreateCollectAction(direction);
        }

        private string GoTo(Point location, Map map, bool moveHorizontally)
        {
            Point direction = null;
            if (moveHorizontally)
            {
                if (PlayerInfo.Position.X != location.X)
                    direction = new Point(MathF.Sign(location.X - PlayerInfo.Position.X), 0);
                else
                    return GoTo(location, map, false);

                TileContent content = map.GetTileAt(PlayerInfo.Position.X + direction.X, PlayerInfo.Position.Y + direction.Y);
                if (content != TileContent.Empty && content != TileContent.House)
                    return GoTo(location, map, false);
                else
                    return AIHelper.CreateMoveAction(direction);
            }
            else
            {
                if (PlayerInfo.Position.Y != location.Y)
                    direction = new Point(0, MathF.Sign(location.Y - PlayerInfo.Position.Y));
                else
                    return GoTo(location, map, true);

                TileContent content = map.GetTileAt(PlayerInfo.Position.X + direction.X, PlayerInfo.Position.Y + direction.Y);
                if (content != TileContent.Empty && content != TileContent.House)
                    return GoTo(location, map, true);
                else
                    return AIHelper.CreateMoveAction(direction);
            }
        }

        /// <summary>
        /// Gets called after ExecuteTurn.
        /// </summary>
        internal void AfterTurn()
        {
        }
    }
}

class TestClass
{
    public string Test { get; set; }
}