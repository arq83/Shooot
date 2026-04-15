# com.yourname.core

Reużywalna paczka UPM zawierająca fundament architektury gier w Unity.

## Instalacja

1. Otwórz projekt Unity
2. Skopiuj folder `com.yourname.core` do folderu `Packages/` swojego projektu
3. Unity automatycznie wykryje paczkę

Lub dodaj przez `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.yourname.core": "file:../com.yourname.core"
  }
}
```

## Zawartość

| Klasa | Opis |
|-------|------|
| `EventQueue` | Kolejka eventów z double-bufferingiem |
| `GameLoop` | Pętla gry iterująca po systemach |
| `ISystem` | Interfejs dla systemów gry |
| `ICoroutineRunner` | Interfejs do uruchamiania coroutine bez zależności od MonoBehaviour |
| `ObjectPool<T>` | Generyczna pula obiektów Unity |

## Namespace

```csharp
using YourName.Core;
```

---

## EventQueue

Kolejka eventów z **double-bufferingiem** — eventy dodane w klatce N są widoczne w klatce N+1. Chroni przed modyfikowaniem kolekcji podczas iteracji.

### Użycie

```csharp
var queue = new EventQueue();

// Dodaj event (widoczny w następnej klatce)
queue.Enqueue(new MoveEvent(direction));

// Dodaj event (widoczny w tej samej klatce)
queue.EnqueueImmediate(new SpawnEvent(position));

// Odczytaj eventy (zawsze zwraca kopię — bezpieczna iteracja)
foreach (var e in queue.GetEvents())
{
    if (e is MoveEvent move)
    {
        // obsłuż event
    }
}

// Wywołaj na początku każdej klatki (robi to GameLoop automatycznie)
queue.Swap();
```

### Kiedy używać `Enqueue` vs `EnqueueImmediate`?

- `Enqueue` — standardowe eventy między systemami (ruch, input)
- `EnqueueImmediate` — krytyczne eventy które muszą być obsłużone w tej samej klatce (np. spawn pocisku po strzale, game over po śmierci gracza)

---

## GameLoop

Zarządza listą systemów i wywołuje ich `Update()` w odpowiedniej kolejności.

### Użycie

```csharp
var queue = new EventQueue();

var systems = new List<ISystem>
{
    new InputSystem(queue),
    new MovementSystem(queue),
    new PhysicsSystem(queue),
};

var gameLoop = new GameLoop(systems, queue);

// W MonoBehaviour.Update():
void Update()
{
    gameLoop.Update();
}
```

### Kolejność systemów ma znaczenie!

Systemy są wykonywane w kolejności na liście. Zalecana kolejność:
1. Input
2. Movement
3. AI/Enemy
4. Shooting/Projectiles
5. Damage
6. State (GameOver, Pause)

---

## ISystem

Interfejs który musi implementować każdy system gry.

### Użycie

```csharp
public class MySystem : ISystem
{
    private EventQueue queue;

    public MySystem(EventQueue queue)
    {
        this.queue = queue;
    }

    public void Update()
    {
        foreach (var e in queue.GetEvents())
        {
            if (e is MyEvent myEvent)
            {
                // obsłuż event
            }
        }
    }
}
```

---

## ICoroutineRunner

Interfejs pozwalający uruchamiać opóźnione akcje bez bezpośredniej zależności od `MonoBehaviour`. Dzięki temu systemy (które nie są MonoBehaviour) mogą zlecać opóźnione operacje.

### Implementacja w projekcie

```csharp
// W swoim MonoBehaviour (np. GameInstaller):
public class GameInstaller : MonoBehaviour, ICoroutineRunner
{
    public void RunDelayed(System.Action action, float delay)
    {
        StartCoroutine(DelayRoutine(action, delay));
    }

    private IEnumerator DelayRoutine(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
}
```

### Użycie w systemie

```csharp
public class SpawnSystem : ISystem
{
    private ICoroutineRunner runner;

    public SpawnSystem(ICoroutineRunner runner)
    {
        this.runner = runner;
    }

    public void Update()
    {
        // opóźniony spawn bez zależności od MonoBehaviour
        runner.RunDelayed(() =>
        {
            SpawnEnemy();
        }, 1f);
    }
}
```

### Testowanie z mockiem

```csharp
// W testach — natychmiastowe wykonanie bez opóźnienia:
public class TestCoroutineRunner : ICoroutineRunner
{
    public void RunDelayed(System.Action action, float delay)
    {
        action?.Invoke(); // od razu, bez czekania
    }
}
```

---

## ObjectPool\<T\>

Generyczna pula obiektów Unity. Zapobiega częstemu `Instantiate`/`Destroy` co poprawia wydajność.

### Użycie

```csharp
// Stwórz pulę z prefabem i rozmiarem początkowym
var pool = new ObjectPool<ProjectileView>(projectilePrefab, initialSize: 20);

// Pobierz obiekt z puli (aktywuje GameObject)
var obj = pool.Get();
obj.transform.position = spawnPosition;

// Zwróć obiekt do puli (dezaktywuje GameObject)
pool.Return(obj);
```

### Wymagania

- `T` musi dziedziczyć z `Component`
- Prefab musi mieć komponent typu `T`
- Pamiętaj o wywołaniu `Return()` zamiast `Destroy()`

---

## Przykładowa architektura projektu

```
Assets/
└── _Project/
    └── Scripts/
        ├── Data/           # RuntimeData, ScriptableObjects
        ├── Events/         # Struktury eventów (MoveEvent, HitEvent...)
        ├── Systems/        # Implementacje ISystem
        ├── View/           # MonoBehaviour widoków
        ├── UI/             # Kontrolery UI
        └── Installers/     # GameInstaller — kompozycja systemu

Packages/
└── com.yourname.core/     # Ta paczka
```

### Przepływ danych

```
Input → EventQueue → Systems → RuntimeData → Views
```

1. `InputSystem` zbiera input i wrzuca eventy do `EventQueue`
2. `GameLoop` wywołuje `Update()` na każdym systemie
3. Systemy czytają eventy i modyfikują `RuntimeData`
4. Widoki (`View`) czytają `RuntimeData` i aktualizują GameObject

---

## Assembly Definition

Jeśli używasz `.asmdef` w projekcie, dodaj referencję:

```json
{
    "name": "com.yourname.game",
    "references": [
        "com.yourname.core.Runtime"
    ]
}
```

I dodaj namespace w plikach:

```csharp
using YourName.Core;
```

---

## Wymagania

- Unity 6000.0+
- .NET Standard 2.1

## Licencja

MIT
