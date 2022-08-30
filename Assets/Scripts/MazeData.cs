using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MazeData {
    public int SIZE;
    private readonly MazeDataCell[,] grid;

    public MazeData(int SIZE) {
        this.SIZE = SIZE;
        grid = new MazeDataCell[SIZE, SIZE];
        for (var row = 0; row < SIZE; row++)
            for (var col = 0; col < SIZE; col++)
                grid[row, col] = new MazeDataCell(row, col);

        Restart();
    }

    private void Restart() {
        for (var row = 0; row < SIZE; row++)
            for (var col = 0; col < SIZE; col++)
                grid[row, col].Reset();
    }

    public enum Neighbor { Top, Bottom, Left, Right };
    public List<Neighbor> GetNeighbors(int row, int col, Func<Vector2Int, bool> valid) {
        var neighbors = new List<Neighbor>();
        if (row > 0 && valid(new Vector2Int(row - 1, col)))
            neighbors.Add(Neighbor.Top);
        if (row + 1 < SIZE && valid(new Vector2Int(row + 1, col)))
            neighbors.Add(Neighbor.Bottom);
        if (col > 0 && valid(new Vector2Int(row, col - 1)))
            neighbors.Add(Neighbor.Left);
        if (col + 1 < SIZE && valid(new Vector2Int(row, col + 1)))
            neighbors.Add(Neighbor.Right);
        return neighbors;
    }

    public Vector2Int RemoveNeighborWall(Neighbor neighbor, int row, int col) {
        switch (neighbor) {
            case Neighbor.Top:
                RemoveWallT(row, col);
                return new Vector2Int(row - 1, col);
            case Neighbor.Bottom:
                RemoveWallB(row, col);
                return new Vector2Int(row + 1, col);
            case Neighbor.Left:
                RemoveWallL(row, col);
                return new Vector2Int(row, col - 1);
            case Neighbor.Right:
                RemoveWallR(row, col);
                return new Vector2Int(row, col + 1);
            default:
                throw new Exception("Invalid Neighbor to Remove");
        }
    }

    public Vector2Int GetNeighbor(Neighbor neighbor, int row, int col) {
        switch (neighbor) {
            case Neighbor.Top:
                return new Vector2Int(row - 1, col);
            case Neighbor.Bottom:
                return new Vector2Int(row + 1, col);
            case Neighbor.Left:
                return new Vector2Int(row, col - 1);
            case Neighbor.Right:
                return new Vector2Int(row, col + 1);
            default:
                throw new Exception("Invalid Neighbor");
        }
    }


    public MazeDataCell this[int row, int col] => grid[row, col];
    
    public void RemoveWallT(int row, int col) {
        grid[row, col].RemoveTopWall();
        grid[row - 1, col].RemoveBottomWall();
    }

    public void RemoveWallB(int row, int col) {
        grid[row, col].RemoveBottomWall();
        grid[row + 1, col].RemoveTopWall();
    }

    public void RemoveWallL(int row, int col) {
        grid[row, col].RemoveLeftWall();
        grid[row, col - 1].RemoveRightWall();
    }

    public void RemoveWallR(int row, int col) {
        grid[row, col].RemoveRightWall();
        grid[row, col + 1].RemoveLeftWall();
    }
}