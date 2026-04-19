# ARPControl

ARPControl is a Windows desktop utility for managing CPU power plans, processor behavior, and dynamic performance switching with a clean control-focused interface inspired by advanced power management tools.

## Features

- View and switch Windows power plans
- Apply AC / DC processor tuning settings
- Core parking control
- Frequency scaling control
- Dynamic Boost style profile switching
- Live system status panel
- Tray-friendly desktop utility workflow

## Planned Advanced Features

- Smart App Boost  
  Automatically switch power plans when selected apps start or stop.

- Thermal Guard Mode  
  Change to a safer profile automatically when CPU temperature rises.

- Focus Session Mode  
  Temporarily activate a quiet or balanced profile for coding, work, or battery sessions.

## Tech Stack

- C#
- .NET WinForms
- MaterialSkin
- Windows `powercfg`

## Screenshots

Add screenshots here:

- Main window
- Dynamic Boost settings
- Tray icon / menu
- Power profile events window

## Project Goals

ARPControl aims to provide:

- clean desktop UX
- fast access to power plan controls
- profile automation
- practical CPU power tuning
- a modern GitHub-ready Windows utility

## Current Scope

ARPControl is being developed as a practical power plan manager for Windows systems.  
The project focuses on real-world usability rather than only visual cloning.

Core areas:

- power profile switching
- AC/DC tuning
- automation
- monitoring
- desktop utility experience

## Roadmap

- [x] Base UI
- [x] Power plan listing
- [x] Power plan switching
- [ ] Persistent settings
- [ ] Start with Windows
- [ ] Dynamic Boost automation
- [ ] App-based profile switching
- [ ] Thermal Guard Mode
- [ ] Event viewer / profile history
- [ ] Export / import profile presets

## Installation

1. Clone the repository
2. Open the solution in Visual Studio
3. Restore NuGet packages
4. Install `MaterialSkin.2`
5. Build and run as Administrator

## Run Notes

Some power configuration operations require administrator privileges.  
For full functionality, run ARPControl as Administrator.

## Branding

Recommended visual direction:

- Background: `#0F172A`
- Surface: `#111827`
- Primary accent: `#22C55E`
- Secondary accent: `#38BDF8`
- Text: `#E5E7EB`

## License

Choose a license for your project, for example:

- MIT License

## Author

Developed by the ARPControl project owner.
