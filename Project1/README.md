# Edytor wielokątów

## Model danych

- `PolygonEditor.Core` – model sceny, geometria i ograniczenia,
- `PolygonEditor` – interfejs (WinForms), obsługa myszy i rysowanie.

### `Scene`
- `Polygons` – lista wielokątów; im dalej na liście, tym wyżej rysowany (i wcześniej trafiany przy wyszukiwaniu pod kursorem).
- `Constraints` – globalna lista wszystkich ograniczeń.
- Przesuwanie wierzchołków i punktów kontrolnych (wywołuje solver ograniczeń i przywracanie ciągłości), dodawanie/usuwanie ograniczeń, wierzchołków, zmiana typu krawędzi.
- Wyszukiwanie pod kursorem: `FindVertexAt`, `FindControlPointAt`, `FindEdgeAt`, `FindPolygonAt`.

### `Polygon`
- `Vertices`, `Edges` – listy cykliczne: `Edges[i]` łączy `Vertices[i]` z `Vertices[(i + 1) % Count]`.

### `Vertex`
- `X`, `Y` – położenie (`float`).
- `IncomingEdge`, `OutgoingEdge` – sąsiednie krawędzie (lista dwukierunkowa wierzchołek - krawędź).

### `Edge`
- `StartVertex`, `EndVertex`.
- `EdgeType` – `Line`, `Bezier` lub `Arc`.
- `ControlPoint1`, `ControlPoint2` – punkty kontrolne (tylko dla krzywej Béziera 3. stopnia, w pozostałych przypadkach `null`).
- `Constraints` – ograniczenia nałożone na krawędź (dodawane i usuwane wyłącznie przez `Scene`).

### Klasy pomocnicze
- `ConstraintSolver` – przywracanie ograniczeń (BFS + relaksacja).
- `ContinuityEnforcer` – utrzymanie ciągłości G0/G1/C1 w wierzchołkach.
- `CubicBezier`, `CircularArc`, `GeometryMath` – geometria (wyliczanie punktów, stycznych, odległości).
