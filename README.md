# AR Image Tracking and 3D Model Spawning (Android)

An Android AR app built with Unity AR Foundation and ARCore. The app detects a physical image through the device camera, spawns a 3D model on top of it, keeps the model aligned with the image, and handles tracking being lost and regained.

> Fill in the items in [square brackets] before you commit this file.

## Task checklist

| # | Requirement | Status |
|---|-------------|--------|
| 1 | Unity AR Foundation project | Done |
| 2 | AR Session and XR Origin configured | Done |
| 3 | Reference Image Library created | Done |
| 4 | Reference image added (printed marker) | Done: see `Marker/` |
| 5 | Image detected with the device camera | Done |
| 6 | 3D model spawned on detection | Done |
| 7 | Model kept aligned with the tracked image | Done |
| 8 | Tracking lost and regained handled | Done |

## Deliverables

- **Unity project:** this repository
- **Android APK:** see the [Releases page]([link to your release])
- **Screen recording:** [link to the release asset or Google Drive]

## Tech stack

- Unity **6000.0.51f1** (Unity 6), Universal Render Pipeline
- AR Foundation **[version]** and Google ARCore XR Plugin **[version]**
- Target: Android with ARCore support
- Tested on: Motorola Moto G62 5G

## How it works

The logic lives in `Assets/Scripts/ImageTrackingHandler.cs`, attached to the XR Origin next to the `AR Tracked Image Manager`.

- **Spawn:** when the manager reports a newly detected image, the script instantiates the model prefab.
- **Alignment:** the model is a child of the tracked image's transform. AR Foundation updates that transform every frame, so the model stays pinned to the image as the phone or the image moves.
- **Scale:** the model is scaled relative to the image's physical width, so it fits the marker.
- **Tracking lost:** when the tracking state drops from `Tracking` to `Limited` or `None`, the model is hidden and the on-screen text shows a "Tracking lost" message.
- **Tracking regained:** when the image becomes visible again and the state returns to `Tracking`, the model is shown again at the correct pose and the text updates.

## The marker

- Marker image: `Marker/[filename]`
- Printed size: **[for example 15 cm x 15 cm]**. The Reference Image Library has the same physical size set, so print it at that width.
- Print it flat, in colour, on matte paper if possible, and avoid glare. You can also display it full-screen on a monitor or another phone at high brightness.

## How to test the APK

1. Download the APK from the Releases page and install it on an ARCore-supported Android phone. You may need to allow installs from unknown sources.
2. Install or update **Google Play Services for AR** from the Play Store if the phone asks.
3. Open the app and allow camera access.
4. Point the camera at the marker from about 20 to 40 cm, with good light and the marker fully in view.
5. The model appears on the marker. Move the phone around to check alignment.
6. Look away or cover the marker to see the "tracking lost" state, then point back at it to see the model reappear.

## Build settings

- Scripting backend IL2CPP, target architecture ARM64
- Minimum API level [24]
- Graphics API OpenGLES3
- Google ARCore enabled in XR Plug-in Management (Android)
- `AR Background Renderer Feature` added to the URP renderer, so the camera feed is visible

## How to open the project

1. Install **Unity 6000.0.51f1** with Android Build Support.
2. Clone this repository and open the folder in Unity Hub.
3. Open the scene `Assets/Scenes/[SceneName].unity`.
4. Switch the platform to Android, connect a phone with USB debugging on, and click **Build And Run**.

The editor shows warnings about a missing camera or image tracking subsystem when you press Play. That is expected, because ARCore only runs on an Android device.

## Project structure

```
Assets/
  Scenes/       Main AR scene
  Scripts/      ImageTrackingHandler.cs
  Prefabs/      Spawned model prefab
  Image/        Reference image and Reference Image Library
Marker/         Printable marker image
Packages/
ProjectSettings/
```

## Known limitations

- Requires an ARCore-supported Android device
- Detection quality depends on lighting, print quality, and the marker's detail
- Image tracking cannot be tested in the Unity editor without extra simulation setup
- [Anything else you noticed]
