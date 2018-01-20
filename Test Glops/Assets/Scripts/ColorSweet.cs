using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 颜色脚本
/// </summary>
public class ColorSweet : MonoBehaviour
{
	public enum ColorType
	{
		YELLOW, //黄
		PURPLE, //紫
		RED,    //红
		BLUE,   //蓝
		GREEN,  //绿
		PINK,   //棒棒糖
		ANY,    //彩虹糖
		COUNT   //预留
	}

	private Dictionary<ColorType, Sprite> colorSpriteDict; //颜色字典
	public  ColorSprite[]                 ColorSprites;    //结构体数组
	[System.Serializable]                                  //序列化
	public struct ColorSprite                              //结构体
	{
		public ColorType color;
		public Sprite    sprite;
	}

	private SpriteRenderer sprite;   //渲染器对象
	public  int            NumColors //颜色长度：多少种颜色
	{
		get { return ColorSprites.Length; }
	}

	private ColorType color; //物品颜色
	public  ColorType Color
	{
		get { return color; }

		set { SetColor(value); }
	}


	private void Awake()
	{
		sprite          = transform.Find("Sweet").GetComponent<SpriteRenderer>(); //获取渲染组件
		colorSpriteDict = new Dictionary<ColorType, Sprite>();                    //实例化字典对象
		for (int i = 0; i < ColorSprites.Length; i++)                             //遍历字典，在字典中添加图片
		{
			if (!colorSpriteDict.ContainsKey(ColorSprites[i].color))
			{
				colorSpriteDict.Add(ColorSprites[i].color, ColorSprites[i].sprite);
			}
		}
	}


	/// <summary>
	/// 设置颜色
	/// </summary>
	/// <param name="newColor"></param>
	public void SetColor(ColorType newColor)
	{
		color = newColor;
		if (colorSpriteDict.ContainsKey(newColor))
		{
			sprite.sprite = colorSpriteDict[newColor];
		}
	}


	void Start()
	{
	}


	void Update()
	{
	}
}