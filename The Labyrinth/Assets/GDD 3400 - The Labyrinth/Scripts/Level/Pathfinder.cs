using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GDD3400.Labyrinth
{
    public class Pathfinder
    {
        public static List<PathNode> FindPath(PathNode startNode, PathNode endNode)
        {
            // List of nodes we might want to take
            List<PathNode> openSet = new List<PathNode>();

            // Nodes we've already looked at
            List<PathNode> closedSet = new List<PathNode>();

            // Saves path information back to start
            Dictionary<PathNode, PathNode> cameFromNode = new Dictionary<PathNode, PathNode>();

            // keeping track of our costs as we go
            Dictionary<PathNode, float> costSoFar = new Dictionary<PathNode, float>();
            Dictionary<PathNode, float> costToEnd = new Dictionary<PathNode, float>();


            // Initialized starting info
            openSet.Add(startNode);
            costSoFar[startNode] = 0;
            costToEnd[startNode] = Heuristic(startNode, endNode);


            while (openSet.Count > 0)
            {
                // Gets the lowest cost node to the end
                PathNode current = GetLowestCost(openSet, costToEnd);

                // We've found the goal, break out and return our path
                if (current == endNode) 
                    return ReconstructPath(cameFromNode, current);

                // Move the current node from open to closed
                openSet.Remove(current);
                closedSet.Add(current);


                foreach (var connection in current.Connections)
                {
                    PathNode neighbor = connection.Key;

                    // Ignores already evaluated nodes
                    if (closedSet.Contains(neighbor)) continue;

                    float tentativeCostFromStart = costSoFar[current] + connection.Value;

                    // If haven't looked at yet, add to open set
                    if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                    // Otherwise if cost from start is greater (longer path), skip this neighbor
                    else if (tentativeCostFromStart >= costSoFar[neighbor]) continue;

                    // Record best path, and update costs
                    cameFromNode[neighbor] = current;
                    costSoFar[neighbor] = tentativeCostFromStart;
                    costToEnd[neighbor] = costSoFar[neighbor] + Heuristic(neighbor, endNode);
                }
            }


            return new List<PathNode>(); // Return an empty path if no path is found
        }

        // Calculate the heuristic cost from the start node to the end node, manhattan distance
        private static float Heuristic(PathNode startNode, PathNode endNode)
        {
            return Vector3.Distance(startNode.transform.position, endNode.transform.position);
        }

        // Get the node in the provided open set with the lowest cost (eg closest to the end node)
        private static PathNode GetLowestCost(List<PathNode> openSet, Dictionary<PathNode, float> costs)
        {
            PathNode lowest = openSet[0];
            float lowestCost = costs[lowest];

            foreach (var node in openSet)
            {
                float cost = costs[node];
                if (cost < lowestCost)
                {
                    lowestCost = cost;
                    lowest = node;
                }
            }

            return lowest;
        }

        // Reconstruct the path from the cameFrom map
        private static List<PathNode> ReconstructPath(Dictionary<PathNode, PathNode> cameFrom, PathNode current)
        {
            List<PathNode> totalPath = new List<PathNode> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Insert(0, current);
            }
            return totalPath;
        }
    }
}
