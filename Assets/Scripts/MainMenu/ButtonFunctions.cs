using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{

    public SavingLoadingBase SaveScript;
   public void Quit(){
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
   } 

   public void Play(int index){
        GameUtils.StartIndependentCoroutine(() => TransitionToScene("OverWorld", "MainMenu", index));
        Destroy(gameObject);
   }

   public IEnumerator TransitionToScene(string newScene, string oldScene, int index){
        Time.timeScale = 0;
        AsyncOperation load = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
        yield return load; // wait until fully loaded

        SaveScript.SaveFileIndex = index;
        SaveScript.LoadAll(index);

        // Now safe to unload the old one
        SceneManager.UnloadSceneAsync(oldScene);
        Time.timeScale = 1;
    }
}
