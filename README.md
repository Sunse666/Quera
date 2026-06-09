# Quera

<p align="center">
  <strong>Keyboard-driven launcher for Windows</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Platform-Windows-blue?style=flat-square">
  <img src="https://img.shields.io/badge/Framework-.NET%208%20WPF-purple?style=flat-square">
  <img src="https://img.shields.io/badge/License-MIT-green?style=flat-square">
</p>

## Introduction

Quera is a keyboard-driven launcher for Windows. Press `Alt + Space` to bring up the search window, type a query, and instantly find apps, files, commands, bookmarks — plus a built-in calculator, system commands, input prefixes, and more. Built with .NET 8 WPF, zero WebView dependency.

## Features

- **Global Hotkey** — `Alt + Space` toggle with fade animation
- **File Search** — Indexes Start Menu and custom directories
- **Custom Commands** — `shell:` / `cmd:` / `ps:` / `run:` with admin elevation
- **Bookmarks** — Keyword-triggered URLs
- **Search Engines** — `keyword query` syntax
- **Folder Shortcuts** — Keyword to open a directory
- **Calculator** — Type `2+2*3` and see `= 8`
- **System Commands** — `shutdown` / `restart` / `lock` / `sleep`
- **Input Prefixes** — `>` commands only, `/` files only, `@` bookmarks only
- **Aliases** — `reboot` → `shutdown /r /t 0`
- **History** — Frequently used results rank higher
- **Paginated Results** — `Tab` / `Shift+Tab` to flip pages
- **Auto-hide** — Disappears when focus is lost
- **System Tray** — Lives in the tray with right-click menu
- **Highly Configurable** — 70+ YAML settings: colors, fonts, layout, shortcuts, priority
- **Image Icons** — Supports emoji and ico/png/jpg files

## Shortcuts

| Key | Action |
|---|---|
| `Alt + Space` | Show / Hide |
| `↑` `↓` | Navigate |
| `Enter` | Execute |
| `Tab` | Next page (auto-complete on search hint) |
| `Shift + Tab` | Previous page |
| `Esc` | Hide |
| `Ctrl + ,` | Open config |
| `Ctrl + R` | Reload config |

## Quick Start

1. Download and extract
2. Run `Quera.exe`
3. Press `Alt + Space`
4. Edit `config.yaml`

## Configuration

```yaml
settings:
  hotkey: Alt+Space
  width: 680
  opacity: 96
  max_results: 30
  autostart: false
  hide_on_deactivate: true
  hide_delay_ms: 200
  show_on_startup: false

window:
  always_on_top: true
  corner_radius: 20

icons:
  enable_image_icons: true
  cache_icons: true

colors:
  background: "#B216213E"
  search_card: "#0DFFFFFF"
  search_border: "#12FFFFFF"
  result_card: "#CC1A1A2E"
  result_border: "#33FFFFFF"
  result_hover: "#18FFFFFF"
  result_selected_start: "#D97035"
  result_selected_end: "#E8955A"
  text_primary: "#FFFFFF"
  text_secondary: "#8899AA"
  text_muted: "#556677"
  accent: "#D97035"

search_box:
  placeholder: "Search apps, files, commands..."
  icon: "🔍"
  esc_hint: "ESC to close"

results:
  padding_h: 10
  padding_v: 7
  margin: 1
  icon_size: 20
  badge_font_size: 9

layout:
  outer_margin: 12
  card_gap: 10
  search_padding: 14
  results_padding: 6

shortcuts:
  next_page: Tab
  prev_page: Shift+Tab
  execute: Enter
  hide: Escape
  open_config: "Ctrl+,"
  reload_config: Ctrl+R

search:
  match_mode: contains
  include_directories: false
  max_depth: -1

exclude:
  paths: [~/AppData, ~/.git]
  patterns: ["*.tmp", "*.log"]

cache:
  enabled: true
  refresh_on_start: true
  max_files: 50000

ui:
  border_radius: 20
  font_family: "Microsoft YaHei UI"
  font_size_search: 17
  font_size_result_name: 14
  font_size_result_desc: 11
  max_visible_items: 10
  item_height: 44
  show_icons: true
  show_type_badge: true
  show_status_bar: true

terminal:
  default: cmd
  admin_default: powershell

paths:
  - ~/Desktop
  - C:/Tools

file_types:
  - .exe
  - .lnk
  - .bat
  - .ps1

commands:
  - keyword: cmd
    name: Command Prompt
    action: run:cmd.exe
    icon: "⚡"
    admin: false

bookmarks:
  - keyword: gh
    name: GitHub
    url: https://github.com
    icon: "🔗"

folders:
  - keyword: desk
    name: Desktop
    path: ~/Desktop
    icon: "📁"

search_engines:
  - keyword: g
    name: Google
    url: https://www.google.com/search?q={query}
    icon: "🔍"

aliases:
  - keyword: reboot
    action: cmd:shutdown /r /t 0
  - keyword: logoff
    action: cmd:shutdown /l

priority:
  types: [command, bookmark, file, folder, search, search_hint]
  extensions: [.exe, .lnk, .bat, .ps1]
  custom_path_first: true
```

## FAQ

**Q: Hotkey conflict?** Change `settings.hotkey` — supports Ctrl / Alt / Shift / Win.

**Q: Config not taking effect?** Press `Ctrl + R` to reload, or restart.

**Q: No file results?** Check `paths` and `file_types`.

**Q: Custom icons?** Use emoji (`⚡`) or local images (ico/png/jpg), `~` = user home.

## Tech Stack

- .NET 8 WPF — native Windows, no WebView
- CommunityToolkit.Mvvm
- YamlDotNet
- Microsoft.Extensions.Hosting

---

<p align="center">Made with 💚 by Quera</p>
