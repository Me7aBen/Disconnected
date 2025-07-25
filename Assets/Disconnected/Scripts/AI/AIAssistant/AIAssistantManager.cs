using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class AIAssistantManager : MonoBehaviour
{
    [SerializeField] private AIGameSettings aiGameSettings;
    [SerializeField] private GameObject aiSpeechToImage3dAssistant;
    [SerializeField] private GameObject voiceCharactersAssistant;

    [Header("Prompt Overrides")]
    [SerializeField] private ImageSessionPreferences sessionPreferences;


    [Header("Debug")]
    [SerializeField] private AIClientToggle aiClientToggle;

    // TODO: add documents and create our prompt file as reference for our assistants 
    // NOTE: when session stuff changes, need to change here and call other open assistants
    // unsure if need this manager to manage it - but might be important for other stuff

    private HashSet<BaseAIAssistant> assistantsList;
    private BaseAIAssistant currentAssistant;

    // Singleton instance - destroyed when scene ends
    private static AIAssistantManager instance;
    public static AIAssistantManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AIAssistantManager>();
                if (instance == null)
                {
                    Debug.LogError("[AIAssistantManager] AIAssistantManager not found in scene! Make sure to add it to your scene.");
                    throw new System.InvalidOperationException("AIAssistantManager not found in scene. Add it to your scene before using it.");
                }
            }
            return instance;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ensure only one instance exists
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogError("[AIAssistantManager] Multiple AIAssistantManager instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        assistantsList = new();
        Debug.Log("[AIAssistantManager] Initialized successfully");
    }

    [Button]
    public void CreateNewChat()
    {
        // FIXME: assign position, rotation
        var obj = Instantiate(aiSpeechToImage3dAssistant);
        AISpeechToImage3dAssistant newAssistant = obj.GetComponent<AISpeechToImage3dAssistant>();
        newAssistant.Initialize(aiGameSettings);

        assistantsList.Add(newAssistant);
        newAssistant.onClosing.AddListener(RemoveAssistant);
    }

    [Button]
    public void CreateNewVoice()
    {
        // FIXME: assign position, rotation
        var obj = Instantiate(voiceCharactersAssistant);
        AICharacterVoiceAssistant newAssistant = obj.GetComponent<AICharacterVoiceAssistant>();
        newAssistant.Initialize(aiGameSettings);

        assistantsList.Add(newAssistant);
        newAssistant.onClosing.AddListener(RemoveAssistant);
    }

    public BaseAIAssistant.State SetStateAfterOnHold(BaseAIAssistant aiAssistant)
    {
        return currentAssistant == aiAssistant ? BaseAIAssistant.State.Selected : BaseAIAssistant.State.None;
    } 

    public void SelectAssistant(BaseAIAssistant aiAssistant)
    {
        TryUnselectAssistant(currentAssistant);
        currentAssistant = aiAssistant;
    }

    public void TryUnselectAssistant(BaseAIAssistant aiAssistant)
    {
        if (currentAssistant == aiAssistant)
        {
            currentAssistant = null;
        }
    }

    public void RemoveAssistant(BaseAIAssistant toDelete)
    {
        TryUnselectAssistant(toDelete);
        assistantsList.Remove(toDelete);
    }

    public void ClearAllChats()
    {
        foreach (var item in assistantsList)
        {
            item.onClosing.RemoveListener(RemoveAssistant);
        }
        currentAssistant = null;
        assistantsList.Clear();
    }

    void OnDestroy()
    {
        ClearAllChats();
        
        // Clear the singleton instance when destroyed
        if (instance == this)
        {
            instance = null;
        }
    }
}
