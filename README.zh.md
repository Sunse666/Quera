# Quera

<p align="center">
  <img src="assets/logo/logo.png" alt="Quera Logo" width="128">
</p>

<p align="center">
  <strong>Windows 键盘快捷启动器</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Platform-Windows-blue?style=flat-square">
  <img src="https://img.shields.io/badge/Framework-.NET%208%20WPF-purple?style=flat-square">
  <img src="https://img.shields.io/badge/License-GPL%203.0-green?style=flat-square">
</p>

## 简介

Quera 是一款 Windows 键盘驱动快速启动器。按下 `Alt + Space` 唤出搜索窗口，输入关键词即可搜索应用、文件、命令、书签，支持计算器、系统命令、输入前缀等。基于 .NET 8 WPF 原生构建，轻量高效。

## 功能

- **全局热键** — `Alt + Space` 唤出/隐藏，带动画过渡
- **文件搜索** — 自动索引开始菜单与自定义目录
- **自定义命令** — `shell:` / `cmd:` / `ps:` / `run:` 多种执行方式，支持管理员权限
- **书签** — 关键词直达网页
- **搜索引擎** — `g 搜索词` 直接搜索
- **文件夹快捷方式** — 关键词打开目录
- **计算器** — 输入 `2+2*3` 直接出结果
- **系统命令** — `shutdown` / `restart` / `lock` / `sleep`
- **输入前缀** — `>` 只搜命令、`/` 只搜文件、`@` 只搜书签
- **别名** — `reboot` → `shutdown /r /t 0`
- **历史记录** — 高频结果自动排前面
- **分页浏览** — `Tab` / `Shift+Tab` 翻页
- **失焦隐藏** — 自动隐藏，不打断工作流
- **系统托盘** — 常驻托盘，右键菜单
- **高自定义** — 70+ YAML 配置项，颜色/字号/布局/快捷键/优先级全可配
- **图片图标** — 支持 emoji 和 ico/png/jpg 图片

## 快捷键

| 键 | 功能 |
|---|---|
| `Alt + Space` | 唤出 / 隐藏 |
| `↑` `↓` | 导航结果 |
| `Enter` | 执行选中项 |
| `Tab` | 下一页（搜索引擎提示时补全关键词） |
| `Shift + Tab` | 上一页 |
| `Esc` | 隐藏窗口 |
| `Ctrl + ,` | 打开配置文件 |
| `Ctrl + R` | 重载配置 |

## 快速开始

1. 下载并解压到任意目录
2. 运行 `Quera.exe`
3. 按 `Alt + Space` 唤出
4. 编辑 `config.yaml` 自定义

## 配置

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
  placeholder: "搜索应用、文件、命令..."
  icon: "🔍"
  esc_hint: "ESC 关闭"

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
    name: 命令提示符
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
    name: 桌面
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

## 常见问题

**Q: 热键冲突？** 修改 `settings.hotkey`，支持 Ctrl / Alt / Shift / Win 组合。

**Q: 新配置未生效？** 按 `Ctrl + R` 重载配置，或重启程序。

**Q: 搜索不到文件？** 检查 `paths` 和 `file_types`。

**Q: 图标怎么设？** 支持 emoji（`⚡`）和本地图片路径（ico/png/jpg），`~` 表示用户目录。

## 技术栈

- .NET 8 WPF（无 WebView 依赖）
- CommunityToolkit.Mvvm
- YamlDotNet
- Microsoft.Extensions.Hosting

---

<p align="center">Made with 💚 by Quera</p>
