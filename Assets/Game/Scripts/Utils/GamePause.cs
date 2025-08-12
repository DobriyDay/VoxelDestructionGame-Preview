using System.Collections.Generic;
using UnityEngine;

public enum PauseSource
{
    Game = 0,
    Focus = 1,
    Ad = 2
}

public static class GamePause
{
    private static float _savedTimeScale = 1f;
    private static readonly Dictionary<PauseSource, bool> _pauseStates = new();
    private static bool _anyPausedBefore = false;

    static GamePause()
    {
        foreach (PauseSource source in (PauseSource[])System.Enum.GetValues(typeof(PauseSource)))
            _pauseStates[source] = false;
    }

    public static bool IsPaused()
    {
        foreach (var state in _pauseStates)
            if (state.Value)
                return true;
        return false;
    }

    public static void SetPause(PauseSource source, bool isPaused, bool pauseAudio = true)
    {
        _pauseStates[source] = isPaused;
        UpdatePauseState(pauseAudio);
    }

    private static void UpdatePauseState(bool pauseAudio)
    {
        bool anyPaused = IsPaused();

        if (anyPaused && !_anyPausedBefore)
        {
            _savedTimeScale = (Time.timeScale == 0 ? 1f : Time.timeScale);
            Time.timeScale = 0f;
            AudioListener.pause = pauseAudio;
        }
        else if (anyPaused)
        {
            Time.timeScale = 0f;
            AudioListener.pause = pauseAudio;
        }
        else
        {
            Time.timeScale = _savedTimeScale;
            AudioListener.pause = false;
        }

        _anyPausedBefore = anyPaused;
    }
}