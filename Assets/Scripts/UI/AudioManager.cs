using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; } // 公开的单例实例

    private void Awake()
    {
        // 单例模式：确保全局只有一个AudioManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 让此管理器对象持久化
        }
        else
        {
            Destroy(gameObject); // 如果已存在，则销毁新的重复对象
        }
    }

    // 可以在此添加更多控制音乐的方法，例如 PlayMusic(), StopMusic(), ChangeVolume() 等
}