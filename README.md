⏰ TimeTracker

TimeTracker is a smart Windows productivity tool that automatically tracks your work hours, breaks, and system activity using Windows events — no manual punching required.

It runs quietly in the background with a system tray UI, logs accurate time entries, and celebrates your achievements when you complete your day 🎉

Created with ❤️ by Anant.

✨ Features

🔐 Automatic tracking of:

Windows login

Lock (Win + L)

Unlock

Break durations

🧠 Accurate work vs break time calculation

🖥️ Windows Service for reliable background tracking

📌 WPF system tray (NotifyIcon) UI

📝 Daily time logs (human-readable)

🎉 Fun notifications when you complete 8.5 hours

🔄 Safe service + app restart handling

📦 MSI installer using WiX Toolset

🛠️ Tech Stack

.NET Framework

WPF

Windows Service

WiX Toolset

Hardcodet NotifyIcon (System Tray)

🚀 How It Works

Windows Service listens to login, lock, and unlock events

Time entries are written automatically

Tray app shows live status and summaries

On completion of daily target → 🎊 celebration notification

📦 Installation

Download the latest .msi from Releases

Run installer (admin required)

Service starts automatically

Tray app launches after install

🔄 Updates

TimeTracker supports versioned releases.
New updates are published via GitHub Releases.

🧑‍💻 Author

Anant
Lead Software Engineer
Built for productivity — with fun 😄

⚠️ Disclaimer

This tool is intended for personal productivity tracking.
No data is sent externally — all data stays on your machine.
