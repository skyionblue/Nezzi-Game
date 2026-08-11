using UnityEngine;
using Unity.Cinemachine;
using OneWayTogether.Data;
using OneWayTogether.Puzzle;

namespace OneWayTogether.Core
{
    /// <summary>
    /// Reads a LevelData ScriptableObject at Awake and constructs the level by
    /// instantiating 3D platform models and puzzle props. No tilemap is used.
    ///
    /// Platform prefabs must have a BoxCollider2D on the Ground layer so that
    /// characters' Rigidbody2D physics can stand on them.
    ///
    /// Call order:
    ///   Awake → BuildLevel → PlaceCharacters, BuildPlatforms, PlaceObjects, ApplyCamera
    ///
    /// BuildLevel is also public so Editor tooling or integration tests can
    /// invoke it from outside this component.
    /// </summary>
    public class LevelBuilder : MonoBehaviour
    {
        [Header("Level")]
        [SerializeField] private LevelData          _levelData;
        [SerializeField] private LevelPrefabRegistry _prefabs;

        [Header("Characters (scene references)")]
        [SerializeField] private Transform _scarletRoot;
        [SerializeField] private Transform _daniRoot;

        [Header("Hierarchy containers (optional — keeps the scene tidy)")]
        [SerializeField] private Transform _platformContainer;
        [SerializeField] private Transform _objectContainer;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            if (_levelData == null)
            {
                Debug.LogWarning("[LevelBuilder] No LevelData assigned — level will not be built.", this);
                return;
            }

            if (_prefabs == null)
            {
                Debug.LogWarning("[LevelBuilder] No LevelPrefabRegistry assigned — level will not be built.", this);
                return;
            }

            BuildLevel();
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Constructs the level from the assigned LevelData and LevelPrefabRegistry.
        /// Safe to call from Editor scripts before play mode.
        /// </summary>
        public void BuildLevel()
        {
            PlaceCharacters();
            BuildPlatforms();
            PlaceObjects();
            ApplyCamera();
        }

        // ── Private build passes ──────────────────────────────────────────────

        private void PlaceCharacters()
        {
            if (_scarletRoot != null)
                _scarletRoot.position = new Vector3(_levelData.scarletStart.x, _levelData.scarletStart.y, 0f);

            if (_daniRoot != null)
                _daniRoot.position = new Vector3(_levelData.daniStart.x, _levelData.daniStart.y, 0f);
        }

        private void BuildPlatforms()
        {
            foreach (PlatformDef def in _levelData.platforms)
            {
                GameObject prefab = PrefabForPlatform(def.type);
                if (prefab == null)
                {
                    Debug.LogWarning($"[LevelBuilder] No prefab registered for PlatformType.{def.type}. Skipping.", this);
                    continue;
                }

                GameObject go = Instantiate(prefab, def.position, Quaternion.Euler(0f, def.yRotation, 0f), _platformContainer);
                go.transform.localScale = def.scale;
                go.name = $"{def.type}_{def.position}";
            }
        }

        private void PlaceObjects()
        {
            foreach (LevelObjectData def in _levelData.objects)
            {
                Vector3 pos = new Vector3(def.position.x, def.position.y, 0f);

                switch (def.type)
                {
                    case LevelObjectType.Gate:
                        SpawnGate(pos, def.id, def.gateOpenOffset);
                        break;

                    case LevelObjectType.Lever:
                        SpawnLever(pos, def.id, def.oneShot);
                        break;

                    case LevelObjectType.Coin:
                        SpawnCoin(pos);
                        break;

                    case LevelObjectType.ReunionTrigger:
                        SpawnReunionTrigger(pos, def.triggerSize);
                        break;

                    case LevelObjectType.Checkpoint:
                        SpawnCheckpoint(pos);
                        break;

                    case LevelObjectType.RopeTrigger:
                        SpawnRopeTrigger(pos);
                        break;

                    // Scarlet/Dani are handled by PlaceCharacters — skip here.
                    case LevelObjectType.Scarlet:
                    case LevelObjectType.Dani:
                        break;

                    default:
                        Debug.LogWarning($"[LevelBuilder] Unhandled LevelObjectType: {def.type}", this);
                        break;
                }
            }
        }

        private void ApplyCamera()
        {
            UnityEngine.Camera cam = UnityEngine.Camera.main;
            if (cam == null) return;

            cam.backgroundColor = _levelData.skyColor;

            // Set orthographic size on the SinglePlayerCam virtual camera.
            // CoopCam framing is driven by CinemachineTargetGroup bounds, not
            // a fixed ortho size, so we leave it untouched.
            CinemachineCamera[] vcams =
                FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);

            foreach (CinemachineCamera v in vcams)
            {
                if (v.name == "SinglePlayerCam")
                {
                    v.Lens.OrthographicSize = _levelData.orthographicSize;
                    break;
                }
            }
        }

        // ── Spawners ──────────────────────────────────────────────────────────

        private void SpawnGate(Vector3 pos, string id, Vector3 openOffset)
        {
            if (_prefabs.gatePrefab == null)
            {
                Debug.LogWarning("[LevelBuilder] gatePrefab is not assigned in LevelPrefabRegistry.", this);
                return;
            }

            // openOffset defaults to (0, 2.5, 0) when left at Vector3.zero in the data.
            Vector3 effectiveOffset = openOffset == Vector3.zero
                ? new Vector3(0f, 2.5f, 0f)
                : openOffset;

            GameObject go   = Instantiate(_prefabs.gatePrefab, pos, Quaternion.identity, _objectContainer);
            Gate       gate = go.GetComponent<Gate>();
            if (gate != null)
                gate.Init(id, id, effectiveOffset);
        }

        private void SpawnLever(Vector3 pos, string id, bool oneShot)
        {
            if (_prefabs.leverPrefab == null)
            {
                Debug.LogWarning("[LevelBuilder] leverPrefab is not assigned in LevelPrefabRegistry.", this);
                return;
            }

            GameObject go    = Instantiate(_prefabs.leverPrefab, pos, Quaternion.identity, _objectContainer);
            Lever      lever = go.GetComponent<Lever>();
            if (lever != null)
                lever.Init(id, oneShot);
        }

        private void SpawnCoin(Vector3 pos)
        {
            if (_prefabs.coinPrefab == null)
            {
                Debug.LogWarning("[LevelBuilder] coinPrefab is not assigned in LevelPrefabRegistry.", this);
                return;
            }

            GameObject go = Instantiate(_prefabs.coinPrefab, pos, Quaternion.identity, _objectContainer);
            go.transform.localScale = Vector3.one * 0.25f;
        }

        private void SpawnReunionTrigger(Vector3 pos, Vector2 triggerSize)
        {
            if (_prefabs.reunionTriggerPrefab == null)
            {
                Debug.LogWarning("[LevelBuilder] reunionTriggerPrefab is not assigned in LevelPrefabRegistry.", this);
                return;
            }

            GameObject    go  = Instantiate(_prefabs.reunionTriggerPrefab, pos, Quaternion.identity, _objectContainer);
            BoxCollider2D col = go.GetComponent<BoxCollider2D>();
            if (col != null && triggerSize != Vector2.zero)
                col.size = triggerSize;
        }

        private void SpawnCheckpoint(Vector3 pos)
        {
            if (_prefabs.checkpointPrefab == null)
            {
                Debug.LogWarning("[LevelBuilder] checkpointPrefab is not assigned in LevelPrefabRegistry.", this);
                return;
            }

            Instantiate(_prefabs.checkpointPrefab, pos, Quaternion.identity, _objectContainer);
        }

        private void SpawnRopeTrigger(Vector3 pos)
        {
            if (_prefabs.ropeTriggerPrefab == null)
            {
                Debug.LogWarning("[LevelBuilder] ropeTriggerPrefab is not assigned in LevelPrefabRegistry.", this);
                return;
            }

            Instantiate(_prefabs.ropeTriggerPrefab, pos, Quaternion.identity, _objectContainer);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private GameObject PrefabForPlatform(PlatformType t) => t switch
        {
            PlatformType.ForestPlatform => _prefabs.forestPlatformPrefab,
            PlatformType.StoneWall      => _prefabs.stoneWallPrefab,
            PlatformType.BackgroundWall => _prefabs.backgroundWallPrefab,
            _                           => null,
        };
    }
}
