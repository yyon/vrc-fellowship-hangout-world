using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public static class Utils {
	public static double UnixSeconds() {
		DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		return (DateTime.UtcNow - epochStart).TotalSeconds;
	}

	public static void Append<T>(ref T[] array, T item) {
		T[] nArray = new T[array.Length + 1];
		Array.Copy(array, nArray, array.Length);
		nArray[array.Length] = item;
		array = nArray;
	}

	public static void Remove<T>(ref T[] array, int index) {
		T[] nArray = new T[array.Length - 1];
		Array.Copy(array, 0, nArray, 0, index);
		Array.Copy(array, index + 1, nArray, index, array.Length - 1 - index);
		array = nArray;
	}

	public static double Clamp(double value, double min, double max) {
		return Math.Min(Math.Max(value, min), max);
	}

	public static Color Gradient(double value, double[] pos, Color[] color) {
		int i = 0;
		while(i < pos.Length && pos[i] < value) i++;
		if(i == 0) return color[0];
		if(i == pos.Length) return color[i - 1];
		return Color.Lerp(color[i - 1], color[i], (float)((value - pos[i - 1]) / (pos[i] - pos[i - 1])));
	}

	public static float Gradient(double value, double[] pos, double[] outValue) {
		int i = 0;
		while(i < pos.Length && pos[i] < value) i++;
		if(i == 0) return (float)outValue[0];
		if(i == pos.Length) return (float)outValue[i - 1];
		return Mathf.Lerp((float)outValue[i - 1], (float)outValue[i], (float)((value - pos[i - 1]) / (pos[i] - pos[i - 1])));
	}

	public static double PingPong(double value, double max) {
		value = value % max * 2;
		if(value < 0) value += max * 2;
		if(value > max) value = max - (value - max);
		return value;
	}

	public static float PerlinNoise3Octives(float position) {
		return Mathf.PerlinNoise(position * 0.25f, 0) * 0.1f
			+ Mathf.PerlinNoise(position * 0.5f, 0) * 0.3f
			+ Mathf.PerlinNoise(position, 0) * 0.6f;
	}

	public static float PerlinNoise3Octives(Vector2 position) {
		return Mathf.PerlinNoise(position.x * 0.25f, position.y * 0.25f) * 0.1f
			+ Mathf.PerlinNoise(position.x * 0.5f, position.y * 0.5f) * 0.3f
			+ Mathf.PerlinNoise(position.x, position.y) * 0.6f;
	}
}
