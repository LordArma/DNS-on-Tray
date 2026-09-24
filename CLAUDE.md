# CLAUDE.md

Guidance for Claude Code (and humans) working in this repository. Keep the **Roadmap & Bug Tracker** section up to date: tick items when done and add new findings as they come up.

## Project

**DNS on Tray** is a Windows system-tray app for switching the machine's DNS servers with one click.

- Upstream: https://github.com/LordArma/DNS-on-Tray (the old name `DNS-on-Try` redirects; `origin` now points at the new URL)
- Stack: C# / WinForms, `net8.0-windows`, self-contained single-file publish
- Storage: SQLite (`Microsoft.Data.Sqlite`) at `%LOCALAPPDATA%\dnsontry.db`, table `dnsTable(dnsName PK, dns1, dns2)`
- Version: `<Version>` in `DNS on Tray/DNS on Tray.csproj`
- Windows-only. From WSL, use the Windows SDK: `"/mnt/c/Program Files/dotnet/dotnet.exe" build ...` (SDK 10.x is installed). If the app is running it locks `bin\Debug`, so build with `-o "obj\\verify"` instead.
- WSL git has no identity configured; commit as the Windows git identity (`git -c user.name=... -c user.email=...`, see `git.exe config user.name`).

## Build & run (Windows)

```powershell
dotnet restore
dotnet build
dotnet run --project "DNS on Tray"
dotnet publish "DNS on Tray" -c Release -r win-x64   # single-file, self-contained exe
```

CI: `.github/workflows/dotnet.yml` (windows-latest, restore + Debug build, uploads `bin\Debug`).

## Code map

| File | Role |
|---|---|
| `DNS on Tray/Program.cs` | Entry point; relaunch through the elevated task when it exists; single-instance mutex (`systemontray123`), and a second launch signals the event `DNSonTray.ShowWindow` to show the running window |
| `DNS on Tray/Form1.cs` | Main window + tray menu logic: server list (`ServerItem` rows with a ✓ for the current DNS and latency), adapter picker, current-DNS display, test/test all, add/edit/remove/set, startup and admin-mode toggles |
| `DNS on Tray/Form1.Designer.cs` | WinForms designer layout (borderless form, `notifyIcon1`, `notifyMenu`) |
| `DNS on Tray/DNSChanger.cs` | `DNS` class: SQLite persistence (`All`, `Find`, `NameTaken`, `Save`, `Remove`, `Exist`), all queries parameterized |
| `DNS on Tray/Helper.cs` | Apply/clear DNS (elevated PowerShell, returns `DnsChangeResult`), `IsValidIPv4`, adapter list and saved adapter choice (`HKCU\SOFTWARE\DNS on Tray\Adapter`), `GetCurrentDNS` (manual vs DHCP from the adapter's `NameServer` registry value), the elevated scheduled task, default DNS seed list, Run-at-startup registry key (`HKCU\...\Run\dnsontry`), UAC shield helper |
| `DNS on Tray/DnsProbe.cs` | Health check: real DNS query (UDP/53, `google.com` A record, 2 s timeout); `IsIntercepted` detects VPN/proxy DNS hijacking by querying the reserved 192.0.2.1 |
| `DNS on Tray/Resources.resx` | Icons (`dns`, `clear`, `exit`) |

### How things work
- **Window show/hide**: `ShowMainWindow`/`HideMainWindow` set `Opacity`, `Visible` and `ShowInTaskbar`. The form sits in the bottom-right of the working area. A left click on the tray icon toggles it. It hides on deactivate (the `lastAutoHide` 500 ms guard stops the same tray click from reopening it), and on ✕, Esc, Alt+F4 or double-click. Only Exit in the tray menu quits.
- **Current DNS** refreshes on `NetworkChange` events, when the tray menu opens, when the window is shown, and after each apply. `RefreshCurrentDNS` rebuilds both lists via `MakeMenuItems`.
- **Administrator mode** (`optAdmin`): registers the scheduled task "DNS on Tray" (RunLevel Highest, action `exe --from-task`, optional AtLogOn trigger that replaces the HKCU Run key) with one UAC prompt. Non-elevated launches then run `schtasks /Run` and exit, so the app runs elevated and DNS changes don't prompt. `--from-task` prevents a relaunch loop for standard users.
- **Tray menu** is rebuilt by `MakeMenuItems()` after every add or remove. DNS items carry their `DNS` object in `Tag` (`DnsMenuItem_Click`); Clear/Settings/Exit have their own handlers. The list box shows a pseudo-entry `ClearItem` ("Clear"), which is a reserved name.
- **Applying DNS** (`Helper.AddDNS/ClearDNS`, async) builds a PowerShell script (`Set-DnsClientServerAddress` / `-ResetServerAddresses`, then `Clear-DnsClientCache`) for the adapters from `ActiveInterfaceIndexes()` (Up, not loopback/tunnel, with an IPv4 default gateway). It runs via `-EncodedCommand` with `Verb=runas` (UAC on every change), waits for the exit code, and the form shows the result as a tray balloon (`ReportResult`). Addresses must pass `IsValidIPv4` (strict dotted quad) before they reach the script.
- **Database init** (`MakeDB`) runs once per process: it creates the table if missing (seeding defaults on creation). A corrupt file is renamed to `dnsontry.db.<timestamp>.bak` and recreated.
- **Default servers** are seeded only when the DB file is first created (`Helper.AddPopularDNS`).

## Conventions
- Match the existing style: namespace `DNS_on_Tray`, `using static DNS_on_Tray.Helper;`, `str`/`btn`/`txt`/`lst` prefixes for locals and controls.
- Edit UI layout through the designer file consistently; don't hand-edit `InitializeComponent` in ways the designer can't round-trip.
- Nullable is enabled; avoid adding more `#pragma warning disable` blocks.

## Roadmap & Bug Tracker

_Last analysed: 2026-09-24 (commit `87ebf58`)._

### Phase 1: Critical bugs (done 2026-09-24; manually verified on Windows 11 by the user)
- [x] **wmic is deprecated/removed** (disabled by default on Win11 24H2+), so DNS changes silently fail. Replace it with `Set-DnsClientServerAddress` / `netsh interface ip set dns`, wait for the exit code, then flush DNS. (`Helper.AddDNS/ClearDNS`)
- [x] **Command injection**: DNS1/DNS2 aren't validated and are interpolated into an elevated `cmd` line. Validate with `IPAddress.TryParse` and never build a shell string from user input.
- [x] **SQL injection / crash on `'` in names**: the constructor, `Exist` and `Remove` interpolate `dnsname`. Use parameters. (`DNSChanger.cs`)
- [x] **Set with no selection** applies the fallback `8.8.8.8/4.2.2.4`, and **Ping with no selection** pings those defaults. Guard both. (`btnDNSSet_Click`, `btnDNSPing_Click`)
- [x] **Ping on the "Clear" entry clears DNS.** Remove that side effect.
- [x] **No success/failure feedback** after apply; a cancelled UAC prompt is swallowed silently. (`RunCMDAsAdmin`)
- [x] **Duplicate names** get added to the list twice. Custom names "Exit"/"Settings"/"Clear" collide with built-in menu actions. Route by `Tag`, not `Text`, and reject reserved or duplicate names.
- [x] **All IPEnabled adapters** are changed, including VPN and virtual ones. Now only adapters with an IPv4 default gateway are changed; a picker is still in Phase 2.
- [x] **`MakeDB` crash** if the DB file exists but has no table. Always run `CREATE TABLE IF NOT EXISTS` and catch DB errors.

### Phase 2: Usability (done 2026-09-24; needs a manual test on Windows, especially admin mode)
- [x] Show the **currently active DNS** (checkmark in the tray menu and tooltip). Refresh on `NetworkChange` events.
- [x] Adapter picker (default: the adapter with the internet route)
- [x] Real **DNS health check** (UDP/53 query latency) instead of ICMP ping; report each server; "Test all" sorted by latency
- [x] Edit existing entries; allow a single DNS server (DNS2 optional)
- [x] Tray balloon notification after apply
- [x] Window UX: hide on deactivate, close/hide button, Alt+F4 hides instead of exiting, single left-click on the tray icon opens the window
- [x] A second launch focuses the running instance instead of exiting silently
- [x] Avoid UAC on every switch (elevate once, or use an elevated helper/service)

### Phase 3: Features (README to-dos)
- [ ] IPv6 support
- [ ] DNS-over-HTTPS (Win11 `netsh dns add encryption`)
- [ ] Import/export server list
- [ ] Global hotkey
- [ ] Better tray icon / logo; icon reflects state
- [ ] Farsi / Gilaki UI (resource-based localization)
- [ ] Test on Windows 10
- [ ] Package & sign releases

### Phase 4: Code health & release
- [ ] Upgrade to .NET 10 LTS (.NET 8 support ends Nov 2026)
- [ ] Consider replacing SQLite with a JSON file (drops the native dependency); migrate the existing `dnsontry.db`
- [ ] Split into storage / DNS service / UI layers; add a unit test project (validation, storage)
- [ ] CI: `dotnet publish -c Release`, attach the exe to GitHub Releases on tags, and drop the hard-coded `D:\a\...` artifact path
- [ ] README: add OpenDNS to the defaults list; refresh screenshots and the to-do list
- [x] Cleanup: `Opacity = 100` should be `1.0`; quote the exe path in the Run key; remove `new Random()` per menu item, the dead `frmMain_Activated` code and unused usings; put `DnsPingResult` in the `DNS_on_Tray` namespace (done in Phase 2; `DnsPingResult` was removed)

### Known limitations
- With a VPN/proxy that intercepts DNS (e.g. the user's `xray_tun`), every server "answers" instantly. The health check detects this and shows a warning instead of results.
- The "Run as administrator" task stores the exe path; moving the exe requires toggling the option off and on again.

### Done
- [x] Sync local repo with upstream `LordArma/DNS-on-Tray` (fast-forward to `87ebf58`, remote URL updated), 2026-09-24
- [x] DNS health-check (ping) button, upstream PR #1/#2
