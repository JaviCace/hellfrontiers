# Bottleneck 01

**What:** The game is GPU-bound: most of the frame time is spent on rendering, not CPU logic.

**Where:** `Gfx.WaitForPresentOnGfxThread` / `Semaphore.WaitForSignal` (CPU waiting for the GPU inside `PostLateUpdate.FinishFrameRendering`)

**Numbers:**

* Main-thread (PlayerLoop): median 18.27 ms, worst frame 53.35 ms
* `Gfx.WaitForPresentOnGfxThread`: median 12.32 ms, maximum 47.79 ms
* `GC.Collect`: yes, 2 times during the capture, up to 1.26 ms
* SetPass Calls: 5
* Total Reserved: 221.8 MB (Textures: 64.5 MB, Meshes: 13.8 KB)
* RenderScaleProbe — full scale: CPU 5 ms / GPU 33 ms
* RenderScaleProbe — scale 0.5: CPU 5 ms / GPU 16 ms

**Verdict:** GPU-bound. When lowering the render scale by half, GPU time drops by 51% (33 ms → 16 ms) while CPU time remains unchanged (5 ms → 5 ms). This rules out the game logic or main thread as the bottleneck.

**Fix to try:** Lower the default Render Scale (or enable a performance mode that reduces it), as this is where the biggest available performance gain can be achieved.
