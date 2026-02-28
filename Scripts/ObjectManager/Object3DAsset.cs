using UnityEngine;
using System.Collections;

namespace Assets.CreateObjects
{
    [CreateAssetMenu(fileName = "New 3D Object", menuName = "3D Object System/Object Asset")]
    public class Object3DAsset : ScriptableObject
    {
        [Header("Основные настройки")]
        public string objectName;
        public GameObject prefab;
        public Sprite icon;

        [Header("Трансформ")]
        public Vector3 position = Vector3.zero;
        public Vector3 rotation = Vector3.zero;
        public Vector3 scale = Vector3.one;

        [Header("Физика")]
        public bool hasRigidbody = false;
        public float mass = 1f;
        public float drag = 0f;
        public float angularDrag = 0.05f;
        public bool useGravity = true;
        public bool isKinematic = false;

        public CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.Discrete;
        public RigidbodyInterpolation interpolate = RigidbodyInterpolation.None;

        [Header("Коллайдер")]
        public ColliderType colliderType = ColliderType.Box;
        public bool isTrigger = false;
        public Vector3 colliderSize = Vector3.one;
        public float colliderRadius = 0.5f;
        public float colliderHeight = 2f;

        [Header("Материалы")]
        public Material[] materials;
        public bool castShadows = true;
        public bool receiveShadows = true;

        [Header("Теги и слои")]
        public string tag = "Untagged";
        public int layer = 0;


        [Header("Дополнительно")]
        public string[] sounds;
        public GameObject[] effects;
        public string description;

        public enum ColliderType
        {
            None,
            Box,
            Sphere,
            Capsule,
            Mesh
        }

        public virtual void ConfigureObject(GameObject obj)
        {
            if (obj == null) return;

            obj.transform.localScale = scale;

            obj.tag = tag;
            obj.layer = layer;

            ConfigurePhysics(obj);

            ConfigureCollider(obj);

            ConfigureMaterials(obj);

            obj.BroadcastMessage("OnObjectConfigured", this, SendMessageOptions.DontRequireReceiver);
        }

        void ConfigurePhysics(GameObject obj)
        {
            if (!hasRigidbody) return;

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb == null) rb = obj.AddComponent<Rigidbody>();

            rb.mass = mass;
            rb.drag = drag;
            rb.angularDrag = angularDrag;
            rb.useGravity = useGravity;
            rb.isKinematic = isKinematic;
            rb.collisionDetectionMode = collisionDetectionMode;
            rb.interpolation = interpolate;
        }

        void ConfigureCollider(GameObject obj)
        {
            if (colliderType == ColliderType.None) return;

            var oldColliders = obj.GetComponents<Collider>();
            foreach (var col in oldColliders)
            {
                DestroyImmediate(col);
            }

            Collider collider = null;

            switch (colliderType)
            {
                case ColliderType.Box:
                    var box = obj.AddComponent<BoxCollider>();
                    box.size = colliderSize;
                    collider = box;
                    break;

                case ColliderType.Sphere:
                    var sphere = obj.AddComponent<SphereCollider>();
                    sphere.radius = colliderRadius;
                    collider = sphere;
                    break;

                case ColliderType.Capsule:
                    var capsule = obj.AddComponent<CapsuleCollider>();
                    capsule.radius = colliderRadius;
                    capsule.height = colliderHeight;
                    collider = capsule;
                    break;

                case ColliderType.Mesh:
                    obj.AddComponent<MeshCollider>();
                    break;
            }

            if (collider != null)
            {
                collider.isTrigger = isTrigger;
            }
        }

        void ConfigureMaterials(GameObject obj)
        {
            if (materials == null || materials.Length == 0) return;

            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = castShadows ?
                    UnityEngine.Rendering.ShadowCastingMode.On :
                    UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = receiveShadows;
            }
        }
        private void PlaySpawnEffects(GameObject obj)
        {
            if (sounds != null && sounds.Length > 0)
            {
                // проигрывания звука
            }
            if (effects != null && effects.Length > 0)
            {
                foreach (var effect in effects)
                {
                    if (effect != null)
                    {
                        Instantiate(effect, obj.transform.position, obj.transform.rotation, obj.transform);
                    }
                }
            }
        }
        public GameObject CreateObject(Vector3? worldPosition = null, Quaternion? worldRotation = null)
        {
            if (prefab == null)
            {
                // ошибку сюда
                return null; 
            }
            Vector3 spawnPos = worldPosition ?? position;
            Quaternion spawnRot = worldRotation ?? Quaternion.Euler(rotation);

            GameObject newObject;

            newObject = Instantiate(prefab, spawnPos, spawnRot);
            newObject.name = string.IsNullOrEmpty(objectName) ? prefab.name : objectName;
            

            ConfigureObject(newObject);

            PlaySpawnEffects(newObject);

            return newObject;
        }
    }
}