using UnityEngine;
using System.Collections.Generic;
using System;

public class Tetramino
{
	private const int MATRIX_SIZE = 4;
	private static readonly int[] chances = { 10, 15, 15, 15, 15, 10, 20 };
	private static readonly int[] advancedChances = { 10, 15, 15, 15, 15, 10, 5, 5, 5, 5 };
	private static bool[][,] tetraminos;

	public int Id { get; private set; }
	public Color Color { get; private set; }
	private int x;
	private int y;
	private bool[,] matrix;

	public static void ParseFile()
	{
		TextAsset textAsset = Resources.Load<TextAsset>("tetraminos");
		char[] separators = { ' ', '\n', '\r' };
		string[] values = textAsset.text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
		List<bool[,]> matrices = new List<bool[,]>();
		for (int i = 0; i < values.Length; i++)
		{
			int id = i / 16;
			int y = i % 16 / 4;
			int x = i % 4;
			bool value = values[i] == "1";
			if (id == matrices.Count)
				matrices.Add(new bool[MATRIX_SIZE, MATRIX_SIZE]);
			matrices[id][y, x] = value;
		}
		tetraminos = matrices.ToArray();
	}

	public Tetramino()
	{
		if (tetraminos == null)
			ParseFile();
		Color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
		x = Game.Width / 2 - MATRIX_SIZE / 2;
		int[] curChances = !Game.IsAdvancedMode ? chances : advancedChances;
		int total = 0;
		foreach (int val in curChances)
			total += val;
		int randomValue = UnityEngine.Random.Range(0, total);
		Id = 0;
		for (int i = 0; i < curChances.GetLength(0); i++)
		{
			if (randomValue < curChances[i])
			{
				Id = i;
				break;
			}
			randomValue -= curChances[i];
		}
		matrix = new bool[MATRIX_SIZE, MATRIX_SIZE];
		for (int i = 0; i < MATRIX_SIZE; i++)
		{
			for (int j = 0; j < MATRIX_SIZE; j++)
				matrix[i, j] = tetraminos[Id][i, j];
		}
	}

	public void SetActive(GameObject[,] field, bool value)
	{
		for (int i = 0; i < MATRIX_SIZE; i++)
		{
			for (int j = 0; j < MATRIX_SIZE; j++)
			{
				if (matrix[i, j])
				{
					field[y + i, (x + j) % Game.Width].SetActive(value);
					SpriteRenderer renderer = field[y + i, (x + j) % Game.Width].GetComponent<SpriteRenderer>();
					renderer.color = Color;
				}
			}
		}
	}
	public bool Check(GameObject[,] field)
	{
		for (int i = 0; i < MATRIX_SIZE; i++)
		{
			for (int j = 0; j < MATRIX_SIZE; j++)
			{
				if (matrix[i, j])
				{
					if (y + i >= Game.Height || !Game.IsAdvancedMode && (0 > x + j || x + j >= Game.Width))
						return false;
					if (field[y + i, (x + j) % Game.Width].activeSelf)
						return false;
				}
			}
		}
		return true;
	}

	public void MoveLeft()
	{
		if (Game.IsAdvancedMode && x == 0)
			x = Game.Width;
		x--;
	}
	public void MoveRight()
	{
		x++;
	}
	public void MoveUp()
	{
		y--;
	}
	public void MoveDown()
	{
		y++;
	}
	public void Rotate()
	{
		bool[,] newTetramino = new bool[MATRIX_SIZE, MATRIX_SIZE];
		for (int i = 0; i < MATRIX_SIZE; i++)
		{
			for (int j = 0; j < MATRIX_SIZE; j++)
				newTetramino[j, MATRIX_SIZE - 1 - i] = matrix[i, j];
		}
		matrix = newTetramino;
	}
}
