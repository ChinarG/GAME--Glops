using UnityEngine;
using System.Collections;

public class ClearLineSweet : ClearedSweet
{
	public bool IsHang;//是不是行


	/// <summary>
	/// 重写清除虚方法
	/// </summary>
	public override void Clear()
	{
		base.Clear();
		if (IsHang)//是行
		{
			sweet.gameManager.ClearHang(sweet.Y);
		}
		else
		{
			sweet.gameManager.ClearLie(sweet.X);
		}
	}
}

