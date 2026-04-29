using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class CustomLogger
{
    #region 카테고리별 전용 로그
    /// <summary>
    /// 데이터 로그를 출력하는 함수
    /// </summary>
    /// <param name="msg"></param>
    [Conditional("UNITY_EDITOR")]
    public static void LogData(string msg)
    {
        Emit(LogType.Log, $"<color=#00FFFF>[DATA]</color> {msg}");
    }

    /// <summary>
    /// 전투 로그를 출력하는 함수
    /// </summary>
    /// <param name="msg"></param>
    [Conditional("UNITY_EDITOR")]
    public static void LogBattle(string msg)
    {
        Emit(LogType.Log, $"<color=#FF9100>[BATTLE]</color> {msg}");
    }

    /// <summary>
    /// 시스템 로그를 출력하는 함수
    /// </summary>
    /// <param name="msg"></param>
    [Conditional("UNITY_EDITOR")]
    public static void LogSystem(string msg)
    {
        Emit(LogType.Log, $"<color=#00C853>[SYSTEM]</color> {msg}");
    }

    /// <summary>
    /// 경고 메세지를 출력하는 함수
    /// </summary>
    /// <param name="msg"></param>
    [Conditional("UNITY_EDITOR")]
    public static void Warn(string msg)
    {
        Emit(LogType.Warning, $"<color=yellow>[WARN]</color> {msg}");
    }

    /// <summary>
    /// 오류 메세지를 출력하는 함수
    /// </summary>
    /// <param name="msg"></param>
    [Conditional("UNITY_EDITOR")]
    public static void Error(string msg)
    {
        Emit(LogType.Error, $"<color=#FF1744>[ERROR]</color> {msg}");
    }
    #endregion

    #region 들여쓰기 및 그룹 로그
    private static int _indentLevel = 0;
    private const int INDENT_SPACES = 2;
    private static string Indent => new string(' ', _indentLevel * INDENT_SPACES);

    [Conditional("UNITY_EDITOR")]
    public static void IndentPush() { _indentLevel++; }

    [Conditional("UNITY_EDITOR")]
    public static void IndentPop() { _indentLevel = Mathf.Max(0, _indentLevel - 1); }

    [Conditional("UNITY_EDITOR")]
    public static void Group(string title, Action body)
    {
        LogSystem($"==== {title} ====");
        IndentPush();
        body?.Invoke();
        IndentPop();
        LogSystem(new string('=', title.Length + 10));
    }
    #endregion

    #region 한 번만 출력
    private static readonly HashSet<string> _onceSet = new HashSet<string>();

    [Conditional("UNITY_EDITOR")]
    public static void Once(string key, string msg)
    {
        if (_onceSet.Contains(key)) return;
        _onceSet.Add(key);
        Warn($"[Once] {msg}");
    }

    [Conditional("UNITY_EDITOR")]
    public static void OnceClear()
    {
        _onceSet.Clear();
    }
    #endregion

    #region 코어 출력 로직
    [Conditional("UNITY_EDITOR")]
    private static void Emit(LogType type, string msg)
    {
        string finalMsg = $"{Indent}{msg}";

        switch (type)
        {
            case LogType.Log: UnityEngine.Debug.Log(finalMsg); break;
            case LogType.Warning: UnityEngine.Debug.LogWarning(finalMsg); break;
            case LogType.Error: UnityEngine.Debug.LogError(finalMsg); break;
        }
    }
    #endregion
}
