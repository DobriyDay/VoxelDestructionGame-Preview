# VoxelDestructionGame-Preview

## LevelConstructorEditor.cs  
- Сохраняет данные уровня в ScriptableObject (позиции объектов, настройки перемещения, вращение и позиции границ уровня, положение персонажа).  
- Позволяет редактировать и загружать уровень прямо в редакторе Unity. (сверху редактора вкладочка Tools/)

## SceneBootstrap  
- Точка входа в игру.
- Создаёт и инициализирует все необходимые игровые сервисы и инстансы.

## LevelsLoader  
- Загружает файлы уровней по их ID из ресурсов или внешних источников(если дебаг включен).

## LevelBuilder  
- Использует `IAssetProvider` для загрузки объектов уровня.  
- В проекте `IAssetProvider` реализован через Addressables.  
- Создаёт объекты, инициализирует их и запускает физику.

## GameplayFlow  
- Управляет игровым процессом:  
  - Загружает данные уровня через `LevelsLoader`.  
  - Запускает построение уровня через `LevelBuilder`.  
  - Контролирует старт игры, экраны наград, завершение и рестарт, включает шторку.  
  - Обрабатывает UI-переходы между состояниями игры.

## Основная механика физики  
- Реализована на чанках:  
  - `Chunk.cs` — набор объектов, которые разрушаются/рассыпаются.  
  - `ChunkContainer` — хранит чанки и управляет взаимодействием с ними.

## Доступ к платформенному SDK  
- Интеграция через интерфейс `IPlatformSDK`.  
- В проекте реализован через PluginYourGames SDK для YandexGames.

---

## LevelConstructorEditor.cs  
- Saves level data into a ScriptableObject (object positions, movement settings, rotation, and level boundary positions, player start position).  
- Allows editing and loading levels directly in the Unity Editor (accessible from the top menu under Tools/).

## SceneBootstrap  
- The entry point of the game.  
- Creates and initializes all necessary game services and instances.

## LevelsLoader  
- Loads level files by their ID from resources or external sources (if debug mode is enabled).

## LevelBuilder  
- Uses `IAssetProvider` to load level objects.  
- In this project, `IAssetProvider` is implemented via Addressables.  
- Instantiates objects, initializes them, and starts physics simulation.

## GameplayFlow  
- Manages the gameplay flow:  
  - Loads level data via `LevelsLoader`.  
  - Triggers level construction through `LevelBuilder`.  
  - Controls game start, reward screens, game end, and restart; also handles screen fade (curtain).  
  - Handles UI transitions between game states.

## Core Physics Mechanics  
- Implemented on chunks:  
  - `Chunk.cs` is a set of objects that break apart or scatter.  
  - `ChunkContainer` stores chunks and manages their interactions.

## Platform SDK Access  
- Integration is done through the `IPlatformSDK` interface.  
- In the project, it is implemented via the PluginYourGames SDK for YandexGames.

