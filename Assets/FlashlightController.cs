using UnityEngine;
using UnityEngine.UI;

public class FlashlightController : MonoBehaviour
{
    #region UI configuration

    [SerializeField]
    private Color flashInactiveColor;
    [SerializeField]
    private Color flashActiveColor;

    #endregion

    #region Current state

    private Color currentColor;

    #endregion

    #region UI

    private Image capImage;

    #endregion

    void Awake()
    {
        var capObject = gameObject.transform.GetChild(1).gameObject;
        capImage = capObject.GetComponent<Image>();

        currentColor = flashInactiveColor;
    }

    void Start()
    {
        var gameController = GameObject.FindObjectOfType<GameController>();
        gameController.flashlightToggleEvent.AddListener(OnFlashlightToggled);
    }

    void Update()
    {
        capImage.color = currentColor;
    }

    private void OnFlashlightToggled(FlashState flashState)
    {
        switch (flashState)
        {
            case FlashState.On:
                currentColor = flashActiveColor;
                break;
            case FlashState.Off:
            default:
                currentColor = flashInactiveColor;
                break;
        }
    }
}
