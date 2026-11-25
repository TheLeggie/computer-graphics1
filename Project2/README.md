# BezierSceneApp

Aplikacja okienkowa (.NET 9, WinForms) do wizualizacji, triangulacji, teksturowania oraz oświetlania powierzchni Béziera 3. stopnia z animowanym i edytowalnym źródłem światła.

## Opis algorytmów, założeń i obsługi

Szczegółowy opis sterowania myszą, opcji panelu bocznego oraz **kluczowych algorytmów i przyjętych założeń** (triangulacja, układ współrzędnych i obroty, rasteryzacja AET z Z-bufferem, model oświetlenia Phonga, mapowanie normalnych oraz tor światła na krzywej Béziera) znajduje się bezpośrednio w aplikacji w sekcji **Help** (górne menu okna głównego → **Help**).

---

## Model danych

### 1. Geometria i struktury podstawowe (`src/BezierSceneApp/Model/`)

- **Punkt i wektor 3D** — `System.Numerics.Vector3` (trzy współrzędne `x, y, z` w prawoskrętnym układzie współrzędnych).
- **`Vertex`** (`Model/Vertex.cs`) — wierzchołek siatki trójkątów przechowujący:
  - dane przed obrotem: punkt `P`, wektory styczne `Pu`, `Pv` oraz wektor normalny `N`,
  - dane po obrocie: `RotP`, `RotPu`, `RotPv`, `RotN`,
  - parametry powierzchni `U`, `V` z przedziału `[0, 1]`.
- **`Triangle`** (`Model/Triangle.cs`) — pojedynczy trójkąt siatki zdefiniowany przez referencje do trzech wierzchołków (`V0`, `V1`, `V2`).
- **`Mesh`** (`Model/Mesh.cs`) — siatka powstała z triangulacji powierzchni: lista współdzielonych wierzchołków (`Vertices`) oraz lista trójkątów (`Triangles`). Współdzielenie wierzchołków pozwala jednorazowo obracać każdy wierzchołek metodą `Rotate(Matrix3x3)`.
- **`BezierSurface`** (`Model/BezierSurface.cs`) — płat powierzchni Béziera 3. stopnia opisany tablicą `4×4` punktów kontrolnych przed obrotem (`ControlPoints`) oraz po obrocie (`RotatedControlPoints`). Udostępnia triangulację `Triangulate(int k)` generującą siatkę `Mesh` o `(k + 1) × (k + 1)` wierzchołkach i `2·k²` trójkątach.
- **`Matrix3x3`** (`Model/Matrix3x3.cs`) — niemodyfikowalna struktura (`readonly struct`) macierzy `3×3` w konwencji wektorów kolumnowych (`P' = M · P`), wykorzystywana do reprezentacji orientacji sceny oraz macierzy bazy stycznej przy mapowaniu normalnych.
- **`Scene`** (`Model/Scene.cs`) — główny kontener stanu sceny grupujący:
  - `Surfaces` (`List<BezierSurface>`) — wczytane powierzchnie Béziera,
  - `Meshes` (`List<Mesh>`) — aktualne siatki triangulacji dla poszczególnych powierzchni,
  - `Appearances` (`List<SurfaceAppearance>`) — konfiguracje wyglądu powiązane indeksem 1:1 z `Surfaces` i `Meshes`,
  - `Orientation` (`Matrix3x3`) — skumulowana macierz obrotu sceny (aplikowana zawsze do oryginalnych danych przed obrotem),
  - `UseNormalMap` (`bool`) i `NormalMap` (`TextureData?`) — wspólna dla sceny mapa normalnych.
- **`LightBezierCurve`** (`Model/LightBezierCurve.cs`) — zamknięta krzywa Béziera 6. stopnia leżąca w płaszczyźnie `xy` (`z = const`): 7 punktów kontrolnych `Vector2` (`V0..V6`) z zachowanym niezmiennikiem `V6 = V0` (6 niezależnych punktów edytowalnych `V0..V5`).

### 2. Wygląd powierzchni, tekstury i oświetlenie (`src/BezierSceneApp/Rendering/`)

- **`SurfaceAppearance`** (`Rendering/SurfaceAppearance.cs`) — źródło koloru obiektu dla pojedynczej powierzchni: flaga `UseTexture`, kolor jednolity `SolidColor` (`Vector3` w `[0, 1]`) oraz opcjonalna tekstura `Texture` (`TextureData?`).
- **`TextureData`** (`Rendering/TextureData.cs`) — jednorazowo wczytany obraz (`Width`, `Height`) przechowywany w pamięci jako płaska tablica `Vector3[]`:
  - dla tekstury koloru: kanały RGB znormalizowane do `[0, 1]`,
  - dla mapy normalnych: zdekodowane i znormalizowane wektory w `[-1, 1]`.
- **`LightSettings`** (`Rendering/LightSettings.cs`) — parametry modelu oświetlenia: współczynniki `Kd`, `Ks`, wykładnik połysku `M`, aktualna pozycja źródła światła `Position` (`Vector3` w jednostkach sceny) oraz barwa światła `LightColor` (`Vector3` w `[0, 1]`).

### 3. Dane wejściowe (`data/`)

- **`data/surfaces.txt`** (wczytywany przez `SurfaceFileReader`) — plik tekstowy z definicją co najmniej 2 powierzchni Béziera 3. stopnia. Każda powierzchnia to grupa 16 niepustych wierszy w formacie `x y z` (separator dziesiętny `.`), mapowanych wierszami do `ControlPoints[k / 4, k % 4]`.
- **`data/texture.png`** oraz **`data/normalmap.png`** — domyślna tekstura koloru i mapa normalnych ładowane przy uruchomieniu (katalog `data/` jest automatycznie kopiowany do katalogu wyjściowego podczas budowania projektu).

---

## Uruchomienie

```bash
dotnet run --project src/BezierSceneApp/BezierSceneApp.csproj
```
