using UnityEngine;
using System.Collections;

public abstract class UnitySingleton<T> : MonoBehaviour where T : UnitySingleton<T>
{
	private static T instance;
	private static object _lock = new object();
	protected static bool applicationIsQuitting = false;
	public static bool IsApplicationQuitting
	{
		get
		{
			return applicationIsQuitting;
		}
	}

	public static T I
	{
		get
		{
			if (applicationIsQuitting)
			{
				Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
					"' already destroyed on application quit." +
					" Won't create again - returning null.");
				return null;
			}

			lock (_lock)
			{
				if (instance == null)
				{
					instance = FindObjectOfType(typeof(T)) as T;
					if (instance == null)
					{
						GameObject singleton = new GameObject(typeof(T).Name);
						instance = singleton.AddComponent<T>();
						instance.Init();
						instance.name = typeof(T).ToString();
						//DontDestroyOnLoad(instance.gameObject);
					}
				}
				return instance;
			}
		}
	}

	void Awake()
	{
		lock (_lock)
		{
			if (instance == null)
			{
				instance = this as T;

			}
			else if (instance != this)
			{
				Debug.LogError(this.name + " Destroy !!!");
				Destroy(this.gameObject);
				return;
			}
		}
	}

	protected virtual void Init()
	{
	}

	protected virtual void OnApplicationQuit()
	{
#if UNITY_EDITOR
#else
		applicationIsQuitting = true;
#endif
	}
}

public abstract class ISingleton<T> where T : class, new()
{
	static volatile T instance = default(T);
	static readonly object padlock = new object();
	protected ISingleton()
	{
	}
	public static T I
	{
		get
		{
			lock (padlock)
			{
				if (instance == null)
				{
					instance = new T();
					(instance as ISingleton<T>).Init();
				}
				return instance;
			}
		}
	}
	public virtual void Init() { }
}