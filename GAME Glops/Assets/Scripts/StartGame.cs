using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class StartGame : MonoBehaviour {
	/// <summary>
	/// 加载游戏
	/// </summary>
	public void LoadTheGame()
	{
		SceneManager.LoadScene(1);
	}

	/// <summary>
	/// 退出游戏
	/// </summary>
	public void ExitGame()
	{
		Application.Quit();
	}

	/// <summary>
	/// 初始化函数
	/// </summary>
	void Start()
	{
		Button button = GameObject.Find("Exit_Button").GetComponent<Button>();
		button.onClick.AddListener(ExitGame);
		button = GameObject.Find("Start_Button").GetComponent<Button>();
		button.onClick.AddListener(LoadTheGame);
	}
}
