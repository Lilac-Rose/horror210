using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuController : MonoBehaviour
{
    [Header("Button Colors")]
    public Color normalColor = Color.black;
    public Color hoverColor = Color.gray;

    [Header("Text Elements (for hover effects)")]
    public TMP_Text startButton;
    public TMP_Text optionsButton;
    public TMP_Text exitButton;

    [Header("Button Components (for click handling)")]
    public Button startButtonComponent;
    public Button optionsButtonComponent;
    public Button exitButtonComponent;

    [Header("Canvases")]
    public Canvas mainMenuCanvas;
    public Canvas optionsCanvas;

    void Awake()
    {
        if (startButton != null)
        {
            startButton.color = normalColor;
        }
        if (optionsButton != null) optionsButton.color = normalColor;
        if (exitButton != null) exitButton.color = normalColor;
    }

    void Start()
    {
        // Wire up button clicks
        if (startButtonComponent != null)
            startButtonComponent.onClick.AddListener(StartGame);
        if (optionsButtonComponent != null)
            optionsButtonComponent.onClick.AddListener(OpenOptions);
        if (exitButtonComponent != null)
            exitButtonComponent.onClick.AddListener(ExitGame);

        // Add hover listeners to buttons
        AddHoverListeners(startButtonComponent, startButton);
        AddHoverListeners(optionsButtonComponent, optionsButton);
        AddHoverListeners(exitButtonComponent, exitButton);
    }

    void AddHoverListeners(Button button, TMP_Text text)
    {
        if (button == null || text == null) return;

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        // Clear existing triggers to avoid duplicates
        trigger.triggers.Clear();

        // Pointer Enter (hover start)
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => {
            text.color = hoverColor;
        });
        trigger.triggers.Add(entryEnter);

        // Pointer Exit (hover end)
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => {
            text.color = normalColor;
        });
        trigger.triggers.Add(entryExit);
    }

    public void CloseOptions()
    {
        optionsCanvas.enabled = false;
        mainMenuCanvas.enabled = true;
    }

    // -----------------------
    // BUTTON FUNCTIONS
    // -----------------------
    public void StartGame()
    {
        SceneManager.LoadScene("Main");
    }

    public void OpenOptions()
    {
        mainMenuCanvas.enabled = false;
        optionsCanvas.enabled = true;
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Debug.Log("Game Quit");
    }
}