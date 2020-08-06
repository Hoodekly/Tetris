using UnityEngine;

public static class Utilities
{
	public static Color SetAlpha(Color color, float alpha)
	{
		color.a = alpha;
		return color;
	}
}
