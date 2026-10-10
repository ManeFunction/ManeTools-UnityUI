# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [2.0.0-preview.1] - 2026-09-01
Initial release of the extracted Unity UI controls and extensions. Types were moved and refactored out of the legacy Unity-coupled module. Versioning starts at 2.0.0 to mark that split; this is not a new project, it is just a fresh start.

### Added
- `ThreeStatesToggle` now has its own pre-constructed sample, like the base `Toggle`. You can add it to the scene from the `UI Canvas > Three States Toggle` context menu.
- Not a big feature, but if you are planning to build your own runtime controls and use `Mane Style` editors, you can reuse the pre-built foldable `Interaction` block (`UIInteractionControl`: Interactable, Transition and `UINavigationControl`) in your `Selectable` inspectors. `UINavigationControl` can also be used on its own.
- Added **Create → Scripting → UIBehaviour** and **Create → Scripting → ManeUIBehaviour**. They create those scripts the same way as the built-in scripting templates, with an empty class body.

### Changed
- All Editor inspectors were re-styled with the new `Mane Style` tool and moved to the modern `UI Toolkit` system.
- `ThreeStateToggle` was renamed to `ThreeStatesToggle`.
- `ManeBehaviour` was renamed to `ManeUIBehaviour` and left exclusively for easy access to `rectTransform` from your UI components.
- `MaxTMProSize` can now fit the width to the widest wrapped line when the text exceeds `Max Width`, instead of always using the full `Max Width`, so a word that wraps to the next line no longer leaves an empty gap on the right. It is controlled by the new `Compact Width` option (off by default), which is shown only while `Max Width` is set.
- `MaxTMProSize` no longer needs a `LayoutElement`. It is a layout element itself, with layout priority 2, so it no longer overwrites values set on a `LayoutElement`, does not dirty scenes in edit mode, and its caps go away when it is disabled or removed.

### Fixed
- `ThreeStatesToggle.StateValueChanged -= handler` did not remove the handler.
- `ThreeStatesToggle` could be clicked while not interactable, and Submit (keyboard or gamepad) flipped `isOn` without changing `State`. Both now cycle the state and respect `interactable`.
- `MaxTMProSize` could leave the preferred height too big (an extra line) after typing new glyphs following spaces, until the next text change. TMP wraps its preferred height at the current RectTransform width, and the component read it in `Update`, before the layout had assigned the new width. The limits are now calculated inside the layout pass, after the width is known.
