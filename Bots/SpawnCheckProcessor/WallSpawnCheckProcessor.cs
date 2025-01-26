﻿using UnityEngine;

namespace Donuts.Bots.SpawnCheckProcessor;

public class WallSpawnCheckProcessor : SpawnCheckProcessorBase
{
	private const string WALL_OBJECT_NAME_UPPER = "WALLS";
	private readonly Collider[] _spawnCheckColliderBuffer = new Collider[50];
	private readonly Vector3 _boxCheckScale = Vector3.one;
	
	public override bool Process(Vector3 spawnPoint)
	{
		int size = Physics.OverlapBoxNonAlloc(spawnPoint, _boxCheckScale, _spawnCheckColliderBuffer,
			Quaternion.identity, LayerMaskClass.LowPolyColliderLayer);
		
		if (size <= 0)
		{
			return base.Process(spawnPoint);
		}
		
		for (var i = 0; i < size; i++)
		{
			Transform currentTransform = _spawnCheckColliderBuffer[i].transform;
			if (RecursiveFindWallsGameObject(currentTransform))
			{
				return false;
			}
		}
		
		return base.Process(spawnPoint);
	}
	
	/// <summary>
	/// Recursively check for "WALLS" string in the game object's name, going upwards to root of hierarchy.
	/// </summary>
	private static bool RecursiveFindWallsGameObject(Transform transform)
	{
		while (transform != null)
		{
			if (transform.gameObject.name.ToUpper().Contains(WALL_OBJECT_NAME_UPPER))
			{
				return true;
			}
			
			transform = transform.parent;
		}
		
		return false;
	}
}