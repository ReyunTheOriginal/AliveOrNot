using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using System.Collections.Generic;
using System;

public class ButtonFunctions : MonoBehaviour
{
    public CanvasGroup StartMenuCanvasGroup;
    public CanvasGroup MainMenuCanvasGroup;

    public List<SaveFileUI> saveFileUIs;

    public SavingLoadingBase SaveScript;
   public void Quit(){
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
   } 

   private void Start() {
        for (int i=0; i<3; i++)
            SaveScript.LoadSaveFileInformation(i);

        for (int i=0; i<3; i++){
            if (SaveScript.SavefileInformation.ContainsKey(i)){
                TimeSpan t = TimeSpan.FromSeconds((double)SaveScript.SavefileInformation[i].PlayTime);
                string formatted = "";

                if (t.Hours > 0)formatted += $"{t.Hours}h";
                if (t.Minutes > 0)formatted += $"{t.Minutes}m";
                if (t.Seconds > 0)formatted += $"{t.Seconds}s";

                if (string.IsNullOrEmpty(formatted))
                    formatted += "0s";

                saveFileUIs[i].PlayTimeText.text = "PlayTime: \n" + formatted;
            }


            saveFileUIs[i].StartButtonText.text = SaveScript.SaveFileAvailable(i)? "Play" : "Create";

            saveFileUIs[i].DeleteButton.SetActive(SaveScript.SaveFileAvailable(i));

            if (!SaveScript.SaveFileAvailable(i))
                saveFileUIs[i].PlayTimeText.text = "PlayTime: \n" + "N/A";;
                
        }
   }

   public void MainStartButton(){
        UIManager.ToggleCanvasGroup(true, StartMenuCanvasGroup, "StartMenu");
        UIManager.ToggleCanvasGroup(true, MainMenuCanvasGroup, "MainMenu");
   }

   public void Play(int index){
        GameUtils.StartIndependentCoroutine(() => TransitionToScene("OverWorld", "MainMenu", index));
        Destroy(gameObject);
   }

   public void Delete(int index){
        if (SaveScript.SaveFileAvailable(index))
            Directory.Delete(Path.Combine(Application.persistentDataPath, $"SaveFile_{index}"), true);

        Start();
   }

   public IEnumerator TransitionToScene(string newScene, string oldScene, int index){
        Time.timeScale = 0;
        AsyncOperation load = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
        yield return load; // wait until fully loaded
        GameUtils.SetAllChunkRendering(false);
        // Now safe to unload the old one
        SceneManager.UnloadSceneAsync(oldScene);

        SaveScript.SaveFileIndex = index;

        SaveScript.LoadAll(index);

        GameUtils.SetAllChunkRendering();
        Time.timeScale = 1;
    }

    [System.Serializable]
    public class SaveFileUI{
        public TMP_Text Name;
        public TMP_Text StartButtonText;
        public TMP_Text PlayTimeText;
        public GameObject DeleteButton;
    }
}
