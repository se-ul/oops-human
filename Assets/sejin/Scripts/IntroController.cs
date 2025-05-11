using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    [SerializeField] private string mainSceneName = "Main";

    // UI Button OnClick에 연결할 메서드
    public void OnStartButton()
    {
        // 씬 전환. 동기 로드면 짧게 끊김.
        SceneManager.LoadScene(mainSceneName);
        // 혹은 비동기:
        // StartCoroutine(LoadAsync());

        // IEnumerator LoadAsync()
        // {
        //     AsyncOperation op = SceneManager.LoadSceneAsync(mainSceneName);
        //     while (!op.isDone) yield return null;
        // }
    }
}


