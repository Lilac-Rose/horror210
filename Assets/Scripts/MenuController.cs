using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Button Colors")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.cyan;

    [Header("Buttons")]
    public TMP_Text startButton;
    public TMP_Text optionsButton;
    public TMP_Text exitButton;

    [Header("Canvases")]
    public Canvas mainMenuCanvas;
    public Canvas optionsCanvas;

    private TMP_Text currentHover;

    void Update()
    {
        HandleHover();
        HandleClick();
    }

    void HandleHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        TMP_Text hitText = null;

        if (Physics.Raycast(ray, out hit))
        {
            hitText = hit.collider.GetComponent<TMP_Text>();
        }

        // If hovered object changed, update colors
        if (currentHover != hitText)
        {
            if (currentHover != null)
                currentHover.color = normalColor;

            currentHover = hitText;

            if (currentHover != null)
                currentHover.color = hoverColor;
        }
    }

    void HandleClick()
    {
        if (currentHover == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (currentHover == startButton)
            {
                StartGame();
            }
            else if (currentHover == optionsButton)
            {
                OpenOptions();
            }
            else if (currentHover == exitButton)
            {
                ExitGame();
            }
        }
    }

    // -----------------------
    // BUTTON FUNCTIONS
    // -----------------------

    void StartGame()
    {
        SceneManager.LoadScene("Main");
    }

    void OpenOptions()
    {
        mainMenuCanvas.enabled = false;
        optionsCanvas.enabled = true;
    }

    void ExitGame()
    {
        // Quit in a build
        Application.Quit();

        // Quit play mode in the editor
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif

        Debug.Log("Game Quit");
    }

}
