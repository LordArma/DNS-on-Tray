# CLAUDE.md

Guidance for Claude Code (and humans) working in this repository. Keep the **Roadmap & Bug Tracker** section up to date: tick items when done and add new findings as they come up.

## Project

**DNS on Tray** is a Windows system-tray app for switching the machine's DNS servers with one click.

- Upstream: https://github.com/LordArma/DNS-on-Tray (the old name `DNS-on-Try` redirects; `origin` now points at the new URL)
- Stack: C# / WinForms, `net8.0-windows`, self-contained single-file publish
- Storage: SQLite (`Microsoft.Data.Sqlite`) at `%LOCALAPPDATA%\dnsontry.db`, table `dnsTable(dnsName PK, dns1, dns2)`
- Version: `<Version>` in `DNS on Tray/DNS on Tray.csproj`
- Windows-only. It can't be built or run from WSL/Linux unless a Windows .NET SDK is available.

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
| `DNS on Tray/Program.cs` | Entry point; single-instance mutex (`systemontray123`) |
| `DNS on Tray/Form1.cs` | Main window + tray menu logic (list, add/remove/set, ping, startup toggle) |
| `DNS on Tray/Form1.Designer.cs` | WinForms designer layout (borderless form, `notifyIcon1`, `notifyMenu`) |
| `DNS on Tray/DNSChanger.cs` | `DNS` class: SQLite persistence (`All`, `Save`, `Remove`, `Exist`) |
| `DNS on Tray/Helper.cs` | Apply/clear DNS (elevated `cmd` + `wmic`), default DNS seed list, Run-at-startup registry key (`HKCU\...\Run\dnsontry`), UAC shield helper |
| `DNS on Tray/Models/DnsPingResult.cs` | Ping result record for the health check |
| `DNS on Tray/Resources.resx` | Icons (`dns`, `clear`, `exit`) |

### How things work
- **Window show/hide** toggles `Opacity`, `Visible` and `ShowInTaskbar` (`ShowMainWindow`). The form sits in the bottom-right of the working area.
- **Tray menu** is rebuilt by `MakeMenuItems()` after every add or remove. Clicks are routed by the menu item's **Text** in `MenuItemClickHandler`.
- **Applying DNS** runs `cmd.exe /C wmic nicconfig where (IPEnabled=TRUE) call SetDNSServerSearchOrder(...)` with `Verb=runas` (UAC on every change, fire-and-forget).
- **Default servers** are seeded only when the DB file is first created (`Helper.AddPopularDNS`).

## Conventions
- Match the existing style: namespace `DNS_on_Tray`, `using static DNS_on_Tray.Helper;`, `str`/`btn`/`txt`/`lst` prefixes for locals and controls.
- Edit UI layout through the designer file consistently; don't hand-edit `InitializeComponent` in ways the designer can't round-trip.
- Nullable is enabled; avoid adding more `#pragma warning disable` blocks.

## Roadmap & Bug Tracker

_Last analysed: 2026-09-24 (commit `87ebf58`)._

### Phase 1: Critical bugs
- [ ] **wmic is deprecated/removed** (disabled by default on Win11 24H2+), so DNS changes silently fail. Replace it with `Set-DnsClientServerAddress` / `netsh interface ip set dns`, wait for the exit code, then flush DNS. (`Helper.AddDNS/ClearDNS`)
- [ ] **Command injection**: DNS1/DNS2 aren't validated and are interpolated into an elevated `cmd` line. Validate with `IPAddress.TryParse` and never build a shell string from user input.
- [ ] **SQL injection / crash on `'` in names**: the constructor, `Exist` and `Remove` interpolate `dnsname`. Use parameters. (`DNSChanger.cs`)
- [ ] **Set with no selection** applies the fallback `8.8.8.8/4.2.2.4`, and **Ping with no selection** pings those defaults. Guard both. (`btnDNSSet_Click`, `btnDNSPing_Click`)
- [ ] **Ping on the "Clear" entry clears DNS.** Remove that side effect.
- [ ] **No success/failure feedback** after apply; a cancelled UAC prompt is swallowed silently. (`RunCMDAsAdmin`)
- [ ] **Duplicate names** get added to the list twice. Custom names "Exit"/"Settings"/"Clear" collide with built-in menu actions. Route by `Tag`, not `Text`, and reject reserved or duplicate names.
- [ ] **All IPEnabled adapters** are changed, including VPN and virtual ones. Target a chosen or active adapter.
- [ ] **`MakeDB` crash** if the DB file exists but has no table. Always run `CREATE TABLE IF NOT EXISTS` and catch DB errors.

### Phase 2: Usability
- [ ] Show the **currently active DNS** (checkmark in the tray menu and tooltip). Refresh on `NetworkChange` events.
- [ ] Adapter picker (default: the adapter with the internet route)
- [ ] Real **DNS health check** (UDP/53 query latency) instead of ICMP ping; report each server; "Test all" sorted by latency
- [ ] Edit existing entries; allow a single DNS server (DNS2 optional)
- [ ] Tray balloon notification after apply
- [ ] Window UX: hide on deactivate, close/hide button, Alt+F4 hides instead of exiting, single left-click on the tray icon opens the window
- [ ] A second launch focuses the running instance instead of exiting silently
- [ ] Avoid UAC on every switch (elevate once, or use an elevated helper/service)

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
- [ ] Cleanup: `Opacity = 100` should be `1.0`; quote the exe path in the Run key; remove `new Random()` per menu item, the dead `frmMain_Activated` code and unused usings; put `DnsPingResult` in the `DNS_on_Tray` namespace

### Done
- [x] Sync local repo with upstream `LordArma/DNS-on-Tray` (fast-forward to `87ebf58`, remote URL updated), 2026-09-24
- [x] DNS health-check (ping) button, upstream PR #1/#2
