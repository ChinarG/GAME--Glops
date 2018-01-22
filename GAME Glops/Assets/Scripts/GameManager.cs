using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


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
	public int        xLie;
	public int        yHang;
	public float      fillTime;   //填充时间
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


	public                   SweetPrefab[]  sweetPrefabs;       //结构体数组
	private                  GameSweet[ , ] sweets;             //物品的数组,二维数组，中间必须加逗号
	private                  GameSweet      pressedSweet;       //按下的物品
	private                  GameSweet      enterSweet;         //松开的物品
	private                  Text           TimeText;           //倒计时文本框
	private                  float          TimeCountDown = 60; //倒计时，时间
	private                  bool           IsGameOver;         //是否结束游戏
	[HideInInspector] public int            PlayerScore;        //分数
	private                  Text           PlayerScoreText;    //玩家分数文本框
	private                  float          AddScoreTime;       //累加时间
	private                  float          CurrentScore;       //当前分数
	public                   GameObject     GameOverPanel;      //结束游戏界面
	private                  Text           FinalScoreText;     //最终得分


	/// <summary>
	/// 初始化函数
	/// </summary>
	void Start()
	{
		TimeText        = GameObject.Find("Time_Text").GetComponent<Text>(); //获取文本框
		PlayerScoreText = GameObject.Find("Score_Internal_Text").GetComponent<Text>();
		Button button   = GameObject.Find("ReTurn_Button").GetComponent<Button>(); //添加重玩按钮方法
		button.onClick.AddListener(RePlay);

		//实例化字典
		sweetPrefabDict = new Dictionary<SweetsType, GameObject>();
		for (int i = 0; i < sweetPrefabs.Length; i++) //遍历结构体数组
		{
			if (!sweetPrefabDict.ContainsKey(sweetPrefabs[i].type)) //如果字典里，不包含结构体里 对应的类型
			{
				print(sweetPrefabs[i].type);
				print(sweetPrefabs[i].prefabs);
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

		Destroy(sweets[4, 4].gameObject);
		CreateNewSweet(4, 4, SweetsType.BARRIER);
		Destroy(sweets[4, 3].gameObject);
		CreateNewSweet(4, 3, SweetsType.BARRIER);
		Destroy(sweets[1, 1].gameObject);
		CreateNewSweet(1, 1, SweetsType.BARRIER);
		Destroy(sweets[7, 1].gameObject);
		CreateNewSweet(7, 1, SweetsType.BARRIER);
		Destroy(sweets[1, 6].gameObject);
		CreateNewSweet(1, 6, SweetsType.BARRIER);
		Destroy(sweets[7, 6].gameObject);
		CreateNewSweet(7, 6, SweetsType.BARRIER);

		for (int x = 0; x < xLie; x++) //实例化背景
		{
			for (int y = 0; y < yHang; y++)
			{
				//实例化方块背景
				GameObject chocolate       = (GameObject) Instantiate(gridPrefab, CorrectPosition(x, y), Quaternion.identity);
				chocolate.transform.parent = transform; //设置父物体
			}
		}





		StartCoroutine(AllFill()); //开启协成
	}


	/// <summary>
	/// 更新函数
	/// </summary>
	void Update()
	{
		if (IsGameOver) return;          //如果游戏结束，直接跳出
		TimeCountDown -= Time.deltaTime; //倒计时
		if (TimeCountDown <= 0)
		{
			TimeCountDown = 0;

			IsGameOver = true;
			GameOverPanel.SetActive(true); //激活结束游戏界面

			FinalScoreText      = GameObject.Find("LastScore_Text").GetComponent<Text>();
			FinalScoreText.text = PlayerScore.ToString(); //最终得分：赋值

			Button button = GameObject.Find("RePlay_Button").GetComponent<Button>(); //添加游戏结束界面：按钮方法
			button.onClick.AddListener(RePlay);
			button = GameObject.Find("ReturnMain_Button").GetComponent<Button>();
			button.onClick.AddListener(ReturnToMain);
			return;
		}
		TimeText.text = TimeCountDown.ToString("0"); //由于，是浮点型变量，所以强转取整数
		if (AddScoreTime <= 0.03f)
		{
			AddScoreTime += Time.deltaTime;
		}
		else
		{
			if (CurrentScore < PlayerScore)
			{
				CurrentScore++;
				PlayerScoreText.text = CurrentScore.ToString();
				AddScoreTime         = 0;
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
		bool needRefill = true; //需要重填

		while (needRefill)
		{
			yield return new WaitForSeconds(fillTime); //等待

			while (Fill()) //本次填充
			{
				yield return new WaitForSeconds(fillTime);
			}

			needRefill = ClearAllMatchedSweet();
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
				if (sweet.CanMove())            //如果能移动就填充
				{
					GameSweet sweetBelow = sweets[x, y + 1]; //下边元素位置
					if (sweetBelow.Type == SweetsType.EMPTY) //如果下方是空格子，就垂直向下填充
					{
						Destroy(sweetBelow.gameObject);
						sweet.MovedComponet.Move(x, y + 1, fillTime); //上边的元素，往下移动
						sweets[x, y                   + 1] = sweet;   //二维数组，对应位置更新。
						CreateNewSweet(x, y, SweetsType.EMPTY);
						filledNotFinished = true;
					}
					else //斜着填充
					{
						for (int down = -1; down < 1; down++)
						{
							if (down != 0) //不是正下方
							{
								int downX = x + down;
								if (downX >= 0 && downX < xLie) //规定范围，排除边缘情况
								{
									GameSweet downSweet = sweets[downX, y + 1]; //左下方甜品
									if (downSweet.Type == SweetsType.EMPTY)     //左下方为空
									{
										bool canfill = true; //用来判断是否可以垂直填充
										for (int upY = y; upY >= 0; upY--)
										{
											GameSweet upSweet = sweets[downX, upY]; //正上方元素
											if (upSweet.CanMove())
											{
												break; //能移动直接跳出
											}
											else if (!upSweet.CanMove() && upSweet.Type != SweetsType.EMPTY)
											{
												canfill = false;
												break;
											}
										}

										if (!canfill) //不能垂直填充
										{
											Destroy(downSweet.gameObject); //删除下边游戏的物体
											sweet.MovedComponet.Move(downX, y + 1, fillTime);
											sweets[downX, y                   + 1] = sweet;
											CreateNewSweet(x, y, SweetsType.EMPTY);
											filledNotFinished = true;
											break;
										}
									}
								}
							}
						}
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
				sweets[x, 0].MovedComponet.Move(x, 0, fillTime);
				sweets[x, 0].ColorComponet.SetColor((ColorSweet.ColorType) Random.Range(0, sweets[x, 0].ColorComponet.NumColors));
				filledNotFinished = true;
			}
		}

		return filledNotFinished;
	}


	/// <summary>
	/// 物品是否相邻
	/// </summary>
	/// <param name="sweet1"></param>
	/// <param name="sweet2"></param>
	/// <returns></returns>
	private bool IsAdjacent(GameSweet sweet1, GameSweet sweet2)
	{
		return (sweet1.X == sweet2.X && Mathf.Abs(sweet1.Y - sweet2.Y) <= 1.06) ||
		       (sweet1.Y == sweet2.Y && Mathf.Abs(sweet1.X - sweet2.X) <= 1.06);
	}


	/// <summary>
	/// 交换物品位置
	/// </summary>
	/// <param name="sweet1"></param>
	/// <param name="sweet2"></param>
	private void ExChangeSweets(GameSweet sweet1, GameSweet sweet2)
	{
		if (sweet1.CanMove() && sweet2.CanMove()) //如果2个物品都能移动
		{
			sweets[sweet1.X, sweet1.Y] = sweet2;
			sweets[sweet2.X, sweet2.Y] = sweet1;

			if (MatchSweets(sweet1, sweet2.X, sweet2.Y) != null                    ||
			    MatchSweets(sweet2, sweet1.X, sweet1.Y) != null                    ||
			    sweet1.Type                             == SweetsType.RAINBOWCANDY ||
			    sweet2.Type                             == SweetsType.RAINBOWCANDY) //如果完成匹配
			{
				int tempX = sweet1.X;
				int tempY = sweet1.Y;

				sweet1.MovedComponet.Move(sweet2.X, sweet2.Y, fillTime);
				sweet2.MovedComponet.Move(tempX, tempY, fillTime);

				if (sweet1.Type == SweetsType.RAINBOWCANDY && sweet1.CanClear() && sweet2.CanClear()) //如果物品1是 特殊物品：消除颜色
				{
					ClearColorSweet clearColor = sweet1.GetComponent<ClearColorSweet>();
					if (clearColor != null) //容错
					{
						clearColor.ClearColor = sweet2.ColorComponet.Color;
					}
					ClearSweet(sweet1.X, sweet1.Y);
				}
				if (sweet2.Type == SweetsType.RAINBOWCANDY && sweet2.CanClear() && sweet2.CanClear()) //如果物品2是 特殊物品：消除颜色
				{
					ClearColorSweet clearColor = sweet2.GetComponent<ClearColorSweet>();
					if (clearColor != null)
					{
						clearColor.ClearColor = sweet1.ColorComponet.Color;
					}
					ClearSweet(sweet2.X, sweet2.Y);
				}


				ClearAllMatchedSweet();    //交换位置后，清除物品，并生成空格
				StartCoroutine(AllFill()); //交换位置后填充

				pressedSweet = null;
				enterSweet = null;
			}
			else
			{
				sweets[sweet1.X, sweet1.Y] = sweet1;
				sweets[sweet2.X, sweet1.Y] = sweet2;
			}
		}
	}


	/// <summary>
	/// 按下物品
	/// </summary>
	public void PressedSweet(GameSweet sweet)
	{
		if (IsGameOver) return; //如果游戏结束，直接跳出
		pressedSweet = sweet;
	}


	/// <summary>
	/// 进入物品
	/// </summary>
	public void EnterSweet(GameSweet sweet)
	{
		if (IsGameOver) return; //如果游戏结束，直接跳出
		enterSweet = sweet;
	}


	/// <summary>
	/// 释放物品
	/// </summary>
	public void ReleaseSweet()
	{
		if (IsGameOver) return;                   //如果游戏结束，直接跳出
		if (IsAdjacent(pressedSweet, enterSweet)) //如果相邻
		{
			ExChangeSweets(pressedSweet, enterSweet); //调用改变位置的方法
		}
	}


	/// <summary>
	/// 匹配消除方法
	/// </summary>
	/// <param name="sweet"></param>
	/// <param name="newX"></param>
	/// <param name="newY"></param>
	/// <returns></returns>
	public List<GameSweet> MatchSweets(GameSweet sweet, int newX, int newY)
	{
		if (sweet.CanColor()) //如果可以着色
		{
			ColorSweet.ColorType color               = sweet.ColorComponet.Color; //上色
			List<GameSweet>      matchHangSweets     = new List<GameSweet>();     //物品行 列表
			List<GameSweet>      matchLieSweets      = new List<GameSweet>();     //物品列 列表
			List<GameSweet>      finishedMatchSweets = new List<GameSweet>();     //完成待删物品 列表

			//检查行消除匹配
			matchHangSweets.Add(sweet);
			for (int i = 0; i <= 1; i++) //i等于0，往左，1往右
			{
				for (int xDistance = 1; xDistance < xLie; xDistance++)
				{
					int x; //偏移后的 x 坐标
					if (i == 0)
					{
						x = newX - xDistance;
					}
					else
					{
						x = newX + xDistance;
					}
					if (x < 0 || x >= xLie)
					{
						break; //限定边界
					}


					if (sweets[x, newY].CanColor() && sweets[x, newY].ColorComponet.Color == color) //如果物品颜色一样
					{
						matchHangSweets.Add(sweets[x, newY]);
					}
					else
					{
						break;
					}
				}
			}

			if (matchHangSweets.Count >= 3) //行列表元素的添加
			{
				for (int i = 0; i < matchHangSweets.Count; i++)
				{
					finishedMatchSweets.Add(matchHangSweets[i]);
				}
			}


			//L T形状匹配
			//遍历后，检测当前行遍历元素数量是否大于3
			if (matchHangSweets.Count >= 3)
			{
				for (int i = 0; i < matchHangSweets.Count; i++)
				{
					//行检查后，检测L和T形状。 检查元素上下元素，是否可以消除:0是上，1是下
					for (int j = 0; j <= 1; j++)
					{
						for (int yDistance = 1; yDistance < yHang; yDistance++)
						{
							int y;      //被检测物体的，Y轴偏移坐标
							if (j == 0) //如果是上方
							{
								y = newY - yDistance; //每次向上递增（物体Y轴坐标，自上而下是0--10）
							}
							else
							{
								y = newY + yDistance; //每次向下递增（）
							}
							if (y < 0 || y >= yHang)
							{
								break; //限定边界
							}

							if (sweets[matchHangSweets[i].X, y].CanColor() &&
							    sweets[matchHangSweets[i].X, y].ColorComponet.Color == color) //如果列方向，颜色一致
							{
								matchLieSweets.Add(sweets[matchHangSweets[i].X, y]); //添加甜品对象到 列表中 
							}
							else
							{
								break;
							}
						}
					}

					if (matchLieSweets.Count < 2) //如果在 行列表中的，垂直方向列 数组中，相同元素小于2
					{
						matchLieSweets.Clear(); //清除
					}
					else //满足条件就加到完成列表
					{
						for (int j = 0; j < matchLieSweets.Count; j++)
						{
							finishedMatchSweets.Add(matchLieSweets[j]);
						}
						break;
					}
				}
			}

			if (finishedMatchSweets.Count >= 3)
			{
				return finishedMatchSweets; //返回  行LT 列表
			}

			matchHangSweets.Clear(); //开始列检查之前：清除列表
			matchLieSweets.Clear();


			//列消除匹配
			matchLieSweets.Add(sweet);
			for (int i = 0; i <= 1; i++) //i等于0，往左，1往右
			{
				for (int yDistance = 1; yDistance < yHang; yDistance++)
				{
					int y; //偏移后的 y 坐标
					if (i == 0)
					{
						y = newY - yDistance;
					}
					else
					{
						y = newY + yDistance;
					}
					if (y < 0 || y >= yHang)
					{
						break; //限定边界
					}


					if (sweets[newX, y].CanColor() && sweets[newX, y].ColorComponet.Color == color) //如果物品颜色一样
					{
						matchLieSweets.Add(sweets[newX, y]);
					}
					else
					{
						break;
					}
				}
			}

			if (matchLieSweets.Count >= 3) //LIE 列表元素的添加
			{
				for (int i = 0; i < matchLieSweets.Count; i++)
				{
					finishedMatchSweets.Add(matchLieSweets[i]);
				}
			}


			//垂直列表中，横向L T形状匹配
			//遍历后，检测当前行遍历元素数量是否大于3
			if (matchLieSweets.Count >= 3)
			{
				for (int i = 0; i < matchLieSweets.Count; i++)
				{
					//行检查后，检测L和T形状。 检查元素上下元素，是否可以消除:0是上，1是下
					for (int j = 0; j <= 1; j++)
					{
						for (int xDistance = 1; xDistance < xLie; xDistance++)
						{
							int x;      //被检测物体的，Y轴偏移坐标
							if (j == 0) //如果是上方
							{
								x = newX - xDistance; //每次向上递增（物体Y轴坐标，自上而下是0--10）
							}
							else
							{
								x = newX + xDistance; //每次向下递增（）
							}
							if (x < 0 || x >= xLie)
							{
								break; //限定边界
							}

							if (sweets[x, matchLieSweets[i].Y].CanColor() &&
							    sweets[x, matchLieSweets[i].Y].ColorComponet.Color == color) //如果列方向，颜色一致
							{
								matchHangSweets.Add(sweets[x, matchLieSweets[i].Y]); //添加甜品对象到 列表中 
							}
							else
							{
								break;
							}
						}
					}

					if (matchHangSweets.Count < 2) //如果在 列列表中的，左右方向行 数组中，相同元素小于2
					{
						matchHangSweets.Clear(); //清除
					}
					else //满足条件就加到完成列表
					{
						for (int j = 0; j < matchHangSweets.Count; j++)
						{
							finishedMatchSweets.Add(matchHangSweets[j]);
						}
						break;
					}
				}
			}

			//这里
			if (finishedMatchSweets.Count >= 3)
			{
				return finishedMatchSweets;
			}
		}

		return null;
	}


	/// <summary>
	/// 清除物品方法
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns></returns>
	public bool ClearSweet(int x, int y)
	{
		if (sweets[x, y].CanClear() && !sweets[x, y].ClearedComponet.IsClearing)
		{
			sweets[x, y].ClearedComponet.Clear(); //调用自身方法，开始清除
			CreateNewSweet(x, y, SweetsType.EMPTY);
			ClearBarrier(x, y); //调用清除障碍物函数
			return true;
		}
		return false;
	}


	/// <summary>
	/// 清除 障碍物
	/// </summary>
	/// <param name="X"></param>
	/// <param name="Y"></param>
	private void ClearBarrier(int X, int Y) //传入坐标是：消除掉物品的坐标
	{
		for (int friendX = X - 1; friendX <= X + 1; friendX++) //左右遍历
		{
			if (friendX != X && friendX >= 0 && friendX < xLie)
			{
				if (sweets[friendX, Y].Type == SweetsType.BARRIER && sweets[friendX, Y].CanClear()) //判断
				{
					sweets[friendX, Y].ClearedComponet.Clear();
					CreateNewSweet(friendX, Y, SweetsType.EMPTY);
				}
			}
		}

		for (int friendY = Y - 1; friendY <= Y + 1; friendY++) //上下遍历
		{
			if (friendY != Y && friendY >= 0 && friendY < yHang)
			{
				if (sweets[X, friendY].Type == SweetsType.BARRIER && sweets[X, friendY].CanClear()) //判断
				{
					sweets[X, friendY].ClearedComponet.Clear();
					CreateNewSweet(X, friendY, SweetsType.EMPTY);
				}
			}
		}
	}


	/// <summary>
	/// 清除规则里物品的方法
	/// </summary>
	/// <returns></returns>
	private bool ClearAllMatchedSweet()
	{
		bool needRefill = false; //是否需要填充
		for (int y = 0; y < yHang; y++)
		{
			for (int x = 0; x < xLie; x++)
			{
				if (sweets[x, y].CanClear()) //如果可以清除
				{
					List<GameSweet> matchList = MatchSweets(sweets[x, y], x, y);

					if (matchList != null) //需要消除
					{
						SweetsType specialSweetsType = SweetsType.COUNT; //定义一个枚举类型：COUNT——是否产生特殊甜品:默认是Count类型

						GameSweet randomSweet   = matchList[Random.Range(0, matchList.Count)]; //随机产生位置
						int       specialSweetX = randomSweet.X;
						int       specialSweetY = randomSweet.Y;


						if (matchList.Count == 4) //消除的4个物品
						{
							specialSweetsType = (SweetsType) Random.Range( (int) SweetsType.HANG_CLEAR,(int) SweetsType.LIE_CLEAR+1); //特殊类型赋值:取左不取右，所以+1
						}
						else if (matchList.Count >= 5)
						{
							specialSweetsType = SweetsType.RAINBOWCANDY;
						}
						//5个

						for (int i = 0; i < matchList.Count; i++) //遍历数组
						{
							if (ClearSweet(matchList[i].X, matchList[i].Y))
							{
								needRefill = true; //填充
							}
						}

						if (specialSweetsType != SweetsType.COUNT) //有特殊类型
						{
							Destroy(sweets[specialSweetX, specialSweetY]);                                        //删除空白物品
							GameSweet newSweet = CreateNewSweet(specialSweetX, specialSweetY, specialSweetsType); //生成特殊甜品
							if (specialSweetsType == SweetsType.HANG_CLEAR || specialSweetsType == SweetsType.LIE_CLEAR &&
							    newSweet.CanColor()                                                                     &&
							    matchList[0].CanColor()) //种类的确定
							{
								newSweet.ColorComponet.SetColor(matchList[0].ColorComponet.Color); //给特殊物品，着色：第一个物品的颜色
							}
							else if (specialSweetsType == SweetsType.RAINBOWCANDY && newSweet.CanColor()) //如果是彩虹堂
							{
								newSweet.ColorComponet.SetColor(ColorSweet.ColorType.ANY);
							}
						}
					}
				}
			}
		}
		return needRefill;
	}


	/// <summary>
	/// 消除整行
	/// </summary>
	/// <param name="hang"></param>
	/// <returns></returns>
	public void ClearHang(int hang)
	{
		for (int x = 0; x < xLie; x++)
		{
			ClearSweet(x, hang);
		}
	}


	/// <summary>
	/// 消除整列
	/// </summary>
	/// <param name="lie"></param>
	public void ClearLie(int lie)
	{
		for (int y = 0; y < yHang; y++)
		{
			ClearSweet(lie, y);
		}
	}


	/// <summary>
	/// 清除颜色
	/// </summary>
	/// <param name="color"></param>
	public void ClearColor(ColorSweet.ColorType color)
	{
		for (int x = 0; x < xLie; x++)
		{
			for (int y = 0; y < yHang; y++)
			{
				if (sweets[x, y].CanColor() && (sweets[x, y].ColorComponet.Color == color || color == ColorSweet.ColorType.ANY))
				{
					ClearSweet(x, y); //清除颜色
				}
			}
		}
	}


	/// <summary>
	/// 返回主界面
	/// </summary>
	public void ReturnToMain()
	{
		SceneManager.LoadScene(0);
	}


	/// <summary>
	/// 重玩
	/// </summary>
	public void RePlay()
	{
		SceneManager.LoadScene(1);
	}
}