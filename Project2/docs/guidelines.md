---
name: bezier-scene-winforms
description: Zasady pracy, architektury, Karpathy guidelines, etapy slice-by-slice i protokol aktualizacji projektu powierzchni Beziera 3. stopnia w .NET 9 (docelowo .NET 10) i WinForms.
---

# Bezier Scene (.NET 9 / .NET 10 + WinForms) — Project Skill

## 1. Rady dla modelu i Karpathy Guidelines
Te zasady mają ograniczać typowe błędy LLM-ów przy pisaniu i modyfikowaniu kodu (na podstawie obserwacji Andreja Karpathy'ego). W razie wątpliwości stawiaj na ostrożność, prostotę i weryfikację zamiast pośpiechu.

### 1.1. Think Before Coding
- Nie zakładaj po cichu i nie ukrywaj niejasności. Pokazuj kompromisy.
- Przed wdrożeniem wypisz jawnie przyjęte założenia; jeśli coś jest niepewne, zapytaj.
- Gdy istnieje kilka interpretacji, przedstaw je zamiast wybierać bez słowa.
- Jeśli istnieje prostsze podejście, powiedz o tym; gdy trzeba, zgłoś sprzeciw.
- Gdy coś jest niejasne, zatrzymaj się, nazwij co budzi wątpliwości i zapytaj.

### 1.2. Simplicity First
- Minimalny kod rozwiązujący problem. Zero spekulacji i funkcji „na zapas”.
- Nie dodawaj funkcji ponad to, o co poproszono.
- Nie twórz abstrakcji dla kodu używanego w jednym miejscu.
- Nie dodawaj „elastyczności” ani konfiguracji, których nie wymagano.
- Nie pisz obsługi błędów dla scenariuszy niemożliwych.
- Jeśli coś zajmuje 200 linii, a da się to napisać w 50, uprość to. Zapytaj siebie, czy senior powiedziałby, że to przekombinowane — jeśli tak, uprość.

### 1.3. Surgical Changes
- Dotykaj tylko tego, co konieczne. Sprzątaj tylko własny bałagan.
- Przy edycji istniejącego kodu nie „poprawiaj” sąsiedniego kodu, komentarzy ani formatowania.
- Nie refaktoryzuj rzeczy, które działają i nie są zepsute.
- Dopasuj się do istniejącego stylu, nawet jeśli zrobiłbyś to inaczej.
- Jeśli zauważysz niepowiązany martwy kod, wspomnij o nim, ale go nie usuwaj.
- Gdy Twoje zmiany zostawią nieużywane importy, zmienne lub funkcje — usuń to, co stało się zbędne przez Twoje zmiany (nie usuwaj cudzego martwego kodu bez prośby).
- Każda zmieniona linia powinna wynikać bezpośrednio z polecenia.

### 1.4. Goal-Driven Execution
- Określ kryteria sukcesu i sprawdzaj, aż zostaną spełnione.
- Zamieniaj zadania na mierzalne cele (np. konkretne przypadki brzegowe i sprawdzenia wizualne/matematyczne po każdym kroku).
- Dla zadań wieloetapowych podaj krótki plan z weryfikacją:
  1. [Krok] -> weryfikacja: [sprawdzenie]
  2. [Krok] -> weryfikacja: [sprawdzenie]

### Dodatkowe rady pod ten projekt (.NET 9 + WinForms + grafika 3D)
- **First principles:** opieraj obliczenia geometryczne, macierze obrotu, współrzędne barycentryczne, Z-buffer i oświetlenie na wzorach z zadania oraz prezentacji `03a_lab2_wstep_3d.pdf`, nie na zgadywaniu[cite: 4, 19, 20].
- **Krótkie komentarze:** pisz zwięzłe komentarze po polsku (małą literą) tam, gdzie wyjaśniają nietrywialne założenia (np. odwrócenie osi Y, kolejność 16 punktów w siatce 4x4, konwencja wektorów kolumnowych w `Matrix3x3`).

## 2. Architektura, katalogi i zasady OOP
- Stack: **.NET 9** (`net9.0-windows`, Visual Studio 2022) + **WinForms** (`System.Numerics.Vector3` dla punktów i wektorów), z zachowaniem prostego modelu ułatwiającego późniejszą migrację na **.NET 10**[cite: 20].
- Struktura katalogów projektu:
  - `src/` — kod aplikacji WinForms,
  - `data/` — pliki wejściowe z powierzchniami Béziera (oraz w późniejszych etapach tekstury / normal mapy),
  - `docs/` — dokumentacja i notatki projektowe.
- **Czyste OOP bez over-engineeringu:** nie twórz interfejsów (`I...`), fabryk, strategii ani rozbudowanych hierarchii, jeśli nie ma realnego powodu na polimorfizm. Używaj zwykłych klas i struktur o jasnym celu.
- **Separacja UI od logiki i danych:** wczytywanie danych, reprezentacja powierzchni/siatki, obrót, oświetlenie, Z-buffer i rasteryzacja muszą być odseparowane od kontrolek `Form`. Kontrolka rysująca obsługuje wyświetlanie i interakcję, a nie ciężką logikę domenową.
- **Pilnowanie `*.Designer.cs` i podglądu UI w Visual Studio:** w plikach `*.Designer.cs` trzymaj wyłącznie kod inicjalizacji i układu kontrolek (`InitializeComponent`) — nigdy nie wklejaj tam logiki domenowej ani matematycznej, aby nie zepsuć projektanta formularzy w Visual Studio. Przed większą edycją formularza zamknij zakładkę Designera w VS, a po dodaniu kontrolek lub zmianie rysowania sprawdź przeładowanie widoku w Designerze oraz uruchom aplikację w celu weryfikacji renderowania na żywo.

## 3. Protokół aktualizacji `SKILL.md` po każdym slice (OBOWIĄZKOWY)
Po zakończeniu i uruchomieniu danego slice'a zaktualizuj ten plik (`SKILL.md`), dopisując wpis w sekcji **Dziennik slice'ów i decyzji** zawierający:
1. **Status slice'a:** co zostało zaimplementowane i w jakich plikach.
2. **Przyjęte założenia:** wszystkie decyzje podjęte w tym kroku (np. mapowanie czułości myszy, kolejność składania obrotów, detale siatki).
3. **Weryfikacja i przypadki brzegowe:** co należy sprawdzić ręcznie przy uruchomieniu aplikacji oraz jakie potencjalne edge case'y wykryto.
4. **Punkt wyjścia do kolejnego slice'a:** zwięzłe podsumowanie gotowego stanu przed przejściem do następnego etapu.

## 4. Model danych i wspólne założenia sceny
- **Punkt i wektor:** trzy współrzędne `x, y, z` (`System.Numerics.Vector3`)[cite: 20].
- **Wierzchołek** przechowuje:
  - punkt `P`, wektory styczne `Pu` i `Pv` oraz wektor normalny `N` **przed obrotem**[cite: 20],
  - punkt `P`, wektory styczne `Pu` i `Pv` oraz wektor normalny `N` **po obrocie**[cite: 20],
  - parametry `u` i `v`, na podstawie których wyznaczono wierzchołek trójkąta[cite: 20].
- **Trójkąt:** trzy wierzchołki[cite: 20].
- **Siatka:** lista trójkątów (oraz lista współdzielonych wierzchołków do sprawnego obracania)[cite: 2, 20].
- **Układ współrzędnych sceny i panelu:**
  - powierzchnie Béziera 3. stopnia wczytywane są jako grupy po **16 wierszy** (`x y z`), co najmniej **2 powierzchnie** w pliku,
  - każda powierzchnia jest funkcją względem płaszczyzny `xy`, a bounding box punktów kontrolnych jest zbliżony do sześcianu wokół `(0, 0, 0)` tak, aby powierzchnie mieściły się na obszarze rysowania przy dowolnym obrocie wokół środka,
  - obszar rysowania to panel WinForms z `DoubleBuffered = true`, początkiem układu współrzędnych `(0, 0)` na jego środku i osią `Y` skierowaną w górę.

## 5. Aktualny etap: SLICE 2 — Triangulacja powierzchni Béziera, obrót myszą i rzut na płaszczyznę `xy`

### Cel Slice 2
Rozszerzyć aplikację o generowanie siatki trójkątów (triangulację) dla każdej wczytanej powierzchni Béziera 3. stopnia wraz z wektorami stycznymi i normalnymi, dodać narastający obrót 3D sceny za pomocą przeciągania myszą (LPM + PPM), rzut prostokątny na płaszczyznę `xy` oraz kontrolki UI (suwak dokładności triangulacji 4–40, domyślnie 16, i checkboxy widoczności)[cite: 19, 20].

### Zakres prac w Slice 2
1. **Model siatki (`Vertex`, `Triangle`, `Mesh`):**
   - utwórz proste klasy/struktury zgodne z sekcją 4:
     - `Vertex` (klasa, aby sąsiednie trójkąty współdzieliły wierzchołki) przechowująca `P`, `Pu`, `Pv`, `N` **przed obrotem** oraz `P`, `Pu`, `Pv`, `N` **po obrocie**, a także parametry `U`, `V`[cite: 20],
     - `Triangle` z trzema wierzchołkami (`V0`, `V1`, `V2`)[cite: 20],
     - `Mesh` z listą wierzchołków i listą trójkątów[cite: 20].
2. **Triangulacja powierzchni Béziera 3. stopnia ($n = 3, m = 3$):**
   - wyznaczaj dla zadanej gęstości podziału $k \times k$ w dziedzinie $(u, v) \in [0, 1]^2$ (siatka $(k + 1) \times (k + 1)$ wierzchołków dla $u_r = r / k$, $v_c = c / k$):
     - punkt powierzchni: $P(u, v) = \sum_{i=0}^{3} \sum_{j=0}^{3} V_{i,j} \cdot B_{i,3}(u) \cdot B_{j,3}(v)$[cite: 20],
     - wektory styczne:
       - $P_u(u, v) = 3 \sum_{i=0}^{2} \sum_{j=0}^{3} (V_{i+1,j} - V_{i,j}) \cdot B_{i,2}(u) \cdot B_{j,3}(v)$[cite: 20],
       - $P_v(u, v) = 3 \sum_{i=0}^{3} \sum_{j=0}^{2} (V_{i,j+1} - V_{i,j}) \cdot B_{i,3}(u) \cdot B_{j,2}(v)$[cite: 20],
     - wektor normalny: $N(u, v) = \text{normalize}(P_u(u, v) \times P_v(u, v))$[cite: 20],
   - dziel każde oczko siatki $(r, c)$ na 2 trójkąty; przy zmianie wartości suwaka generuj siatkę na nowo i od razu zastosuj do niej aktualną macierz orientacji sceny (bez resetowania obrotu).
3. **Własna macierz `Matrix3x3` i narastający obrót 3D myszą (LPM + PPM):**
   - utwórz prostą strukturę `Matrix3x3` zgodną z konwencją wektorów kolumnowych $P' = R \cdot P$ ze wzorów zadania (`Identity`, mnożenie macierzy `A * B`, mnożenie macierz × wektor `R.Transform(Vector3)` oraz fabryki `CreateRotationX`, `CreateRotationY`, `CreateRotationZ`)[cite: 4, 19],
   - przechowuj aktualną macierz orientacji sceny $R$ (początkowo `Matrix3x3.Identity`)[cite: 19],
   - obracaj scenę przez przeciąganie kursora myszy w `DrawingPanel` (`MouseDown`, `MouseMove`, `MouseUp` — nie przyciskami)[cite: 19]:
     - **LPM (`MouseButtons.Left`):** ruch poziomy (`dx`) obraca wokół osi `Y`, ruch pionowy (`dy`) obraca wokół osi `X`,
     - **PPM (`MouseButtons.Right`):** ruch poziomy (`dx`) obraca wokół osi `Z` (roll),
   - **nie ograniczaj wartości kątów obrotu**[cite: 19],
   - wyznaczaj obrót **narastająco**: dla każdego przesunięcia kursora od poprzedniej pozycji zbuduj macierz przyrostu $R_{\text{przyrost}}$ i złóż ją z lewej strony z aktualną orientacją: $R_{\text{nowa}} = R_{\text{przyrost}} \cdot R_{\text{aktualna}}$[cite: 19],
   - obracaj oryginalne (`przed obrotem`) wartości wierzchołków siatki: `P`, `Pu`, `Pv`, `N` (z ponowną normalizacją `N` po obrocie) wokół `(0, 0, 0)`[cite: 2, 19],
   - obracaj również punkty kontrolne wieloboku Béziera tą samą macierzą $R$, aby wielobok i siatka trójkątów obracały się spójnie.
4. **Rzut prostokątny na płaszczyznę `xy` i kontrolki UI:**
   - rysuj rzut prostokątny na płaszczyznę `xy` (współrzędne `x, y` po obrocie, z wykorzystaniem istniejącego `DrawingPanel.ToScreen`)[cite: 19],
   - dodaj w `MainForm` panel sterowania (w `MainForm.Designer.cs` wyłącznie definicje kontrolek):
     - suwak (`TrackBar`) do regulacji dokładności triangulacji: **zakres `4`–`40`, wartość domyślna `16`**, wraz z etykietą wyświetlającą aktualną liczbę podziałów[cite: 19],
     - pola wyboru (`CheckBox`) umożliwiające niezależne włączanie/wyłączanie rysowania:
       - wieloboków Béziera (domyślnie zaznaczone)[cite: 19],
       - siatki trójkątów (domyślnie zaznaczone)[cite: 19].

### Czego NIE robić w Slice 2
- Nie implementuj jeszcze wypełniania trójkątów (scanline / sortowanie krawędzi lub wierzchołków), Z-buffera, modelu oświetlenia Phonga/Lamberta, tekstur ani mapy normalnych — te elementy trafią do kolejnych slice'ów.
- Nie dodawaj jeszcze nieaktywnego/martwego checkboxa „Wypełnione trójkąty” (dodamy go w Slice 3 razem z rasteryzacją).
- Nie używaj `System.Numerics.Matrix4x4` (stosujemy ustaloną strukturę `Matrix3x3`).
- Nie twórz zbędnych abstrakcji ani interfejsów.

### Po zakończeniu Slice 2 (obowiązkowy wpis do `SKILL.md`)
Dopisz w sekcji **Dziennik slice'ów i decyzji** wpis `Slice 2`:
- listę utworzonych/zmodyfikowanych plików w `src/`,
- przyjęte założenia (czułość ruchu myszy na radiany, kolejność składania $R_x, R_y$, kolejność przekątnej przy podziale oczka na 2 trójkąty),
- wyniki weryfikacji:
  - płynna zmiana gęstości siatki suwakiem w zakresie 4–40 (domyślnie 16) przy zachowaniu aktualnego obrotu sceny,
  - płynny, narastający obrót myszą (LPM: X/Y, PPM: Z) o ponad 360° bez blokady i bez „skakania” orientacji,
  - spójność położenia wieloboków kontrolnych względem siatki trójkątów po obrocie (widoczna głębia 3D z `data/surfaces.txt`),
  - poprawne działanie checkboxów widoczności,
  - poprawność ładowania `MainForm` w Designerze Visual Studio 2022,
- potwierdzenie gotowości do przejścia do **Slice 3**.

## 6. Dziennik slice'ów i decyzji
- **Decyzja 1:** Start na **.NET 9** (`net9.0-windows`, Visual Studio 2022) + **WinForms** z planem łatwej migracji `TargetFramework` na **.NET 10** w przyszłości; praca małymi vertical slices z aktualizacją `SKILL.md` po każdym etapie.
- **Decyzja 2 (Obszar rysowania):** Własny panel WinForms z `DoubleBuffered = true`, początkiem układu współrzędnych `(0, 0)` na środku kontrolki oraz osią `Y` skierowaną w górę.
- **Decyzja 3 (Katalog danych):** Pliki z powierzchniami Béziera (min. 2 powierzchnie 3. stopnia, grupy po 16 wierszy `x y z`) umieszczane są w katalogu `data/` obok `docs/` i `src/`.
- **Decyzja 4 (Slice 2 — macierze, mysz i triangulacja):** Własna struktura `Matrix3x3` z konwencją wektorów kolumnowych ($P' = R \cdot P$, składanie narastające $R_{\text{nowa}} = R_{\text{przyrost}} \cdot R_{\text{aktualna}}$ — przydatna również później przy macierzy przekształcenia NormalMap)[cite: 19, 22]; obrót myszą dwoma przyciskami (**LPM** dla osi `X` i `Y`, **PPM** dla osi `Z`); suwak dokładności triangulacji w zakresie **4–40** z wartością domyślną **16**.
- *(Wpisy z realizacji kolejnych slice'ów będą dopisywane po wykonaniu każdego etapu.)*

### Slice 1 — wykonany (2026-11-22)
1. **Status / pliki** (`src/BezierSceneApp/`):
   - `BezierSurface.cs` — siatka `Vector3[4,4]` punktów kontrolnych,
   - `SurfaceFileReader.cs` — statyczne `Load(path)` → `List<BezierSurface>` (kolekcja powierzchni sceny to zwykła lista, bez osobnej klasy `Scene`),
   - `DrawingPanel.cs` — `Panel` z `DoubleBuffered = true`, `ResizeRedraw = true`, rysuje wieloboki kontrolne i pomocnicze osie,
   - `MainForm.cs` / `MainForm.Designer.cs` / `MainForm.resx` — zmiana nazwy z `Form1`; Designer zawiera tylko `drawingPanel` (`Dock = Fill`),
   - `Program.cs` — wczytanie danych przed `Application.Run`,
   - `BezierSceneApp.csproj` — kopiowanie `../../data/**` do `bin/.../data`; `BezierSceneApp.csproj.user` — zaktualizowane `SubType`,
   - `data/surfaces.txt` — 2 powierzchnie: „falista” (x ∈ [-0.75, -0.15]) i „kopuła” (x ∈ [0.15, 0.75]), y ∈ [-0.5, 0.5], |z| ≤ 0.3.
2. **Przyjęte założenia:**
   - plik: `data/surfaces.txt`, odczyt z `AppContext.BaseDirectory/data/` (kopiowany przez csproj przy buildzie),
   - format: wiersz = 3 liczby oddzielone białymi znakami (spacja/tab), separator dziesiętny `.` (`InvariantCulture`); przecinek NIE jest obsługiwany; puste linie pomijane (mogą rozdzielać powierzchnie),
   - indeksowanie: k-ty punkt grupy (k = 0..15) → `ControlPoints[k / 4, k % 4]` (wiersz-major),
   - walidacja: liczba punktów musi być wielokrotnością 16 i dawać ≥ 2 powierzchnie; błąd → `MessageBox` z numerem wiersza/opisem i zamknięcie aplikacji,
   - jednostki sceny ~[-1, 1]; skala = `0.45 * min(Width, Height)` px (promień 1 mieści się w panelu przy każdym obrocie),
   - rzut prostokątny na płaszczyznę `xy` (z pomijane), `screenX = W/2 + x*s`, `screenY = H/2 - y*s`,
   - kolory powierzchni z cyklicznej tablicy 4 kolorów; pomocnicze osie `+X`/`+Y` do weryfikacji układu.
3. **Weryfikacja:** build bez ostrzeżeń; aplikacja startuje, wczytuje 2 powierzchnie, rysuje 2 siatki 4x4 symetrycznie wokół środka; osie przecinają się w środku panelu, `+Y` u góry; przy zmianie rozmiaru brak migotania, skala się dopasowuje.
   - Edge case'y: brak pliku / zły format / 1 powierzchnia / niepełna grupa → komunikat i wyjście; przy rzucie na `xy` siatki wyglądają płasko (różnice w `z` będą widoczne dopiero po obrocie w kolejnych slice'ach).
4. **Punkt wyjścia do Slice 2:** gotowy model 4x4 punktów kontrolnych, loader i panel z transformacją scena → ekran (`DrawingPanel.ToScreen`). Gotowe do przejścia do **Slice 2**.

### Slice 2 — wykonany (2026-11-22)
1. **Status / pliki** (`src/BezierSceneApp/`):
   - `Matrix3x3.cs` (nowy) — `readonly struct`, wektory kolumnowe `P' = R·P`: `Identity`, `A * B`, `Transform(Vector3)`, `CreateRotationX/Y/Z`,
   - `Mesh.cs` (nowy) — klasy `Vertex` (`P, Pu, Pv, N` + `RotP, RotPu, RotPv, RotN`, `U, V`, `Rotate(R)` z normalizacją `RotN`), `Triangle` (`V0, V1, V2`), `Mesh` (`Vertices`, `Triangles`, `Rotate(R)`),
   - `BezierSurface.cs` — `Triangulate(k)` (wzory na `P`, `Pu`, `Pv`, `N` z Bernsteinem st. 3 i 2), `RotatedControlPoints` + `Rotate(R)`,
   - `Scene.cs` (nowy) — stan sceny: `Surfaces`, `Meshes`, `Orientation`; `SetDivisions(k)` (nowa siatka + aktualna orientacja), `Rotate(przyrost)` (`R = przyrost · R`); zawsze obracane są wartości oryginalne,
   - `DrawingPanel.cs` — właściwości `Scene`, `ShowPolygons`, `ShowMesh`; obrót myszą w `OnMouseDown/OnMouseMove`; rysowanie siatki (krawędzie trójkątów) i obróconych wieloboków,
   - `MainForm.cs` / `MainForm.Designer.cs` — `controlPanel` (`Dock = Right`, 200 px): `divisionsLabel`, `divisionsTrackBar` (4–40, domyślnie 16), `showPolygonsCheckBox`, `showMeshCheckBox` (oba zaznaczone); w Designerze tylko kontrolki i podpięcie zdarzeń.
2. **Przyjęte założenia:**
   - czułość myszy: **0.01 rad/px**; kąty nieograniczone,
   - LPM: `R_przyrost = Rx(dy) · Ry(dx)`; `dx > 0` (w prawo) → przód sceny w prawo, `dy > 0` (ekranowo w dół) → przód sceny w dół,
   - PPM: `R_przyrost = Rz(-dx)` — ruch w prawo obraca scenę zgodnie z ruchem wskazówek zegara,
   - triangulacja: `u ↔ i` (wiersz `ControlPoints`), `v ↔ j` (kolumna); oczko `(r, c)` dzielone przekątną `(r,c)–(r+1,c+1)`: `[(r,c),(r+1,c),(r+1,c+1)]` i `[(r,c),(r+1,c+1),(r,c+1)]`; wierzchołki współdzielone,
   - zdegenerowane naroże (`|Pu × Pv| ≈ 0`) → `N = (0, 0, 1)`,
   - kolory: wielobok Béziera szary (`Gray`), siatka trójkątów zielona (`LimeGreen`); siatka rysowana pod wielobokiem; wspólne kolory dla wszystkich powierzchni (tablica kolorów z Slice 1 usunięta).
3. **Weryfikacja i przypadki brzegowe:**
   - build bez ostrzeżeń; aplikacja startuje (smoke test); pozostałe punkty do ręcznego sprawdzenia: suwak 4–40 przebudowuje siatkę bez resetu obrotu, obrót LPM/PPM ponad 360° bez skoków, wielobok i siatka obracają się spójnie (narożniki siatki = narożne punkty kontrolne), checkboxy niezależnie ukrywają warstwy, `MainForm` ładuje się w Designerze VS 2022,
   - edge case: dla `data/surfaces.txt` normalne `Pu × Pv` wskazują w stronę **−z** (u rośnie wzdłuż `y`, v wzdłuż `x`) — istotne przy oświetleniu w Slice 3,
   - edge case: macierz `Orientation` jest składana z tysięcy przyrostów — możliwy powolny dryf ortogonalności (float); na razie bez reortonormalizacji,
   - przy k = 40 i antyaliasingu rysowanie ~6400 krawędzi — do obserwacji płynności.
4. **Punkt wyjścia do Slice 3:** siatki z wektorami `P, Pu, Pv, N` (przed/po obrocie), narastająca orientacja `Matrix3x3`, panel sterowania gotowy na kolejne kontrolki (np. „Wypełnione trójkąty”). Gotowe do przejścia do **Slice 3**.