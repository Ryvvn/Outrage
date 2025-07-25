<!-- docs/prd.md -->
# Procedural Tower Defense Product Requirements Document (PRD)

## Goals and Background Context

**Goals**  
- Deliver a fresh, procedurally generated2-D tower-defense run every time, so no two play-throughs feel the same.  
- Provide compelling 15-minute “rogue-lite” sessions that fit mid-core players’ limited time while still offering meaningful meta-progression.  
- Achieve smooth 60 FPS performance on mid-range PCs (RTX 2060/GTX 1660 Super) to ensure a polished Steam experience.  
- Design the core loop (“Day → Build → Night → Survive → Upgrade”) so it is fun and self-contained by the end of each run.  
- Monetize ethically in a free-to-play model via optional cosmetics and convenience unlocks, avoiding pay-to-win mechanics.  
- Build an architecture that supports frequent content drops (new biomes, towers, enemies) without large code rewrites.

**Background Context**  
Procedural Tower Defense targets Steam’s mid-core strategy audience that enjoys quick-hit, replayable games such as Vampire Survivors and They Are Billions. Each 15-minute run feels like a self-contained mini-campaign: gather resources and build towers by day, fend off lethal waves by night, then choose upgrades carrying into future runs. Permanent progression (unlocking new tower blueprints, meta-talents, and cosmetics) supplies long-term goals, while the free-to-play model lowers the barrier to entry.

**Change Log**

| Date       | Version | Description             | Author |
|------------|---------|-------------------------|--------|
| 2025-07-25 | 0.1     | Initial Goals & Context | PM     |

---

## Requirements

### Functional

1. FR1: The game procedurally generates each world “chunk” using a deterministic seed-based Perlin-noise algorithm and path-carving rules managed by `ProceduralChunkGenerator`.  
2. FR2: The system streams world chunks in and out based on player proximity via `WorldStreamer`, ensuring only relevant chunks are loaded.  
3. FR3: During the **Build** phase, the player can place, rotate, and sell towers on valid grid cells; placement updates the pathfinding grid in real time.  
4. FR4: During the **Wave** phase, `WaveManager` spawns enemy waves at procedurally determined points around the base via `PerimeterSpawnManager`.  
5. FR5: Towers auto-target enemies within range, fire projectiles at specified intervals, and apply damage driven by `UpgradeData` and `HealthSystem`.  
6. FR6: Players collect resources during day cycles by interacting with resource nodes, incrementing resources according to node type and player stats.  
7. FR7: At wave end, `ChoiceManager` presents three random upgrade options; upon selection, `TowerManager` and `PlayerBuildTracker` apply stat modifications for subsequent runs.  
8. FR8: The game loop transitions states **Day → Build → Night → WaveInProgress → EndWave** via `GameManager`, enforcing UI prompts and input restrictions.  
9. FR9: The A* `Pathfinder` recalculates paths dynamically on tower placement/removal; if pathing fails, block placement.  
10. FR10: Player controls character via WASD and interacts via action key; `CameraManager` toggles between follow and overview modes.  
11. FR11: Permanent progression unlocks new tower blueprints, talents, or cosmetics stored persistently via `PlayerBuildTracker`.  
12. FR12: The day/night cycle follows a visible timer; day phases shorten build budgets and night phases increase spawn density.  
13. FR13: UI displays phase, time remaining, resources, life count, FPS counter, and active upgrades updated in real time.  
14. FR14: The game supports pausing/resuming, halting timers, spawns, and AI when paused.

### Non-Functional

| Category              | Requirement                                                                                                                                         |
|-----------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------|
| Performance           | Maintain 60 FPS (±5 FPS) at 1920×1080 on mid-range PCs with up to 200 simultaneous entities.                                                        |
| Resource Usage        | Limit GC spikes to <5 ms per frame under stress (1 000+ bullets).                                                                                    |
| Load Times            | World chunk streaming loads/unloads within 200 ms of crossing boundaries.                                                                            |
| Scalability           | Procedural parameters must support adding new biomes, tower types, and enemies without engine changes.                                               |
| Reliability           | Automated stress tests (1 000 waves) pass ≥99% without crash; no game-breaking bugs under normal play.                                               |
| Usability             | UI meets WCAG AA contrast ratios for text and icons.                                                                                                 |
| Mod-Friendliness      | All data (towers, enemies, waves) driven by editable ScriptableObjects or JSON for mod creation.                                                    |
| Localization          | Support English, Spanish, Simplified Chinese via external resource files.                                                                            |
| Platform              | PC only (Windows 10/11), no external dependencies beyond DirectX 11.                                                                                 |
| Security              | Validate JSON schemas before loading mods to prevent code injection.                                                                                |
| Maintainability       | Follow SOLID principles; incremental builds compile <10 ms.                                                                                          |
| Accessibility         | Provide keyboard controls and screen-reader labels for all UI elements.                                                                              |
| Build Size            | Final build ≤1 GB including assets.                                                                                                                 |

---

## User Interface Design Goals

*Not applicable (UI spec handled separately).*

---

## Epic List

1. Epic 1: Foundation & Core Infrastructure — Establish project setup, Git, CI/CD, core services, and initial gameplay scene.  
2. Epic 2: Procedural World & Pathfinding — Implement deterministic chunk generation, dynamic streaming, and A* grid pathfinding.  
3. Epic 3: Combat & Towers — Develop tower prefabs, targeting, shooting, pooling, and enemy behaviors.  
4. Epic 4: Day/Night & Wave System — Create day resource runs, build phase UI, night waves, and upgrade choices.  
5. Epic 5: Meta-Progression & Upgrades — Build permanent unlock systems, talent trees, and cosmetic options.  
6. Epic 6: Polish & Quality Assurance — Integrate UI polish, audio, analytics, and automated stress tests.

---

## Next Steps

- **UX Expert Prompt**  
  *“Using this PRD, draft a high-level UI/UX specification focusing on core screens, interaction paradigms, and accessibility requirements for Procedural Tower Defense.”*

- **Architect Prompt**  
  *“Based on this PRD, design a scalable, modular Unity architecture document covering ScriptableObject data, ECS patterns, pooling, game loop FSM, chunk streaming, pathfinding, and save formats.”*  

---

