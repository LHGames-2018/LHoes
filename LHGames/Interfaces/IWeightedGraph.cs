using System;
using LHGames.Helper;
using System.Collections.Generic;

namespace LHGames.Interfaces
{
    public interface IWeightedGraph<L>
    {   
        double Cost(Point a, Point b);
        IEnumerable<Point> Neighbors(Point id);
    }
}
