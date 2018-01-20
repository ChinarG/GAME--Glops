using UnityEngine;
using System.Collections;


/// <summary>
/// 控制移动脚本
/// </summary>
public class MovedSweet : MonoBehaviour
{
	private GameSweet   sweet;
	private IEnumerator moveCoroutine; //这样得到其他指令的时候，我们可以终止这个协成


	private void Awake()
	{
		sweet = GetComponent<GameSweet>(); //获取组件
	}


	/// <summary>
	/// 开启，或者关闭协成
	/// </summary>
	/// <param name="newx"></param>
	/// <param name="newy"></param>
	public void Move(int newx, int newy, float time)
	{
		if (moveCoroutine != null)
		{
			StopCoroutine(moveCoroutine);//停止协成
		}
		moveCoroutine = MoveCoroutine(newx, newy, time);
		StartCoroutine(moveCoroutine);//重开协成
	}


	/// <summary>
	/// 负责移动的协成
	/// </summary>
	/// <param name="newx"></param>
	/// <param name="newy"></param>
	/// <param name="time"></param>
	/// <returns></returns>
	private IEnumerator MoveCoroutine(int newx, int newy, float time)
	{
		sweet.X          = newx;
		sweet.Y          = newy;
		Vector3 startPos = transform.position;//每一帧移动一点
		Vector3 endPos   = sweet.gameManager.CorrectPosition(newx, newy);
		for (float t = 0; t < time; t += Time.deltaTime)
		{
			sweet.transform.position = Vector3.Lerp(startPos, endPos, t / time);
			yield return 0;//等待一帧
		}
		sweet.transform.position = endPos; //如果发生意外 没移动，就直接赋值
	}


	void Start()
	{
	}


	void Update()
	{
	}
}