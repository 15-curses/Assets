using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Assets.ObjectManager
{
    public class DinamicManager : MonoBehaviour
    {
        public static DinamicManager Instance {get; private set;}

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
       
        #region Методы для управления физикой
        private Rigidbody RigidbodyOfThis(GameObject gameObject) => gameObject.GetComponent<Rigidbody>();

        public GameObject TeleportLogic(GameObject gameObject, Vector3 newPosition)
        {
            gameObject.transform.position = newPosition;
            return gameObject;
        }

        #region [Некинематик]
        public void PushInDirectionLogic(GameObject gameObject, Vector3 endV3, Vector3? startV3 = null)
        {
            if (startV3 != null)
            {
                gameObject = TeleportLogic(gameObject, (Vector3)startV3);
            }

            var rb = RigidbodyOfThis(gameObject);

            if (rb == null)
            {
                // ошибку
                return;
            }
            rb.AddForce(endV3, ForceMode.Impulse);
        }

        #endregion

        #region [Кинематик]
        public void MoveToPositionLogic(GameObject gameObject, Vector3 endV3, float speed = 0)
        {
            if (gameObject == null | endV3 == null | speed == 0) return;

            gameObject.transform.position = Vector3.MoveTowards(
                gameObject.transform.position,
                endV3,
                speed * Time.deltaTime
            );
        }
        #endregion
        #endregion
    }
}