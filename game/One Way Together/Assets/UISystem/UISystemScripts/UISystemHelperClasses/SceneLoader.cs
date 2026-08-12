using UnityEngine;
using UnityEngine.SceneManagement;

namespace LB.UI.System
{
	public class SceneLoader : MonoBehaviour
	{
		public void SceneLoadWithBuildIndex(int id)
		{
			SceneManager.LoadScene(id);
		}

		public void SceneLoadWithSceneName(string sceneName)
		{
			SceneManager.LoadScene(sceneName);
		}
	}
}