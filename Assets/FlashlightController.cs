using UnityEngine;
using UnityEngine.UI;

// FFE900
public class FlashlightController : MonoBehaviour
{
    [SerializeField]
    private Color flashInactiveColor;
    [SerializeField]
    private Color flashActiveColor;

    private Color currentColor;

    private Image capImage;

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
