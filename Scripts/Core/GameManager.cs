using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("State Data")]
    [SerializeField] private GameStateDataSO menuData;
    [SerializeField] private GameStateDataSO gamePlayData;
    [SerializeField] private GameStateDataSO gameOverData;
    [SerializeField] private GameStateDataSO winData;

    private GameStateDataSO currentState;
    private List<IStateService> stateServices = new List<IStateService>();
    private bool isTransitioning = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Debug.LogWarning($"Уничтожаем дубликат GameManager: {gameObject.name}");
            Destroy(gameObject);
        }
    }
    void Start()
    {
        FindAllStateServices();
        if (menuData != null)
            SwitchToState(menuData);
        else
            ;  // сюда ошибку
    }

    private void FindAllStateServices()
    {
        stateServices.Clear();

        var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
        int foundCount = 0;

        foreach (var mb in allMonoBehaviours)
        {
            if (mb != null && mb.gameObject.activeInHierarchy && mb.enabled)
            {
                if (mb is IStateService service)
                {
                    stateServices.Add(service);
                    foundCount++;
                }
            }
        }
        Debug.Log($"GameManager: Найдено {stateServices.Count} сервисов");
    }

    void Update()
    {
        if(currentState != null && !isTransitioning)
        {
            foreach (var service in stateServices)
            {
                if (service != null)
                    service.OnUpdate(currentState);
            }
        }
    }

    public void SwitchToState(GameStateDataSO newState)
    {

        if (isTransitioning)
        {
            //LogReader.Read(""); сюда кастом ошибку
            Debug.Log($"asasasassssssss");
            return;
        }
        if (newState == null)
        {
           // LogReader.Read("");  надо ошибку написать
           return;
        }
        if(currentState != null)
        {
            foreach(var service in stateServices)
                service.OnExit(currentState);
        }
        isTransitioning = true;
        Debug.Log($"Переключение состояния: {currentState?.type} -> {newState.type}");

        Time.timeScale = newState.timeScale;
        Cursor.lockState = newState.cursorMode;
        Cursor.visible = newState.cursorVisible;

        currentState = newState;
        foreach(var service in stateServices)
            service.OnEnter(currentState);
        isTransitioning = false;
    }

    public void GoToMenu()
    {
        if (menuData != null) SwitchToState(menuData);
        else Debug.LogError("MenuData не назначено!"); //LogReader.Read(""); сюда кастом ошибку
    }

    public void StartGame()
    {
        if (gamePlayData != null) SwitchToState(gamePlayData);
        else Debug.LogError("GamePlayData не назначено!"); //LogReader.Read(""); сюда кастом ошибку
    }

    public void GameOver()
    {
        if (gameOverData != null) SwitchToState(gameOverData);
        else Debug.LogError("GameOverData не назначено!"); //LogReader.Read(""); сюда кастом ошибку
    } 

    public void WinGame()
    {
        if (winData != null) SwitchToState(winData);
        else Debug.LogError("WinData не назначено!");  //LogReader.Read(""); сюда кастом ошибку
    }

    public void ReloadServices()
    {
        FindAllStateServices();

        if (currentState != null)
        {
            foreach (var service in stateServices)
            {
                if (service != null)
                    service.OnEnter(currentState);
            }
        }
    }
    public GameStateDataSO GetCurrentState() => currentState;
    // пример public void Run() { var currentState = GetCurrentState(); if(currentState == )  }
}