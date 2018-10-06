using System;
using System.Collections.Generic;
using LHGames.Helper;
using LHGames.Interfaces;

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

            List<IPlayer> targets = new List<IPlayer>();
            foreach(IPlayer player in visiblePlayers)
            {
                if (Point.Distance(PlayerInfo.Position, player.Position) <= 5f)
                    targets.Add(player);
            }

            targets.Sort((a, b) => Point.Distance(a.Position, PlayerInfo.Position).CompareTo(Point.Distance(b.Position, PlayerInfo.Position)));

            if (targets.Count == 0)
                action = MineResources(map);
            else
                action = AttackPlayer(targets[0], map);
                       
            var data = StorageHelper.Read<TestClass>("Test");
            Console.WriteLine(data?.Test);
            return action;
        }

        private string AttackPlayer(IPlayer player, Map map)
        {
            string action = "";
            if(Point.Distance(PlayerInfo.Position, player.Position) <= 1d)
            {
                action = AIHelper.CreateMeleeAttackAction(player.Position - PlayerInfo.Position);
            }
            else
            {
                action = GoTo(player.Position, map, true);
            }

            return action;
        }

        private string MineResources(Map map)
        {
            string action = "";
            bool isFull = PlayerInfo.CarriedResources >= PlayerInfo.CarryingCapacity;
            List<Tile> possibleResources = new List<Tile>();
            foreach (Tile tile in map.GetVisibleTiles())
            {
                if (tile.TileType == TileContent.Resource)
                {
                    possibleResources.Add(tile);
                }
            }

            // Sort resources
            possibleResources.Sort((a, b) => Point.Distance(a.Position, PlayerInfo.Position).CompareTo(Point.Distance(b.Position, PlayerInfo.Position)));

            Point adjacentResource = GetAdjacentResource(map);

            // prioritize this and upgrade
            if (PlayerInfo.Position == PlayerInfo.HouseLocation)
            {
                int carryingLevel = PlayerInfo.GetUpgradeLevel(UpgradeType.CarryingCapacity);
                int attackLevel = PlayerInfo.GetUpgradeLevel(UpgradeType.AttackPower);
                if (carryingLevel <= attackLevel && carryingLevel < 5
                    && UpgradeCosts[carryingLevel + 1] <= PlayerInfo.TotalResources)
                {
                    return AIHelper.CreateUpgradeAction(UpgradeType.CarryingCapacity);
                }
                else if (attackLevel < 5 && UpgradeCosts[attackLevel + 1] <= PlayerInfo.TotalResources)
                {
                    return AIHelper.CreateUpgradeAction(UpgradeType.AttackPower);
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
            else if (!isFull && adjacentResource != null)
            {
                action = AIHelper.CreateCollectAction(adjacentResource);
            }
            else if (isFull || possibleResources.Count == 0)
            {
                action = GoTo(PlayerInfo.HouseLocation, map, true);
            }

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
                {
                    if (content == TileContent.Wall)
                        return AIHelper.CreateMeleeAttackAction(direction);
                    else
                        return GoTo(location, map, false);

                }
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
                {
                    if (content == TileContent.Wall)
                        return AIHelper.CreateMeleeAttackAction(direction);
                    else
                        return GoTo(location, map, true);

                }
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

class BreadthFirstSearch
{
    static void Search(Map map, Point playerPos)
    {

        var frontier = new Queue<Point>();
        frontier.Enqueue(playerPos);

        var visited = new HashSet<Point>();
        visited.Add(playerPos);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            Console.WriteLine("Visiting {0}", current);

            var neighbors = new Queue<Point>();
            neighbors.Enqueue(new Point(playerPos.X, playerPos.Y - 1));
            neighbors.Enqueue(new Point(playerPos.X, playerPos.Y + 1));
            neighbors.Enqueue(new Point(playerPos.X + 1, playerPos.Y));
            neighbors.Enqueue(new Point(playerPos.X - 1, playerPos.Y));
            

            while(neighbors.Count != 0)
            {
                Point p = neighbors.Dequeue();

                if (!visited.Contains(p)) {
                    frontier.Enqueue(p);
                    visited.Add(p);
                }
            }
        }
    }
}

public class AStarSearch
{
    public Dictionary<Point, Point> cameFrom = new Dictionary<Point, Point>();
    public Dictionary<Point, double> costSoFar = new Dictionary<Point, double>();

    // Note: a generic version of A* would abstract over Location and
    // also Heuristic
    static public double Heuristic(Point a, Point b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    public AStarSearch(IWeightedGraph<Point> graph, Point start, Point goal)
    {
        var frontier = new PriorityQueue<Point>();
        frontier.Enqueue(start, 0);

        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (current.Equals(goal))
            {
                break;
            }

            foreach (var next in graph.Neighbors(current))
            {
                double newCost = costSoFar[current]
                    + graph.Cost(current, next);
                if (!costSoFar.ContainsKey(next)
                    || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    double priority = newCost + Heuristic(next, goal);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }
    }
}