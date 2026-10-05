# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [2.0.0-preview.1] - 2026-09-01
Initial release of the extracted Unity UI controls and extensions. Types were moved and refactored out of the legacy Unity-coupled module. Versioning starts at 2.0.0 to mark that split; this is not a new project, it is just a fresh start.

### Added
- `ThreeStatesToggle` now has its own pre-constructed sample, like the base `Toggle`. You can add it to the scene from the `UI Canvas > Three States Toggle` context menu.
- Not a big feature, but if you are planning to build your own runtime controls and use `Mane Style` editors, you can use the pre-built `UINavigationControl`.

### Changed
- All Editor inspectors were re-styled with the new `Mane Style` tool and moved to the modern `UI Toolkit` system.
- `ThreeStateToggle` was renamed to `ThreeStatesToggle`.
- `ManeBehaviour` was renamed to `ManeUIBehaviour` and left exclusively for easy access to `rectTransform` from your UI components.

### Fixed
- `MaxTMProSize` could leave the `LayoutElement` preferred height too big (an extra line) after typing new glyphs following spaces, until the next text change. TMP wraps its preferred height at the current RectTransform width, and the component read it in `Update`, before the layout had assigned the new width. The limits are now calculated inside the layout pass, after the width is known.
