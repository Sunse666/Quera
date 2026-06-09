# Quera

<p align="center">
  <strong>Windows 键盘快捷启动器</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Platform-Windows-blue?style=flat-square">
  <img src="https://img.shields.io/badge/Framework-.NET%208%20WPF-purple?style=flat-square">
  <img src="https://img.shields.io/badge/License-MIT-green?style=flat-square">
</p>

## 简介

Quera 是一款 Windows 键盘驱动快速启动器。按下全局热键唤出搜索窗口，输入关键词即可搜索应用、文件、命令、书签，一键执行或打开。基于 .NET 8 WPF 原生构建，轻量高效。

## 功能

- **全局热键** — 一键唤出/隐藏，默认 `Alt + Space`
- **文件搜索** — 自动索引开始菜单与自定义目录
- **自定义命令** — shell: / cmd: / ps: / run: 多种执行方式
- **书签** — 关键词直达网页
- **搜索引擎** — `g 搜索词` 直接搜索
- **文件夹快捷方式** — 关键词打开目录
- **分页浏览** — 结果分页显示，Tab/Shift+Tab 翻页
- **失焦隐藏** — 自动隐藏，不打断工作流
- **系统托盘** — 常驻托盘，右键菜单
- **高自定义** — 70+ 配置项，颜色/字号/布局/快捷键全可配

## 快捷键

| 键 | 功能 |
|---|---|
| `Alt + Space` | 唤出 / 隐藏 |
| `↑` `↓` | 导航结果 |
| `Enter` | 执行选中项 |
| `Tab` | 下一页 |
| `Shift + Tab` | 上一页 |
| `Esc` | 隐藏窗口 |
| `Ctrl + ,` | 打开配置文件 |
| `Ctrl + R` | 重载配置 |

## 快速开始

1. 下载并解压到任意目录
2. 运行 `Quera.exe`
3. 按 `Alt + Space` 唤出搜索窗口
4. （可选）编辑 `config.yaml` 自定义配置

## 配置

配置文件为 `config.yaml`，支持以下全部选项：

```yaml
# ── 基础设置 ──
settings:
  hotkey: Alt+Space          # 全局热键 (Ctrl/Alt/Shift/Win + 键)
  width: 680                 # 窗口宽度
  opacity: 96                # 不透明度 (1-100)
  max_results: 30            # 最大搜索结果数
  autostart: false           # 开机自启
  hide_on_deactivate: true   # 失焦自动隐藏
  hide_delay_ms: 200         # 失焦隐藏延迟（毫秒）
  show_on_startup: false     # 启动时是否直接显示窗口

# ── 窗口 ──
window:
  always_on_top: true        # 始终置顶

# ── 颜色 ──
colors:
  background: "#B216213E"          # 窗口背景
  search_card: "#0DFFFFFF"         # 搜索卡片背景
  search_border: "#12FFFFFF"       # 搜索卡片边框
  result_card: "#CC1A1A2E"         # 结果卡片背景
  result_border: "#33FFFFFF"       # 结果卡片边框
  result_hover: "#18FFFFFF"        # 结果悬停
  result_selected_start: "#D97035" # 选中渐变起点
  result_selected_end: "#E8955A"   # 选中渐变终点
  text_primary: "#FFFFFF"          # 主文字
  text_secondary: "#8899AA"        # 次要文字
  text_muted: "#556677"            # 弱化文字
  accent: "#D97035"                # 强调色

# ── 搜索框 ──
search_box:
  placeholder: "搜索应用、文件、命令..."
  icon: "🔍"
  esc_hint: "ESC 关闭"

# ── 结果列表 ──
results:
  padding_h: 10
  padding_v: 7
  margin: 1
  icon_size: 20
  badge_font_size: 9

# ── 布局 ──
layout:
  outer_margin: 12
  card_gap: 10
  search_padding: 14
  results_padding: 6

# ── 快捷键 ──
shortcuts:
  next_page: Tab
  prev_page: Shift+Tab
  execute: Enter
  hide: Escape
  open_config: "Ctrl+,"
  reload_config: Ctrl+R

# ── 搜索 ──
search:
  match_mode: contains        # contains / starts_with / fuzzy
  include_directories: false  # 是否搜索目录名
  max_depth: -1               # 目录扫描深度，-1 无限

# ── 排除 ──
exclude:
  paths:
    - ~/AppData
    - ~/.git
  patterns:
    - "*.tmp"
    - "*.log"

# ── 缓存 ──
cache:
  enabled: true
  refresh_on_start: true
  max_files: 50000

# ── UI ──
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

# ── 终端 ──
terminal:
  default: cmd
  admin_default: powershell

# ── 搜索路径 ──
paths:
  - ~/Desktop                 # ~ = 用户目录
  - C:/Tools

# ── 文件类型 ──
file_types:
  - .exe
  - .lnk
  - .bat
  - .ps1

# ── 自定义命令 ──
commands:
  - keyword: cmd
    name: 命令提示符
    action: run:cmd.exe       # shell: / cmd: / ps: / run:
    icon: "⚡"
    admin: false

# ── 书签 ──
bookmarks:
  - keyword: gh
    name: GitHub
    url: https://github.com
    icon: "🔗"

# ── 文件夹 ──
folders:
  - keyword: desk
    name: 桌面
    path: ~/Desktop
    icon: "📁"

# ── 搜索引擎 ──
search_engines:
  - keyword: g
    name: Google
    url: https://www.google.com/search?q={query}
    icon: "🔍"

# ── 优先级 ──
priority:
  types:                      # 越靠前越先显示
    - command
    - bookmark
    - file
    - folder
    - search
    - search_hint
  extensions:                 # 同类文件排序
    - .exe
    - .lnk
    - .bat
    - .ps1
  custom_path_first: true
```

## 常见问题

**Q: 热键冲突？** 修改 `settings.hotkey`，支持 Ctrl / Alt / Shift / Win 组合。

**Q: 新配置未生效？** 按 `Ctrl + R` 重载配置，或重启程序。

**Q: 搜索不到文件？** 检查 `paths` 是否配置了正确的搜索路径，`file_types` 是否包含目标后缀。

**Q: 图标怎么设？** 支持 emoji（如 `⚡`）和本地图片路径（ico/png/jpg）。

## 技术栈

- .NET 8 WPF（原生 Windows 框架，无 WebView 依赖）
- CommunityToolkit.Mvvm
- YamlDotNet
- Microsoft.Extensions.Hosting（DI 容器）

---

<p align="center">Made with 💚 by Quera</p>
