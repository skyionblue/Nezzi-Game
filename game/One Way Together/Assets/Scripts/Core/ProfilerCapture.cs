using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace OneWayTogether.Core
{
    /// <summary>
    /// Attach to any active GameObject in CityWorld.
    /// Captures 300 frames of ProfilerRecorder data and logs a full report.
    /// Remove after profiling is done.
    /// </summary>
    public class ProfilerCapture : MonoBehaviour
    {
        [SerializeField] private int _captureFrames = 300;

        private ProfilerRecorder _mainThreadTime;
        private ProfilerRecorder _renderThreadTime;
        private ProfilerRecorder _gpuTime;
        private ProfilerRecorder _drawCalls;
        private ProfilerRecorder _setPassCalls;
        private ProfilerRecorder _triangles;
        private ProfilerRecorder _gcAlloc;
        private ProfilerRecorder _totalMemory;

        private readonly List<double> _frameSamples = new();

        private void OnEnable()
        {
            _mainThreadTime  = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 300);
            _renderThreadTime= ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Render Thread", 300);
            _gpuTime         = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "GPU Frame Time", 300);
            _drawCalls       = ProfilerRecorder.StartNew(ProfilerCategory.Render,   "Draw Calls Count", 1);
            _setPassCalls    = ProfilerRecorder.StartNew(ProfilerCategory.Render,   "SetPass Calls Count", 1);
            _triangles       = ProfilerRecorder.StartNew(ProfilerCategory.Render,   "Triangles Count", 1);
            _gcAlloc         = ProfilerRecorder.StartNew(ProfilerCategory.Memory,   "GC Allocated In Frame", 1);
            _totalMemory     = ProfilerRecorder.StartNew(ProfilerCategory.Memory,   "Total Used Memory", 1);
        }

        private void OnDisable()
        {
            _mainThreadTime.Dispose();
            _renderThreadTime.Dispose();
            _gpuTime.Dispose();
            _drawCalls.Dispose();
            _setPassCalls.Dispose();
            _triangles.Dispose();
            _gcAlloc.Dispose();
            _totalMemory.Dispose();
        }

        private void Start() => StartCoroutine(Capture());

        private IEnumerator Capture()
        {
            // Wait for scene to fully initialise
            yield return new WaitForSeconds(3f);

            Debug.Log("[Profiler] Starting 300-frame capture...");
            _frameSamples.Clear();

            for (int i = 0; i < _captureFrames; i++)
            {
                yield return null;
                if (_mainThreadTime.Valid && _mainThreadTime.LastValue > 0)
                    _frameSamples.Add(_mainThreadTime.LastValue * 1e-6); // ns → ms
            }

            PrintReport();
        }

        private void PrintReport()
        {
            if (_frameSamples.Count == 0) { Debug.LogError("[Profiler] No samples captured."); return; }

            _frameSamples.Sort();
            double avg    = 0; foreach (var s in _frameSamples) avg += s; avg /= _frameSamples.Count;
            double p95    = _frameSamples[(int)(_frameSamples.Count * 0.95f)];
            double p99    = _frameSamples[(int)(_frameSamples.Count * 0.99f)];
            double worst  = _frameSamples[_frameSamples.Count - 1];
            double best   = _frameSamples[0];

            long gc     = _gcAlloc.Valid     ? _gcAlloc.LastValue      : -1;
            long dc     = _drawCalls.Valid   ? _drawCalls.LastValue    : -1;
            long sp     = _setPassCalls.Valid? _setPassCalls.LastValue : -1;
            long tris   = _triangles.Valid   ? _triangles.LastValue    : -1;
            long mem    = _totalMemory.Valid  ? _totalMemory.LastValue / (1024*1024) : -1;
            double gpu  = _gpuTime.Valid && _gpuTime.LastValue > 0 ? _gpuTime.LastValue * 1e-6 : -1;

            string report = $@"
╔══════════════════════════════════════════════════════╗
║   PROFILER BASELINE — One Way Together (Editor)      ║
║   {_captureFrames} frames captured after 3s warmup               ║
╠══════════════════════════════════════════════════════╣
║  FRAME TIME (Main Thread)                            ║
║    Average:          {avg,7:F2} ms   (target < 16.6ms)  ║
║    95th percentile:  {p95,7:F2} ms   (target < 16.6ms)  ║
║    99th percentile:  {p99,7:F2} ms                       ║
║    Worst frame:      {worst,7:F2} ms   (target < 33ms)   ║
║    Best frame:       {best,7:F2} ms                       ║
╠══════════════════════════════════════════════════════╣
║  GPU                                                 ║
║    GPU frame time:   {(gpu >= 0 ? gpu.ToString("F2") + " ms" : "N/A    "),10}                     ║
╠══════════════════════════════════════════════════════╣
║  RENDERING                                           ║
║    Draw calls:       {(dc >= 0 ? dc.ToString() : "N/A"),10}   (target < 100)       ║
║    SetPass calls:    {(sp >= 0 ? sp.ToString() : "N/A"),10}   (target < 50)        ║
║    Triangles:        {(tris >= 0 ? (tris/1000).ToString() + "k" : "N/A"),10}                     ║
╠══════════════════════════════════════════════════════╣
║  MEMORY                                              ║
║    Total used:       {(mem >= 0 ? mem.ToString() + " MB" : "N/A    "),10}   (target < 400MB)     ║
║    GC alloc/frame:   {(gc >= 0 ? gc.ToString() + " B" : "N/A    "),10}   (target = 0)          ║
╚══════════════════════════════════════════════════════╝

NOTE: Editor adds ~2-3× overhead vs device. Divide ms values by 2-3 for
      a rough device estimate. Draw calls and memory are accurate.";

            Debug.Log(report);
            Debug.Log("[Profiler] Done. Remove ProfilerCapture component when finished.");
        }
    }
}
