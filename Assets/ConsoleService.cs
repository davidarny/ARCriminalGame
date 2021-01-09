// NOTE: For an actively-maintained version of this script, see https://github.com/mminer/consolation.

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A console to display Unity's debug logs in-game.
/// </summary>
public class ConsoleService : MonoBehaviour
{
    private struct Log
    {
        public string message;
        public string stacktrace;
        public LogType type;
    }

    /// <summary>
    /// The hotkey to show and hide the console window.
    /// </summary>
    public KeyCode toggleKey = KeyCode.BackQuote;

    private List<Log> logs = new List<Log>();
    private Vector2 scrollPosition;

    private bool show = true;
    private bool collapse;

    // Visual elements:

    private static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>()
    {
        { LogType.Assert, Color.white },
        { LogType.Error, Color.red },
        { LogType.Exception, Color.red },
        { LogType.Log, Color.white },
        { LogType.Warning, Color.yellow },
    };

    private const int margin = 20;

    private const int width = 640;
    private const int height = 480;

    private Rect windowRect = new Rect(margin, margin, width - (margin * 2), height - (margin * 2));
    private Rect titleBarRect = new Rect(0, 0, width, margin);

    private GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
    private GUIContent collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            show = !show;
        }
    }

    void OnGUI()
    {
        if (!show)
        {
            return;
        }

        windowRect = GUILayout.Window(0, windowRect, ConsoleWindow, "Console");
    }

    /// <summary>
    /// A window that displayss the recorded logs.
    /// </summary>
    /// <param name="windowID">Window ID.</param>
    private void ConsoleWindow(int windowID)
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        // Iterate through the recorded logs.
        for (int i = 0; i < logs.Count; i++)
        {
            var log = logs[i];

            // Combine identical messages if collapse option is chosen.
            if (collapse)
            {
                var messageSameAsPrevious = i > 0 && log.message == logs[i - 1].message;

                if (messageSameAsPrevious)
                {
                    continue;
                }
            }

            GUI.contentColor = logTypeColors[log.type];
            GUILayout.Label(log.message);
        }

        GUILayout.EndScrollView();

        GUI.contentColor = Color.white;

        GUILayout.BeginHorizontal();

        if (GUILayout.Button(clearLabel))
        {
            logs.Clear();
        }

        collapse = GUILayout.Toggle(collapse, collapseLabel, GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();

        // Allow the window to be dragged by its title bar.
        GUI.DragWindow(titleBarRect);
    }

    /// <summary>
    /// Records a log from the log callback.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="stacktrace">Trace of where the message came from.</param>
    /// <param name="type">Type of message (error, exception, warning, assert).</param>
    private void HandleLog(string message, string stacktrace, LogType type)
    {
        logs.Add(new Log()
        {
            message = message,
            stacktrace = stacktrace,
            type = type,
        });
    }
}
