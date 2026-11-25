# BezierSceneApp

Aplikacja okienkowa (.NET 9, WinForms) do wizualizacji, triangulacji, teksturowania oraz oświetlania powierzchni Béziera 3. stopnia z animowanym i edytowalnym źródłem światła.

## Opis algorytmów, założeń i obsługi

Opis sterowania myszą, opcji panelu bocznego oraz algorytmów i założeń znajduje się bezpośrednio w aplikacji w sekcji **Help**

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
- **`BezierSurface`** (`Model/BezierSurface.cs`) — powierzchnia Béziera 3. stopnia opisana tablicą `4×4` punktów kontrolnych przed obrotem (`ControlPoints`) oraz po obrocie (`RotatedControlPoints`). Udostępnia triangulację `Triangulate(int k)` generującą siatkę `Mesh` o `(k + 1) × (k + 1)` wierzchołkach i `2·k²` trójkątach.
- **`Matrix3x3`** (`Model/Matrix3x3.cs`) — niemodyfikowalna struktura  macierzy `3×3` w konwencji wektorów kolumnowych (`P' = M · P`), wykorzystywana do reprezentacji orientacji sceny oraz macierzy bazy stycznej przy mapowaniu normalnych.
- **`Scene`** (`Model/Scene.cs`) — główny kontener stanu sceny:
  - `Surfaces` (`List<BezierSurface>`) — wczytane powierzchnie Béziera,
  - `Meshes` (`List<Mesh>`) — aktualne siatki triangulacji dla poszczególnych powierzchni,
  - `Appearances` (`List<SurfaceAppearance>`) — konfiguracje wyglądu,
  - `Orientation` (`Matrix3x3`) — skumulowana macierz obrotu sceny (aplikowana zawsze do oryginalnych danych przed obrotem),
  - `UseNormalMap` (`bool`) i `NormalMap` (`TextureData?`) — wspólna dla sceny mapa normalnych.
- **`LightBezierCurve`** (`Model/LightBezierCurve.cs`) — zamknięta krzywa Béziera 6. stopnia leżąca w płaszczyźnie `xy` (`z = const`): 7 punktów kontrolnych 
