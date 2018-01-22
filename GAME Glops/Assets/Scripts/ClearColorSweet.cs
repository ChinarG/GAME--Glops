using System;
using UnityEngine;
using System.Collections;


public class ClearColorSweet : ClearedSweet
{
	private ColorSweet.ColorType clearColor; //颜色类型对象

	public ColorSweet.ColorType ClearColor
	{
		get { return clearColor; }

		set { clearColor = value; }
	}


	/// <summary>
	/// 重写清除方法
	/// </summary>
	public override void Clear()
	{
		base.Clear();
		sweet.gameManager.ClearColor(clearColor);
	}
}