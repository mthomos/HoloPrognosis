using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class AudioFeedBackManager : MonoBehaviour
{
    public FlowController flowController;
    public TextMesh DebugDisplay;
    public bool PersistentKeywords; // Destroy AudioFeedBackManager When i switch scenes
    public List<string> Keywords;
    //
    private ConfidenceLevel recognitionConfidenceLevel = ConfidenceLevel.Medium;
    private KeywordRecognizer keywordRecognizer;

    void Start()
    {
        //if (PersistentKeywords)
         //   this.gameObject.DontDestroyOnLoad();
        
        int keywordCount = Keywords.Count;
        if (keywordCount > 0)
        {
            var keywords = new string[keywordCount];

            for (int index = 0; index < keywordCount; index++)
                keywords[index] = Keywords[index];

            keywordRecognizer = new KeywordRecognizer(keywords, recognitionConfidenceLevel);
            keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
            //keywordRecognizer.Start();
        }
    }

    void OnDestroy()
    {
        if (keywordRecognizer != null)
        {
            StopKeywordRecognizer();
            keywordRecognizer.OnPhraseRecognized -= KeywordRecognizer_OnPhraseRecognized;
            keywordRecognizer.Dispose();
        }
    }

    void OnDisable()
    {
        if (keywordRecognizer != null)
            StopKeywordRecognizer();
    }

    void OnEnable()
    {
        if (keywordRecognizer != null)
            StartKeywordRecognizer();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        OnPhraseRecognized(args.confidence, args.phraseDuration, args.phraseStartTime, args.semanticMeanings, args.text);
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

    protected void OnPhraseRecognized(ConfidenceLevel confidence, TimeSpan phraseDuration, DateTime phraseStartTime, SemanticMeaning[] semanticMeanings, string text)
    {
        //Use semanticMeanings and text
        DebugDisplay.text = "text: " + text;
        if (text.Contains("Yes") )
        {
            flowController.userSaidYes();
        }
        else if (text.Contains("No"))
        {
            flowController.userSaidNo();
        }
    }

}