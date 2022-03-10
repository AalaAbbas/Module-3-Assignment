using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;

public class EnvironmentManager : MonoBehaviour
{
    #region Fields and properties

    VoxelGrid _voxelGrid;
    int _randomSeed = 666;

    bool _showVoids = true;

    #endregion

    #region Unity Standard Methods

    void Start()
    {
        // Initialise the voxel grid
        Vector3Int gridSize = new Vector3Int(64, 10, 64);
        _voxelGrid = new VoxelGrid(gridSize, Vector3.zero, 1, parent: this.transform);

        // Set the random engine's seed
        Random.InitState(_randomSeed);
    }

    void Update()
    {
        // Draw the voxels according to their Function Colors
        DrawVoxels();

        // Use the V key to switch between showing voids
        if (Input.GetKeyDown(KeyCode.V))
        {
            _showVoids = !_showVoids;
        }

		if (Input.GetMouseButtonDown(0))
		{
            var voxel = SelectVoxel();

			if (voxel != null)
			{
                _voxelGrid.CreateBlackRectangle(voxel.Index, 10, 30, 1);
			}
		}
		if (Input.GetKeyDown(KeyCode.R))
		{
            _voxelGrid.ClearGrid();
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
            //CreateRandomRectangles(3,5,20,5,30);
            PopulateRecAndSave(800, 2, 4, 5, 25, 2, 30);
		}
    }

    #endregion

    #region Private Methods

    void CreateRandomRectangles(int amt, int minWidth, int maxWidth, int minDepth, int maxDepth, bool picky= true)
    {
        for (int i = 0; i < amt; i++)
        {
            bool success = false;
            while (!success)
            {
                float rand = Random.value;

                int x;
                int z;
                if (rand <0.5f)
                {
                    x = Random.value < 0.5f ? 0 : _voxelGrid.GridSize.x - 1;
                    z = Random.Range(0, _voxelGrid.GridSize.z);
                }
                else
                {
                    z = Random.value < 0.5f ? 0 : _voxelGrid.GridSize.z - 1;
                    x = Random.Range(0, _voxelGrid.GridSize.x);
                }
                

                Vector3Int origin = new Vector3Int(x, 0, z);

                int width = Random.Range(minWidth, maxWidth);
                int depth = Random.Range(minDepth, maxDepth);

                success = _voxelGrid.CreateBlackRectangle(origin, width, depth,0,picky);
            }
        }
    }

    private void PopulateRecAndSave(int sampleSize, int minAmt, int maxAmt, int minWidth, int maxWidth, int minDepth, int maxDepth)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        string saveFolder = "TrainingSet";
		for (int i = 0; i < sampleSize; i++)
		{
            int amt = Random.Range(minAmt, maxAmt);

            _voxelGrid.ClearGrid();
            CreateRandomRectangles(amt, minWidth,maxWidth,minDepth,maxDepth, true);

            Texture2D gridImage = _voxelGrid.ImageFromGrid(transparent: true);
            Texture2D resizedImage = ImageReadWrite.Resize256(gridImage, Color.grey);
            ImageReadWrite.SaveImage(resizedImage, $"{saveFolder}/Grid_{i}");


		}
        stopwatch.Stop();
        print($"Took {stopwatch.ElapsedMilliseconds}millseconds to generate {sampleSize}images");
    }

    private Voxel SelectVoxel()
    {
        Voxel selected = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out RaycastHit hit))
		{
            Transform objectHit = hit.transform;

			if (objectHit.CompareTag("Voxel"))
			{
                string voxelName = objectHit.name;
                var index = voxelName.Split('_').Select(n => int.Parse(n)).ToArray();
                selected = _voxelGrid.Voxels[index[0], index[1], index[2]];
			}
		}

        return selected;
    }
    /// <summary>
    /// Create random rectangles on the grid
    /// </summary>
    /// <param name="amt">The amount of rectangles to generate</param>
    /// <param name="minWidth">The minimum width of the rectangles</param>
    /// <param name="maxWidth">The maximum width of the rectangles</param>
    /// <param name="minDepth">The minimum depth of the rectangles</param>
    /// <param name="maxDepth">The maximum depth of the rectangles</param>
   
    /// <summary>
    /// Draws the voxels according to it's state and Function Corlor
    /// </summary>
    void DrawVoxels()
    {
        foreach (var voxel in _voxelGrid.Voxels)
        {
            if (voxel.IsActive)
            {
                Vector3 pos = (Vector3)voxel.Index * _voxelGrid.VoxelSize + transform.position;
                if (voxel.FColor    ==   FunctionColor.Black)   Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.black);
                else if (voxel.FColor == FunctionColor.Red)     Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.red);
                else if (voxel.FColor == FunctionColor.Yellow)  Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.yellow);
                else if (voxel.FColor == FunctionColor.Green)   Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.green);
                else if (voxel.FColor == FunctionColor.Cyan)    Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.cyan);
                else if (voxel.FColor == FunctionColor.Magenta) Drawing.DrawCube(pos, _voxelGrid.VoxelSize, Color.magenta);
                else if (_showVoids && voxel.VoxelCollider != null)
                    Drawing.DrawTransparentCube(pos, _voxelGrid.VoxelSize);
            }
        }
    }

    #endregion
}
