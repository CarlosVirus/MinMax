using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
public enum States
{
    CanMove,
    CantMove
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public BoxCollider2D collider;
    public GameObject token1, token2;
    public int Size = 3;
    public int[,] Matrix;
    public int[,] MatrixAuxiliar;
    [SerializeField] private States state = States.CanMove;
    public Camera camera;
    public int lastMoveX, lastMoveY;
    public Stack<Node> stack;
    void Start()
    {
        Instance = this;
        Matrix = new int[Size, Size];
        MatrixAuxiliar = new int[Size, Size];
        Calculs.CalculateDistances(collider, Size);
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                Matrix[i, j] = 0; // 0: desocupat, 1: fitxa jugador 1, -1: fitxa IA;
            }
        }
        stack = new Stack<Node>();
    }
    private void Update()
    {
        if (state == States.CanMove)
        {
            Vector3 m = Input.mousePosition;
            m.z = 10f;
            Vector3 mousepos = camera.ScreenToWorldPoint(m);
            if (Input.GetMouseButtonDown(0))
            {
                if (Calculs.CheckIfValidClick((Vector2)mousepos, Matrix))
                {
                    state = States.CantMove;
                    if(Calculs.EvaluateWin(Matrix)==2)
                        StartCoroutine(WaitingABit());
                }
            }
        }
    }
    private IEnumerator WaitingABit()
    {
        yield return new WaitForSeconds(1f);
        RandomAI();
    }
    public void RandomAI()
    {
        /*int x;
        int y;
        do
        {
            x = Random.Range(0, Size);
            y = Random.Range(0, Size);
        } while (Matrix[x, y] != 0);
        DoMove(x, y, -1);*/
        Node startNode = new Node(null, -1, -2, 2, lastMoveX, lastMoveY);
        GenerateNodes(startNode);
        MinMaxAlphaBetaPruning(startNode);
        Debug.Log(" x " + startNode.BestChild.X + " y " + startNode.BestChild.Y);
        DoMove(startNode.BestChild.X, startNode.BestChild.Y, -1);
        state = States.CanMove;
    }
    public void DoMove(int x, int y, int team)
    {
        Matrix[x, y] = team;
        if (team == 1)
            Instantiate(token1, Calculs.CalculatePoint(x, y), Quaternion.identity);
        else
            Instantiate(token2, Calculs.CalculatePoint(x, y), Quaternion.identity);
        int result = Calculs.EvaluateWin(Matrix);
        switch (result)
        {
            case 0:
                Debug.Log("Draw");
                break;
            case 1:
                Debug.Log("You Win");
                break;
            case -1:
                Debug.Log("You Lose");
                break;
            case 2:
                if(state == States.CantMove)
                    state = States.CanMove;
                break;
        }
    }
    public void GenerateNodes(Node node)
    {
        for(int i=0; i< Size; i++)
        {
            for(int j=0; j< Size; j++)
            {
                if (MatrixAuxiliar[i,j] == 0)
                {
                    Node newNode = new Node(node, -1 * node.Team, node.Alpha, node.Beta, i, j);
                    CopyMatrix();
                    BuildAuxMatrix(newNode);
                    MatrixAuxiliar[i, j] = node.Team;
                    newNode.EValue = Calculs.EvaluateWin(MatrixAuxiliar);
                    DebugMatrixAux();
                    node.NodeChildren.Push(newNode);
                    if(newNode.EValue==2) GenerateNodes(newNode);
                    else
                        CopyMatrix();
                }
            }
        }
    }
    public void CopyMatrix()
    {
        for( int i=0; i< Size; ++i)
        {
            for(int j=0; j< Size; ++j)
            {
                MatrixAuxiliar[i, j] = Matrix[i, j];
            }
        }
    }

    public void DebugMatrixAux()
    {
        string text = "";
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                text += MatrixAuxiliar[i, j] + " ";
            }
            text += "\n";
        }
        Debug.Log(text); 
    }
    public void BuildAuxMatrix(Node node) //bucle infinito
    {
        if(node.Parent != null)
        {
            MatrixAuxiliar[node.Parent.X, node.Parent.Y] = -node.Parent.Team;
            //Debug.Log()
            BuildAuxMatrix(node.Parent);
        }
    }
    public void MinMaxAlphaBetaPruning(Node node)
    {
        List<Node> nodes = new List<Node>();
        while(node.NodeChildren.Count!=0)
        {
            /*if (Mathf.Abs(node.NodeChildren.Peek().Value) != 2)
            {
                AlphaBetaPruningNewStack(node);
                if(node.Team == -1)
                {
                    if (node.Value < node.Alpha) break;
                }
                else
                {
                    if (node.Value > node.Beta) break;
                }
            }
            else*/
                stack.Push(node.NodeChildren.Peek());
            nodes.Add(node.NodeChildren.Pop());
            foreach (Node n in nodes)
                MinMaxAlphaBetaPruning(n);
        }

        if(stack.Count!=0)
        {
            //if (Mathf.Abs(stack.Peek().Value) != 2) //stack overflow peek
            //{
            if(stack.Peek().Parent != null)
                AlphaBetaPruningStack(stack.Pop());
            /*}
            else
                MinMaxAlphaBetaPruning(stack.Pop());*/
        }
    }  
    public void AlphaBetaPruningNewStack(Node node)
    {
        int value = node.NodeChildren.Peek().Value;
        if (node.Team == -1)
        {
            node.Value = Mathf.Min(node.Value, value);
            //node.Beta = Mathf.Min(node.Beta, value);
            //if (node.Beta == value) node.BestChild = node.NodeChildren.Pop();
            if (node.Value == value) node.BestChild = node.NodeChildren.Pop();
        }
        else
        {
            node.Value = Mathf.Max(node.Value, value);
            //node.Alpha = Mathf.Max(node.Alpha, value);
            //if (node.Alpha == value) node.BestChild = node.NodeChildren.Pop();
            if (node.Value == value) node.BestChild = node.NodeChildren.Pop();
        }
    }
    public void AlphaBetaPruningStack(Node node)
    {
        //int value = stack.Peek().Value;
        int value = node.Parent.Value;
        if (node.Parent.Team == -1)
        {
            node.Parent.Value = Mathf.Min(node.Value, value);
            //node.Beta = Mathf.Min(node.Beta, value);
            //if (node.Beta == value) node.BestChild = stack.Pop();
        }
        else
        {
            node.Parent.Value = Mathf.Max(node.Value, value);
            //node.Alpha = Mathf.Max(node.Alpha, value);
            //if (node.Alpha == value) node.BestChild = stack.Pop();
        }
        if (node.Value == value) node.Parent.BestChild = node;
    }
}
