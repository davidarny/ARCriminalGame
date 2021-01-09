using UnityEngine;

public class ToastService : MonoBehaviour
{
    void OnEnable()
    {
        Application.logMessageReceived += ShowAndroidToastMessage;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= ShowAndroidToastMessage;
    }

    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    private void ShowAndroidToastMessage(string message, string stacktrace, LogType type)
    {
        var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = player.GetStatic<AndroidJavaObject>("currentActivity");

        if (activity != null)
        {
            var toastClass = new AndroidJavaClass("android.widget.Toast");
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                var toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", activity, message, 0);
                toastObject.Call("show");
            }));
        }
    }
}
