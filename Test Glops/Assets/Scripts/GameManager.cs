using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 游戏控制脚本
/// </summary>
public class GameManager : MonoBehaviour
{
	private static GameManager _instance; //单例
	public static  GameManager Instance
	{
		get { return _instance; }
		set { _instance = value; }
	}


	private void Awake()
	{
		_instance = this;
	}


	//大网格的行列数
	public int xLie;
	public int yHang;
	public float fillTime;//填充时间
	public GameObject gridPrefab; //背景块


	//消消乐物品的种类
	public enum SweetsType
	{
		EMPTY,        //空物体
		NORMAL,       //普通
		BARRIER,      //障碍物
		HANG_CLEAR,   //行消除
		LIE_CLEAR,    //列消除
		RAINBOWCANDY, //彩虹糖
		COUNT         //标记类型
	}


	//物品的预制体的字典 —— 可以通过物品的种类，来得到相对应的物体
	private Dictionary<SweetsType, GameObject> sweetPrefabDict;


	//由于字典不会直接在 Inspector面板上显示，所以需要用结构体（因为结构体，经过序列化，可以显示）
	[System.Serializable] //加上可序列化特性
	public struct SweetPrefab
	{
		public SweetsType type;
		public GameObject prefabs;
	}


	public  SweetPrefab[]  sweetPrefabs; //结构体数组
	private GameSweet[ , ] sweets;       //物品的数组,二维数组，中间必须加逗号


	/// <summary>
	/// 初始化函数
	/// </summary>
	void Start()
	{
		//实例化字典
		sweetPrefabDict = new Dictionary<SweetsType, GameObject>();
		for (int i = 0; i < sweetPrefabs.Length; i++) //遍历结构体数组
		{
			if (!sweetPrefabDict.ContainsKey(sweetPrefabs[i].type)) //如果字典里，不包含结构体里 对应的类型
			{
				sweetPrefabDict.Add(sweetPrefabs[i].type, sweetPrefabs[i].prefabs); //添加 结构体 到字典里
			}
		}


		sweets = new GameSweet[ xLie, yHang ]; //实例化二维数据，第一个维度是列，第二个是行
		for (int x = 0; x < xLie; x++)
		{
			for (int y = 0; y < yHang; y++)
			{
				CreateNewSweet(x, y, SweetsType.EMPTY); //调用创建按钮的方法
			}
		}

		Destroy(sweets[4,4].gameObject);//删除一个甜品
		CreateNewSweet(4, 4, SweetsType.BARRIER);

		StartCoroutine(AllFill());//开启协成

		for (int x = 0; x < xLie; x++) //实例化背景
		{
			for (int y = 0; y < yHang; y++)
			{
				//实例化方块背景
				GameObject chocolate       = (GameObject)Instantiate(gridPrefab, CorrectPosition(x, y), Quaternion.identity);
				chocolate.transform.parent = transform; //设置父物体
			}
		}
	}


	/// <summary>
	/// 背景块的实际位置
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns></returns>
	public Vector3 CorrectPosition(int x, int y)
	{
		//实例化巧克力的实际位置
		return new Vector3(transform.position.x - xLie / 2f + x, transform.position.y + yHang / 2f - y);
	}


	/// <summary>
	/// 生成物品方法
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="type"></param>
	/// <returns></returns>
	public GameSweet CreateNewSweet(int x, int y, SweetsType type)
	{
		GameObject newSweet =
			(GameObject) Instantiate(sweetPrefabDict[type], CorrectPosition(x, y), Quaternion.identity); //实例化+强转
		newSweet.transform.parent = transform;
		sweets[x, y]              = newSweet.GetComponent<GameSweet>();
		sweets[x, y].Init(x, y, this, type);
		return sweets[x, y];
	}


	/// <summary>
	/// 协成-全部填充
	/// </summary>
	public IEnumerator AllFill()
	{
		while (Fill())
		{
			yield return new WaitForSeconds(fillTime);
		}
	}


	/// <summary>
	/// 分步填充
	/// </summary>
	public bool Fill()
	{
		bool filledNotFinished = false;      //判断本次否填，是否完成
		for (int y = yHang - 2; y >= 0; y--) //从下往上
		{
			for (int x = 0; x < xLie; x++) //从左到右
			{
				GameSweet sweet = sweets[x, y]; //得到当前元素位置的物品对象
				if (sweet.CanMove())
				{
					GameSweet sweetBelow = sweets[x, y + 1]; //下边元素位置
					if (sweetBelow.Type == SweetsType.EMPTY)
					{
						sweet.MovedComponet.Move(x, y + 1,fillTime);         //上边的元素，往下移动
						sweets[x, y                   + 1] = sweet; //二维数组，对应位置更新。
						CreateNewSweet(x, y, SweetsType.EMPTY);
						filledNotFinished = true;
					}
				}
			}
		}

		//最上排的特殊情况
		for (int x = 0; x < xLie; x++)
		{
			GameSweet sweet = sweets[x, 0];
			if (sweet.Type == SweetsType.EMPTY)
			{
				GameObject newSweet =
					(GameObject) Instantiate(sweetPrefabDict[SweetsType.NORMAL], CorrectPosition(x, -1), Quaternion.identity);
				newSweet.transform.parent = transform;
				sweets[x, 0]              = newSweet.GetComponent<GameSweet>();
				sweets[x, 0].Init(x, -1, this, SweetsType.NORMAL);
				sweets[x, 0].MovedComponet.Move(x, 0,fillTime);
				sweets[x, 0].ColorComponet.SetColor((ColorSweet.ColorType) Random.Range(0, sweets[x, 0].ColorComponet.NumColors));
				filledNotFinished = true;
			}
		}

		return filledNotFinished;
	}
}