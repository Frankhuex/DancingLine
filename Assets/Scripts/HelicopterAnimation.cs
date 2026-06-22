using UnityEngine;

public class HelicopterAnimation : MonoBehaviour
{
    public Animator animator;      // 拖入你的 Animator 组件
    public string triggerName;     // 填入 Animator 中的 Trigger 参数名

    // 这是一个公共方法，可以被 UnityEvent 调用
    public void PlayAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(triggerName))
        {
            animator.SetTrigger(triggerName);
        }
        else
        {
            Debug.LogWarning("Animator 或 Trigger 名称未设置！");
        }
    }
}
