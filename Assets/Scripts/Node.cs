using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int EValue { get; set; }
    public int Team { get; set; }
    public Node Parent { get; set; }
    public int Alpha { get; set; }
    public int Beta { get; set; }
    public int Value { get; set; }
    public Stack<Node> NodeChildren { get; set; }
    public int[,] MatrixNode {  get; set; } //here
    public Node BestChild { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public Node( Node parent, int team, int alpha, int beta, int x, int y)
    {
        Team = team;
        Parent = parent;
        Alpha = alpha;
        Beta = beta;
        Value = -team * 2;
        NodeChildren = new Stack<Node>();
        X = x;
        Y = y;
    }
}
