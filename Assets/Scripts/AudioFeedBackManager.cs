using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

/*
 * AudioFeedBackManager script
 * Script for receiving audio feedback from user
 */

public class AudioFeedBackManager : MonoBehaviour
{
    public bool PersistentKeywords;
    public List<string> Keywords = new List<string> { "Yes", "No", "Finish" };
    private ConfidenceLevel recognitionConfidenceLevel = ConfidenceLevel.Medium;
    private KeywordRecognizer keywordRecognizer;
    //Singleton
    public static AudioFeedBackManager Instance { get; private set; }

    private void Start()
    {
        int keywordCount = Keywords.Count;
        if (keywordCount > 0)
        {
            var keywords = new string[keywordCount];

            for (int index = 0; index < keywordCount; index++)
                keywords[index] = Keywords[index];

            keywordRecognizer = new KeywordRecognizer(keywords, recognitionConfidenceLevel);
            keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        }
    }

    private void OnDestroy()
    {
        if (keywordRecognizer != null)
        {
            StopKeywordRecognizer();
            keywordRecognizer.OnPhraseRecognized -= KeywordRecognizer_OnPhraseRecognized;
            keywordRecognizer.Dispose();
        }
    }

    private void OnDisable()
    {
        if (keywordRecognizer != null)
            StopKeywordRecognizer();
    }

    private void OnEnable()
    {
        if (keywordRecognizer != null)
            StartKeywordRecognizer();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        OnPhraseRecognized(args.confidence, args.phraseDuration, args.phraseStartTime, args.semanticMeanings, args.text);
    }

    private void OnPhraseRecognized(ConfidenceLevel confidence, TimeSpan phraseDuration, DateTime phraseStartTime, SemanticMeaning[] semanticMeanings, string text)
    {
        if (text.Contains("Yes"))
        {
            EventManager.TriggerEvent("user_said_yes");
        }
        else if (text.Contains("No"))
        {
            EventManager.TriggerEvent("user_said_no");
        }
        else if (text.Contains("Finish"))
        {
            EventManager.TriggerEvent("user_said_finish");
        }
    }

    public void StartKeywordRecognizer()
    {
        if (keywordRecognizer != null && !keywordRecognizer.IsRunning)
            keywordRecognizer.Start();
    }

    public void StopKeywordRecognizer()
    {
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
            keywordRecognizer.Stop();
    }
}