using UnityEngine;
using System.Collections;

public class speedController : MonoBehaviour
{
    public Animator animator; // 将您的 Animator 组件拖放到这个字段
    public float delay = 5f; // 延迟时间（秒）

    void Start()
    {
        if (animator == null)
        {
            Debug.LogError("Animator not assigned.");
            return;
        }
        StartCoroutine(PlayAnimationAfterDelay(delay));
    }

    IEnumerator PlayAnimationAfterDelay(float delayTime)
    {
        yield return new WaitForSeconds(delayTime); // 等待指定的延迟时间
        animator.Play("AnimationStateName"); // 开始播放动画
    }
}