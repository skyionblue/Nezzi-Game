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
    /// All positions are in 3D world space. Characters and objects are placed on
    /// the XZ floor plane (Y = 0 by default).
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
                _scarletRoot.position = _levelData.scarletStart;

            if (_daniRoot != null)
                _daniRoot.position = _levelData.daniStart;
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
                // Position is already a full Vector3 — use it directly.
                Vector3    pos = def.position;
                Quaternion rot = Quaternion.Euler(0f, def.yRotation, 0f);

                switch (def.type)
                {
                    case LevelObjectType.Gate:
                        SpawnGate(pos, rot, def.id, def.gateOpenOffset);
                        break;

                    case LevelObjectType.Lever:
                        SpawnLever(pos, rot, def.id, def.oneShot);
                        break;

                    case LevelObjectType.Coin:
                        SpawnCoin(pos);
                        break;

                    case LevelObjectType.ReunionTrigger:
                        SpawnReunionTrigger(pos, rot, def.triggerSize);
                        break;

                    case LevelObjectType.Checkpoint:
                        SpawnCheckpoint(pos, rot);
                        break;

                    case LevelObjectType.RopeTrigger:
                        SpawnRopeTrigger(pos, rot);
                        break;

                    case LevelObjectType.Bridge:
                        SpawnBridge(pos, rot, def.id);
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

            // Find the SinglePlayerCam and push the field of view.
            // Orthographic size is ignored — camera is now perspective.
            CinemachineCamera[] vcams =
                FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Exclude);

            foreach (CinemachineCamera v in vcams)
            {
                if (v.name == "SinglePlayerCam")
                {
                    v.Lens.FieldOfView = 40f;
                    break;
                }
            }
        }

        // ── Spawners ──────────────────────────────────────────────────────────

        private void SpawnGate(Vector3 pos, Quaternion rot, string id, Vector3 openOffset)
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

            GameObject go   = Instantiate(_prefabs.gatePrefab, pos, rot, _objectContainer);
            Gate       gate = go.GetComponent<Gate>();
            if (gate != null)
                gate.Init(id, id, effectiveOffset);
        }

        private void SpawnLever(Vector3 pos, Quaternion rot, string id, bool oneShot)
        {
            if (_prefabs.leverPrefab == null)
            {
                Debug.LogWarning("[LevelBuilder] leverPrefab is not assigned in LevelPrefabRegistry.", this);
                return;
            }

            GameObject go    = Instantiate(_prefabs.leverPrefab, pos, rot, _objectContainer);
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

        private void SpawnReunionTrigger(Vector3 pos, Quaternion rot, Vector2 triggerSize)
        {
            if (_prefabs.reunionTriggerPrefab == null)
            {
                Debug.LogWarning("[LevelBuilder] reunionTriggerPrefab is not assigned in LevelPrefabRegistry.", this);
                return;
            }

            GameObject go = Instantiate(_prefabs.reunionTriggerPrefab, pos, rot, _objectContainer);

            // Try 3D BoxCollider first (isometric), fall back to BoxCollider2D for legacy prefabs.
            if (triggerSize != Vector2.zero)
            {
                BoxCollider col3d = go.GetComponent<BoxCollider>();
                if (col3d != null)
                    col3d.size = new Vector3(triggerSize.x, 2f, triggerSize.y);
                else
                {
                    BoxCollider2D col2d = go.GetComponent<BoxCollider2D>();
                    if (col2d != null)
                        col2d.size = triggerSize;
                }
            }
        }

        private void SpawnCheckpoint(Vector3 pos, Quaternion rot)
        {
            if (_prefabs.checkpointPrefab == null)
            {
                Debug.LogWarning("[LevelBuilder] checkpointPrefab is not assigned in LevelPrefabRegistry.", this);
                return;
            }

            Instantiate(_prefabs.checkpointPrefab, pos, rot, _objectContainer);
        }

        private void SpawnRopeTrigger(Vector3 pos, Quaternion rot)
        {
            if (_prefabs.ropeTriggerPrefab == null)
            {
                Debug.LogWarning("[LevelBuilder] ropeTriggerPrefab is not assigned in LevelPrefabRegistry.", this);
                return;
            }

            Instantiate(_prefabs.ropeTriggerPrefab, pos, rot, _objectContainer);
        }

        private void SpawnBridge(Vector3 pos, Quaternion rot, string plateId)
        {
            if (_prefabs.bridgePrefab == null)
            {
                Debug.LogWarning("[LevelBuilder] bridgePrefab is not assigned in LevelPrefabRegistry.", this);
                return;
            }

            GameObject go     = Instantiate(_prefabs.bridgePrefab, pos, rot, _objectContainer);
            Bridge     bridge = go.GetComponent<Bridge>();
            if (bridge != null)
                bridge.Init(plateId);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private GameObject PrefabForPlatform(PlatformType t) => t switch
        {
            PlatformType.ForestPlatform        => _prefabs.forestPlatformPrefab,
            PlatformType.StoneWall             => _prefabs.stoneWallPrefab,
            PlatformType.BackgroundWall        => _prefabs.backgroundWallPrefab,

            PlatformType.RPGFloorCovered       => _prefabs.rpgFloorCoveredPrefab,
            PlatformType.RPGFloorDirt          => _prefabs.rpgFloorDirtPrefab,
            PlatformType.RPGFloorStone         => _prefabs.rpgFloorStonePrefab,
            PlatformType.RPGFloorWater         => _prefabs.rpgFloorWaterPrefab,
            PlatformType.RPGRampCovered        => _prefabs.rpgRampCoveredPrefab,

            PlatformType.VegetationBushLarge   => _prefabs.vegetationBushLargePrefab,
            PlatformType.VegetationBushSmall   => _prefabs.vegetationBushSmallPrefab,
            PlatformType.VegetationFern        => _prefabs.vegetationFernPrefab,

            PlatformType.RockBoulder           => _prefabs.rockBoulderPrefab,
            PlatformType.RockChunks            => _prefabs.rockChunksPrefab,

            PlatformType.AncientRuinsArch      => _prefabs.ancientRuinsArchPrefab,
            PlatformType.AncientRuinsColumn    => _prefabs.ancientRuinsColumnPrefab,
            PlatformType.AncientRuinsWall      => _prefabs.ancientRuinsWallPrefab,

            _                                  => null,
        };
    }
}
