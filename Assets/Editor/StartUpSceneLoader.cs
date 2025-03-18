using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class StartUpSceneLoader
{
   static StartUpSceneLoader()
   {
      // 에디터 플레이 모드 상태가 변경될 때, LoadStartUpScene 함수가 자동실행
      EditorApplication.playModeStateChanged += LoadStartUpScene;
   }

   private static void LoadStartUpScene(PlayModeStateChange state)
   {
      if (state == PlayModeStateChange.ExitingEditMode)
      {
         //현재 열려있는 씬 저장
         EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
      }

      if (state == PlayModeStateChange.EnteredPlayMode)
      {
         if (EditorSceneManager.GetActiveScene().buildIndex != 0)
         {
            EditorSceneManager.LoadScene(0);
         }
      }
   }
}
