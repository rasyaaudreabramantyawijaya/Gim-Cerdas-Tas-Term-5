# Independent review and regression evidence — 21 October 2026

Fresh-context read-only review by Carson. No reviewer mutations or test runs. Verdict: needs fixes. Root causes confirmed in source, then reproduced by the implementer's real Unity integration checks.

| Finding | Reproduced red check | Targeted fix |
|---|---|---|
| Editor verification writes real save and could be interrupted by reload | editor verification uses isolated save slot | Serialized configurable filename, verifier sets isolated filename before new game; backup scope also isolated |
| Losing sight allows immediate teleport despite combat memory | fresh combat memory remains engaged behind cover | Investigate is engaged while recent memory is active |
| Guardian takes damage before intro | Guardian invulnerable until introductory dialogue | Damage rejects IntroBlocked |
| Completed Guardian respawns and pays again | completed Guardian does not respawn for duplicate rewards | Saved guardian completion prevents respawning Sanctum boss |
| Same attack animation does not restart | repeated attack restarts actual animation | Explicit restart CrossFade at time zero for action requests |
| Pine trunks omitted from NavMesh | navigation excludes solid pine trunk footprint | Include layer-0 capsule geometry in NavMesh sources |
| Weapon/projectile runtime material allocations retained | weapon swap releases obsolete native material | Explicit native resource owner and per-effect teardown |

Evidence before fixes: review-regressions-red.txt — 49 passing checks, 7 failures. Green verification after fixes must be read from the latest play-checks.txt; this document does not substitute for that run.

Review did not judge subjective native typography, audio quality or standalone shader stripping. Those need direct captures/player-build checks. Tower-based respawn is the current checkpoint policy, and transform-driven hair/capes are explicitly not a cloth-physics claim.
