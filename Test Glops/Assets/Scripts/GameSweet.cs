using System;
using UnityEngine;


/// <summary>
/// 物品基础脚本
/// </summary>
public class GameSweet : MonoBehaviour
{
	private int x; //物品列数量
	public  int X
	{
		get { return x; }

		set
		{
			if (CanMove())
			{
				x = value;
			}
		}
	}
	private int y; //物品行数量
	public  int Y
	{
		get { return y; }

		set
		{
			if (CanMove())
			{
				y = value;
			}
		}
	}
	private GameManager.SweetsType type; //物品种类
	public  GameManager.SweetsType Type
	{
		get { return type; }
	}
	private MovedSweet movedComponet; //移动组件
	public  MovedSweet MovedComponet
	{
		get { return movedComponet; }
	}
	private ColorSweet colorComponet; //颜色组件
	public  ColorSweet ColorComponet
	{
		get { return colorComponet; }
	}

	[HideInInspector]               //在Inspector面板隐藏
	public GameManager gameManager; //控制脚本对象


	private void Awake()
	{
		movedComponet = GetComponent<MovedSweet>(); //初始化之前就获取移动组件
		colorComponet = GetComponent<ColorSweet>(); //初始化之前就获取移动组件
	}


	/// <summary>
	/// 初始化函数
	/// </summary>
	/// <param name="_x"></param>
	/// <param name="_y"></param>
	/// <param name="_gameManager"></param>
	/// <param name="_type"></param>
	public void Init(int _x, int _y, GameManager _gameManager, GameManager.SweetsType _type)
	{
		x           = -x; //当前的 x 就等于初始化函数，传进来的传入参数的值
		y           = _y;
		gameManager = _gameManager;
		type        = _type;
	}


	/// <summary>
	/// 判断物品能否移动
	/// </summary>
	/// <returns></returns>
	public bool CanMove()
	{
		return movedComponet != null;
	}


	/// <summary>
	/// 判断物品能否作色
	/// </summary>
	/// <returns></returns>
	public bool CanColor()
	{
		return colorComponet != null;
	}


	void Start()
	{
	}
}