using PurrNet;
using UnityEngine;

namespace Scripts
{
    public class NetSceneManager : NetworkBehaviour
    {
        [PurrScene] public string _sceneName;

        [ContextMenu("ChangeScene")]
        private void changeScene()
        {
            networkManager.sceneModule.LoadSceneAsync(_sceneName);
        }
    }
}
