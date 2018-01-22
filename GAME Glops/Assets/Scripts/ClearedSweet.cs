using UnityEngine;
using System.Collections;


/// <summary>
/// 清除管控类脚本
/// </summary>
public class ClearedSweet : MonoBehaviour
{
	public  AnimationClip clearAnimation; //动画
	public  AudioClip     DestoryClip;    //消除音效
	private bool          isClearing;     //是否正在清除
	public  bool          IsClearing
	{
		get { return isClearing; }
	}

	protected GameSweet sweet; //可扩充


	/// <summary>
	/// 唤醒函数
	/// </summary>
	private void Awake()
	{
		sweet = GetComponent<GameSweet>();
	}


	/// <summary>
	/// 清除
	/// </summary>
	public virtual void Clear()
	{
		isClearing = true; //正在被清除
		StartCoroutine(ClearCoroutine());
	}


	/// <summary>
	/// 清除动画协成
	/// </summary>
	/// <returns></returns>
	private IEnumerator ClearCoroutine()
	{
		BoxCollider collider = GetComponent<BoxCollider>();
		if (collider!=null)//容错
		{
			collider.enabled = false;
		}

		Animator animator = GetComponent<Animator>();
		if (animator != null)
		{
			animator.Play(clearAnimation.name); //播放清除动画

			GameManager.Instance.PlayerScore++;                           //得分
			AudioSource.PlayClipAtPoint(DestoryClip, transform.position); //播放消除音效
			yield return new WaitForSeconds(clearAnimation.length);

			Destroy(gameObject);
		}
	}
}