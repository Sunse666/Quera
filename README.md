# Quera
<p align="center"> <img src="assets/logo/logo.png" alt="Quera Logo" width="128"> </p><p align="center"> <strong>用于快捷启动</strong> </p><p align="center"> <img src="https://img.shields.io/badge/Platform-Windows-blue?style=flat-square" alt="Platform"> <img src="https://img.shields.io/badge/Engine-WebView2-brightgreen?style=flat-square" alt="Engine"> </p>


## ✨ 简介
- 基于 WebView2 的 Windows 快速启动器，追求极致的轻量与高效，通过全局热键一键唤出，快速搜索并启动应用、文件、命令、书签，提升工作效率。


## 🖼️ 截图
<p align="center"> <img src="assets/Screenshot.png" alt="Screenshot"> </p>


## 🚀 功能特性

### 🔍 全局搜索
- 一键搜索应用程序、文件、文件夹
- 自动索引开始菜单程序与自定义目录
- 支持模糊匹配，快速定位目标

### ⚡ 自定义命令
- 创建自定义快捷命令
- 支持 Shell、CMD、PowerShell 多种执行方式
- 可配置管理员权限运行

### 🔗 书签管理
- 快速访问常用网页书签
- 关键词触发，一键直达

### 🌐 搜索引擎集成
- 输入 关键词 + 空格 + 搜索词 直接搜索
- 支持自定义多个搜索引擎

### 📁 文件夹快捷方式
- 配置常用文件夹快捷入口
- 关键词快速打开目标目录

### ⌨️ 快捷键操作

> [!TIP]
> |快捷键|功能|
> |---|---|
> |Alt + Space|唤出/隐藏窗口|
> |↑ ↓|	选择导航结果|
> |Enter|执行选中项|
> |Tab|展开搜索引擎|
> |Esc|	隐藏窗口|
> |Ctrl + ,|打开配置文件|
> |Ctrl + R|重新加载配置|

### 📌 系统托盘
- 最小化后自动收入系统托盘
- 右键托盘图标可快速打开、配置或退出


## 📦 安装与使用

### 系统要求
- 操作系统：Windows 10 / 11
- 运行时：Microsoft Edge WebView2 Runtime

### 快速开始

1. 从 Releases 页面下载最新版本
2. 解压到任意目录
3. 运行 Quera.exe
4. 按下 Alt + Space 唤出搜索窗口

### 目录结构

```
Quera/
├── Quera.exe       # 主程序
├── config.ini       # 配置文件
└── assets/icon/   # 图标
```


## ⚙️ 配置说明

配置文件为程序目录下的 config.ini，支持以下配置段：

### 基础设置 

```
[settings]
hotkey = Alt+Space      # 全局热键
width = 680             # 窗口宽度
height = 480            # 窗口高度
opacity = 95            # 窗口透明度
max_results = 30        # 最大显示结果数
```

### 搜索路径 

```
[path]
# 每行一个路径，支持绝对路径和 ~ 表示用户目录
D:\Tools
~\Downloads
```

### 文件类型 

```
[filetype]
# 每行一个扩展名
.exe
.lnk
.bat
.ps1
```

### 自定义命令

```
[command]
keyword = calc
name = 计算器
action = run:calc.exe
icon = 🧮
```

### 书签

```
[bookmark]
keyword = gt
name = GitHub
url = https://github.com
icon = .\assets\icon\github.png

[bookmark]
keyword = mail
name = Gmail
url = https://mail.google.com
icon = M
```

### 搜索引擎

```
[search]
keyword = gg
name = Google
url = https://www.google.com/search?q={query}
icon = 🔍

[search]
keyword = bing
name = bing
url = https://www.baidu.com/s?wd={query}
icon = 🔍

[search]
keyword = bili
name = Bilibili
url = https://search.bilibili.com/all?keyword={query}
icon = .\assets\icon\bilibili.png
```

> [!NOTE]
> 搜索引擎使用方法： 输入 gg hello world 后按 Enter，将使用 Google 搜索 "hello world"


## 🛠️ 技术特点

- 基于 Chromium WebView2 渲染引擎，界面现代风格
- 智能缓存 文件索引，搜索响应迅速
- 失焦自动隐藏，不打断工作流程
- 托盘常驻，随时待命
- 支持 高 DPI 显示器 适配


## ❓ 常见问题

### Q: 热键冲突怎么办？

修改 config.ini 中的 hotkey 配置项，支持的修饰键：Ctrl、Alt、Shift、Win

### Q: 如何添加自定义图标？

图标支持 emoji 或本地图片路径（ico/png/jpg），例如：

```
icon = 🚀
icon = ~\icons\myapp.png
```

### Q: 新配置未加载？

按 Ctrl + R 重新加载配置并刷新索引

---
<p align="center"> Made with 💚 by Quera </p>
