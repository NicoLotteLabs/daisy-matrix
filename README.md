# Daisy Matrix

A free, independent Windows companion for the classic Desktop Commander MCP console, by **NicoLotteLabs**. Not affiliated with Desktop Commander or the Matrix films.

## English
- Green console output and decorative digital rain after 10 seconds without console changes.
- Returns to the real console when output changes or another window takes focus.
- Escape reveals logs for 30 seconds.
- Local CPU, available RAM, system disk usage, clock and quiet time.
- No network calls, credentials, telemetry or saved console contents. The process samples recent console characters in memory solely to detect changes. The local status file contains state and timestamps only.

### Quick start (Windows)
Requires Windows with .NET Framework 4.x and a classic visible cmd/conhost console. Windows Terminal / ConPTY, services and elevated consoles may not work. No administrator rights normally required. Download the [Windows ZIP](https://github.com/NicoLotteLabs/daisy-matrix/raw/refs/heads/main/DaisyMatrix-v1.0.0-win.zip), extract it, start Desktop Commander, and double-click `Start.cmd` from the extracted folder. If auto-detection fails, run `Start.cmd 1234` with the PID of the intended classic console process. Bring the console to the foreground. Run `Stop.cmd` to stop gracefully. To uninstall, stop it and delete the extracted folder. No startup entry is created.
To compile from source, run `Build.ps1`. PowerShell execution policy may require unblocking downloaded scripts after inspecting them; do not disable system-wide security policies.

## Français
Compagnon visuel indépendant et gratuit pour la console classique de Desktop Commander. Pluie numérique après 10 secondes sans changement de console, retour automatique aux logs, Échap (30 secondes), CPU/RAM/disque réels. Aucun transfert de données ni journalisation du contenu de la console. Compatible uniquement avec les consoles classiques visibles testées sur notre PC Windows ; compatibilité Windows Terminal et autres configurations non garantie.
Téléchargez le ZIP de la release, extrayez-le, lancez Desktop Commander puis double-cliquez sur `Start.cmd`. `Stop.cmd` arrête le module ; supprimez ensuite le dossier pour le désinstaller. Compilation : `Build.ps1`.

## Status
v1.0.0 initial experimental public release. Tested on one Windows setup; broader compatibility testing is welcome. Silence of the console is a visual heuristic, not proof that the agent is idle.

MIT license.

## Full screen / Plein écran
Click FULL SCREEN or press F11 while Matrix is visible. Escape or new console output restores the normal desktop. F11 switches back to window mode.
Cliquez sur FULL SCREEN ou appuyez sur F11 pendant la veille. Échap ou la reprise des logs restitue le bureau normal. F11 revient au mode fenêtré.
Contact: support@nicolottelabs.com


