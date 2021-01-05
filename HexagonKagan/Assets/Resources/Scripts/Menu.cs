using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void OnClickPlay()
    {
        SceneManager.LoadScene("Game");
    }
}
