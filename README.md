# WindowsGSM.RSDragonwilds

🧩 A [WindowsGSM](https://windowsgsm.com/) plugin for hosting a **RuneScape: Dragonwilds Dedicated Server**.

## Requirements

- [WindowsGSM](https://github.com/WindowsGSM/WindowsGSM) >= 1.21.0
- A 64-bit Windows host
- Minimum 2GB RAM + ~1GB per player (max 8GB for the 6-player cap)

## Installation

1. Download the latest [release](https://github.com/darkharasho/WindowsGSM.RunescapeDragonwilds/releases) (or clone this repo).
2. Copy the `RSDragonwilds.cs` folder into the `plugins` folder of your WindowsGSM installation.
3. Click `[RELOAD PLUGINS]` in WindowsGSM.
4. The plugin appears under **Add a Game** as `RuneScape: Dragonwilds Dedicated Server`.

## Server details

| Setting | Value |
| --- | --- |
| Steam App ID | `4019830` |
| Start file | `RSDragonwilds.exe` |
| Default game port | `7777/UDP` |
| Max players | `6` (hard cap enforced by the game) |
| Default launch args | `-log` |

## ⚠️ First-time setup (required)

The server **will not start** until you set your `OwnerId`.

1. Install the server in WindowsGSM (this seeds a default config).
2. Open the config file:
   ```
   <server files>\RSDragonwilds\Saved\Config\WindowsServer\DedicatedServer.ini
   ```
3. Fill in the values:
   ```ini
   [/Script/Dominion.DedicatedServerSettings]
   OwnerId=your-player-id      ; REQUIRED - found at the bottom of the in-game Settings menu
   ServerName=My Dragonwilds Server
   DefaultWorldName=My World
   AdminPassword=YourAdminPassword
   WorldPassword=              ; leave empty for a public world
   ```
4. Save the file, then start the server.

> Changing config values **while the server is running** will cause those changes to be lost. Stop the server first.

## Notes

- The game port can be changed in WindowsGSM; the plugin passes it to the server via `-port=`.
- Want a standalone console window instead of the embedded WindowsGSM console? Add `-NewConsole` to the server's launch parameters.
- World saves are stored at `%LocalAppData%\RSDragonwilds\Saved\Savegames`.

## References

- [RuneScape: Dragonwilds Dedicated Servers (Wiki)](https://dragonwilds.runescape.wiki/w/Dedicated_Servers)
- [Official Dedicated Servers How-To Guide](https://dragonwilds.runescape.com/news/how-to-dedicated-servers)
- [WindowsGSM Plugins](https://windowsgsm.com/products/windowsgsm-plugins)
- Plugin structure based on [ohmcodes/WindowsGSM.Palworld](https://github.com/ohmcodes/WindowsGSM.Palworld)

## License

[MIT](LICENSE)
