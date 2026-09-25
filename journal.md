# Journal

## Week 2, Lab B — Lifecycle, back, accessibility and scope lock

### Lifecycle matrix (Part A)

| Test                                      | Expected                                            | Result   |
| ----------------------------------------- | --------------------------------------------------- | -------- |
| Press Home, wait 10 s, return             | Paused, panel visible, audio silent, progress saved | Achieved |
| Pull the notification shade down and up   | Paused                                              | Achieved |
| Neighbour calls you, you hang up          | Paused, game resumes only on Resume                 | Achieved |
| Screen off with the power button, back on | Paused                                              | Achieved |
| Force stop from Settings, relaunch        | Progress restored from the save                     | Achieved |

---

## Week 3, Lab A — Rendering budgets and profiling on device

### Part B — CPU capture of worst case

* **Worst-frame main-thread ms:** 18.27 ms (median), 53.35 ms (worst frame)
* **Tallest marker:** `PostLateUpdate.FinishFrameRendering` (15.01 ms median) → inside it, `Gfx.WaitForPresentOnGfxThread` (12.32 ms) → `Semaphore.WaitForSignal` (12.32 ms)
* **Top 3 hierarchy entries (self time):**

  1. `PostLateUpdate.FinishFrameRendering` — 15.01 ms
  2. `Gfx.WaitForPresentOnGfxThread` — 12.32 ms
  3. `Semaphore.WaitForSignal` — 12.32 ms
* **GC.Collect:** Yes, it appeared 2 times during the capture, with a duration of up to 1.26 ms

### Part C — Rendering and Memory modules

* **SetPass Calls:** 5
* **Total Reserved:** 221.8 MB
* **Textures:** 64.5 MB
* **Meshes:** 13.8 KB

### Part D — RenderScaleProbe verdict

|                  | CPU  | GPU   |
| ---------------- | ---- | ----- |
| Full scale       | 5 ms | 33 ms |
| Half scale (0.5) | 5 ms | 16 ms |

* **Verdict:** GPU-bound (GPU time decreases by 51% when reducing the scale, while CPU time remains unchanged)
* **Candidate fix:** Lower the default Render Scale / reduce transparent layers

---

## Week 3, Lab B — Frame pacing and your performance baseline

> [PENDING — this lab has not been completed yet]

### Part C — GC allocations per frame (to be filled in)

* GC Allocated in Frame (steady state):
* Markers that allocate memory:
* Seconds between GC.Collect (if present):

### Throttling observed (to be filled in)

* Do the numbers increase over the course of the session?:
