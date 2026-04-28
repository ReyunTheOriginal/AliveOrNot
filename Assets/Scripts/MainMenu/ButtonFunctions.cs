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
    public GameObject OptionsUI;

    public void Options(){
        StartCoroutine(OptionsTroll());
    }
    public IEnumerator OptionsTroll(){
        OptionsUI.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(0.05f);
        OptionsUI.gameObject.SetActive(false);
    }
   public void Quit(){
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
   } 

   private void Start() {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("MainMenu")){
            for (int i=0; i<3; i++){
                Saving.SavefileInfo info = Saving.LoadSaveFileInformation(i);

                if (info != null){
                    TimeSpan t = TimeSpan.FromSeconds((double)info.PlayTime);
                    string formatted = "";

                    if (t.Hours > 0)formatted += $"{t.Hours}h";
                    if (t.Minutes > 0)formatted += $"{t.Minutes}m";
                    if (t.Seconds > 0)formatted += $"{t.Seconds}s";

                    if (string.IsNullOrEmpty(formatted))
                        formatted += "1s";

                    saveFileUIs[i].PlayTimeText.text = "PlayTime: \n" + formatted;
                }


                saveFileUIs[i].StartButtonText.text = Saving.SaveFileAvailable(i)? "Play" : "Create";

                saveFileUIs[i].DeleteButton.SetActive(Saving.SaveFileAvailable(i));

                if (!Saving.SaveFileAvailable(i))
                    saveFileUIs[i].PlayTimeText.text = "PlayTime: \n" + "N/A";;
                    
            }
        }
   }

   public void MainStartButton(){
        UIManager.ToggleCanvasGroup(true, StartMenuCanvasGroup, "StartMenu");
        UIManager.ToggleCanvasGroup(true, MainMenuCanvasGroup, "MainMenu");
   }

   public void Play(int index){
        GameUtils.StartIndependentCoroutine(() => StartGame("OverWorld", "MainMenu", index));
        Destroy(gameObject);
   }

   public void Delete(int index){
        if (Saving.SaveFileAvailable(index))
            Directory.Delete(Path.Combine(Application.persistentDataPath, $"SaveFile_{index}"), true);

        Start();
   }

   public IEnumerator StartGame(string newScene, string oldScene, int index){
        Time.timeScale = 0;

        AsyncOperation load = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
        yield return load; // wait until fully loaded
        GameUtils.SetAllChunkRendering(false);
        // Now safe to unload the old one
        SceneManager.UnloadSceneAsync(oldScene);

        Saving.SaveFileIndex = index;

        if (!Saving.SaveFileAvailable(index)){
            GameServices.WorldGenerationBase.Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            Saving.SaveAll(index);
        }else{
            Saving.LoadAll(index);
        }

        Saving.SavefileInfo info = Saving.LoadSaveFileInformation(index);

        GameServices.GlobalTimer = info.PlayTime;

        GameServices.WorldGenerationBase.Seed = info.Seed;

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
