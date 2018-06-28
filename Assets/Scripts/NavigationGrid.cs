﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

public class NavigationGrid : MonoBehaviour {

    [SerializeField] LayerMask m_UnpassableMask;
    [SerializeField] Vector2 m_GridWorldSize;
    [SerializeField] Tilemap m_CollisionMap;

    public List<Node> m_Path;

    Grid m_Grid;
    Node[,] m_NavGrid;
    float m_NodeRadius;
    float m_NodeDiameter;
    int m_GridSizeX;
    int m_GridSizeY;

    private void Awake()
    {
        m_Grid = GetComponent<Grid>();
        m_NodeDiameter = m_Grid.cellSize.x;
        m_NodeRadius = m_NodeDiameter * 0.5f;
        m_GridSizeX = Mathf.RoundToInt(m_GridWorldSize.x / m_NodeDiameter);
        m_GridSizeY = Mathf.RoundToInt(m_GridWorldSize.y / m_NodeDiameter);
        CreateGrid();
    }

    void CreateGrid()
    {
        m_NavGrid = new Node[m_GridSizeX, m_GridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * m_GridWorldSize.x / 2 - Vector3.up * m_GridWorldSize.y / 2;

        for (int x = 0; x < m_GridSizeX; x++)
        {
            for (int y = 0; y < m_GridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * m_NodeDiameter + m_NodeRadius) + Vector3.up * (y * m_NodeDiameter + m_NodeRadius);
                bool passable = !m_CollisionMap.HasTile(new Vector3Int(Mathf.RoundToInt(x - m_GridSizeX * 0.5f), Mathf.RoundToInt(y - m_GridSizeY * 0.5f), 0));
                m_NavGrid[x, y] = new Node(passable, worldPoint, x, y);
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (Mathf.Abs(x) == Mathf.Abs(y))
                {
                    continue;
                }
                int checkX = node.m_GridX + x;
                int checkY = node.m_GridY + y;

                if (checkX >= 0 && checkX < m_GridSizeX && checkY >= 0 && checkY < m_GridSizeY)
                {
                    neighbours.Add(m_NavGrid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + m_GridWorldSize.x / 2) / m_GridWorldSize.x;
        float percentY = (worldPosition.y + m_GridWorldSize.y / 2) / m_GridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((m_GridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((m_GridSizeY - 1) * percentY);
        return m_NavGrid[x, y];
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(m_GridWorldSize.x, m_GridWorldSize.y, 1.0f));
        if (m_NavGrid != null)
        {
            foreach (Node node in m_NavGrid)
            {
                Gizmos.color = node.m_Passable ? Color.white : Color.red;
                if (m_Path != null)
                {
                    if (m_Path.Contains(node))
                    {
                        Gizmos.color = Color.black;
                    }
                }
                Gizmos.DrawCube(node.m_WorldPosition, Vector3.one * (m_NodeDiameter));
            }
        }
    }
}
