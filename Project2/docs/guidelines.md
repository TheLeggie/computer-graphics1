---
name: bezier-scene-winforms
description: Zasady pracy, architektury, Karpathy guidelines, etapy slice-by-slice i protokol aktualizacji projektu powierzchni Beziera 3. stopnia w .NET 9 (docelowo .NET 10) i WinForms.
---

# Bezier Scene (.NET 9 / .NET 10 + WinForms) — Project Guidelines

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
- **First principles:** opieraj obliczenia geometryczne, macierze obrotu, współrzędne barycentryczne, Z-buffer i oświetlenie na wzorach z zadania oraz prezentacji `03a_lab2_wstep_3d.pdf`, nie na zgadywaniu.
- **Krótkie komentarze:** pisz zwięzłe komentarze po polsku (małą literą) tam, gdzie wyjaśniają nietrywialne założenia (np. odwrócenie osi Y, konwencja znaku Z w Z-bufferze, obsługa dwustronności normalnych).

## 2. Architektura, katalogi i zasady OOP
- Stack: **.NET 9** (`net9.0-windows`, Visual Studio 2022) + **WinForms** (`System.Numerics.Vector3` dla punktów i wektorów), z zachowaniem prostego modelu ułatwiającego późniejszą migrację na **.NET 10**.
- Struktura katalogów projektu:
  - `src/` — kod aplikacji WinForms,
  - `data/` — pliki wejściowe z powierzchniami Béziera (oraz w późniejszych etapach tekstury / normal mapy),
  - `docs/` — dokumentacja i notatki projektowe.
- **Czyste OOP bez over-engineeringu:** nie twórz interfejsów (`I...`), fabryk, strategii ani rozbudowanych hierarchii, jeśli nie ma realnego powodu na polimorfizm. Używaj zwykłych klas i struktur o jasnym celu.
- **Separacja UI od logiki i danych:** wczytywanie danych, reprezentacja powierzchni/siatki, obrót, oświetlenie, Z-buffer i rasteryzacja muszą być odseparowane od kontrolek `Form`. Kontrolka rysująca obsługuje wyświetlanie i interakcję, a nie ciężką logikę domenową.
- **Pilnowanie `*.Designer.cs` i podglądu UI w Visual Studio:** w plikach `*.Designer.cs` trzymaj wyłącznie kod inicjalizacji i układu kontrolek (`InitializeComponent`) — nigdy nie wklejaj tam logiki domenowej ani matematycznej, aby nie zepsuć projektanta formularzy w Visual Studio. Przed większą edycją formularza zamknij zakładkę Designera w VS, a po dodaniu kontrolek lub zmianie rysowania sprawdź przeładowanie widoku w Designerze oraz uruchom aplikację w celu weryfikacji renderowania na żywo.

## 3. Protokół aktualizacji `guidelines.md` po każdym slice (OBOWIĄZKOWY)
Po zakończeniu i uruchomieniu danego slice'a zaktualizuj ten plik (`guidelines.md`), dopisując wpis w sekcji **Dziennik slice'ów i decyzji** zawierający:
1. **Status slice'a:** co zostało zaimplementowane i w jakich plikach.
2. **Przyjęte założenia:** wszystkie decyzje podjęte w tym kroku (np. domyślne wartości $k_d, k_s, m, z_L$, sposób przekazywania atrybutów wierzchołków do rasteryzera, obsługa dwustronności).
3. **Weryfikacja i przypadki brzegowe:** co należy sprawdzić ręcznie przy uruchomieniu aplikacji oraz jakie potencjalne edge case'y wykryto (w tym wnioski z review / LLM-as-a-judge).
4. **Punkt wyjścia do kolejnego slice'a:** zwięzłe podsumowanie gotowego stanu przed przejściem do następnego etapu.

## 4. Model danych i wspólne założenia sceny
- **Punkt i wektor:** trzy współrzędne `x, y, z` (`System.Numerics.Vector3`).
- **Wierzchołek** przechowuje:
  - punkt `P`, wektory styczne `Pu` i `Pv` oraz wektor normalny `N` **przed obrotem**,
  - punkt `P`, wektory styczne `Pu` i `Pv` oraz wektor normalny `N` **po obrocie**,
  - parametry `u` i `v`, na podstawie których wyznaczono wierzchołek trójkąta.
- **Trójkąt:** trzy wierzchołki.
- **Siatka:** lista trójkątów (oraz lista współdzielonych wierzchołków do sprawnego obracania).
- **Układ współrzędnych sceny i panelu:**
  - powierzchnie Béziera 3. stopnia wczytywane są jako grupy po **16 wierszy** (`x y z`), co najmniej **2 powierzchnie** w pliku,
  - układ 3D jest prawoskrętny: `+X` w prawo, `+Y` w górę, `+Z` w stronę obserwatora (wektor do obserwatora $V = [0, 0, 1]$),
  - obszar rysowania to panel WinForms z `DoubleBuffered = true`, początkiem układu współrzędnych `(0, 0)` na jego środku i osią `Y` skierowaną w górę.

## 5. Aktualny etap: SLICE 6 — Animowane i edytowalne źródło światła na zamkniętej krzywej Béziera

### Cel Slice 6

Zastąpić statyczne źródło światła $(0, 0, z_L)$ źródłem poruszającym się animacyjnie po zadanej **zamkniętej krzywej Béziera** (7 punktów kontrolnych $V_0 \dots V_6$, $V_6 = V_0$) leżącej w płaszczyźnie $z = \text{const}$, z możliwością zatrzymywania/wznawiania animacji, regulacji wysokości $z$ istniejącym suwakiem, interaktywnej edycji punktów kontrolnych krzywej myszą oraz widocznym znacznikiem aktualnego położenia źródła światła na krzywej.

### Zakres prac w Slice 6

#### 1. Model zamkniętej krzywej Béziera (`Model/LightBezierCurve.cs`)

- Utwórz prostą klasę `LightBezierCurve` reprezentującą pojedynczą zamkniętą krzywą Béziera stopnia $n = 6$ (7 punktów kontrolnych $V_0 \dots V_6$ w płaszczyźnie $xy$, z niezmiennikiem $V_6 = V_0$):
  - przechowuje 7 punktów `Vector2` (jednostki sceny), przy czym 6 punktów $V_0 \dots V_5$ jest niezależnych, a $V_6$ jest zawsze równe $V_0$,
  - metoda `SetPoint(int index, Vector2 value)` aktualizuje punkt `index`; dla `index == 0` (lub `6`) ustawia jednocześnie oba końce (`points[0] = points[6] = value`), dzięki czemu krzywa zawsze pozostaje zamknięta,
  - **domyślna krzywa:** 7 punktów rozłożonych w zamkniętą pętlę wokół środka sceny (np. promień $\approx 0.75\text{–}0.85$ w jednostkach sceny, z $V_6 = V_0$), tak aby światło wyraźnie omiatało obie powierzchnie z `data/surfaces.txt` i cała krzywa mieściła się w panelu.
- **Obliczanie punktu na krzywej — algorytm de Casteljau:**
  - `Evaluate(float t)` dla $t \in [0, 1]$ zwraca `Vector2` na płaszczyźnie $xy$ (z reużywaną tablicą roboczą `Vector2[7]` lub `stackalloc`, tj. **0 alokacji sterty** na wywołanie),
  - dla $t = 0$ daje dokładnie $V_0$, dla $t = 1$ daje dokładnie $V_6 = V_0$,
  - `Evaluate3D(float t, float z)` zwraca `new Vector3(p.X, p.Y, z)`.
- Krzywa leży na płaszczyźnie $z = \text{const}$ w układzie obserwatora (nie obraca się razem z `Scene.Orientation`), spójnie z `LightSettings.Position` z Slice 4.

#### 2. Animacja źródła światła i integracja z suwakiem `z`

- Dodaj w `MainForm` komponent `System.Windows.Forms.Timer` (`lightAnimationTimer`, `Interval = 33` ms $\approx$ 30 FPS):
  - **ważne dla WinForms Designera:** w `MainForm.Designer.cs` ustaw `Enabled = false`, a animację uruchamiaj dopiero w konstruktorze uruchomieniowym `MainForm(List<BezierSurface> surfaces)`, aby timer nie tykał w projektancie Visual Studio,
  - w każdym tyknięciu zwiększaj parametr animacji $t$ o stały krok $\Delta t$ (np. $\Delta t = 0.005$, pełny obieg w $\approx 6.6\text{ s}$), zawijając go cyklicznie `t = (t + dt) % 1f`, przeliczaj `Light.Position = LightCurve.Evaluate3D(t, z)` i wywołuj `drawingPanel.Invalidate()`.
- **Poprawka suwaka `z` światła (`lightingTrackBar_ValueChanged`):**
  - zmiana suwaków `kd`, `ks`, `m` lub `z światła` **nie może** resetować `X` i `Y` światła do `(0, 0)` — zachowaj bieżące `(x(t), y(t))` na krzywej i aktualizuj wyłącznie składową `Z` (`light.Position = lightCurve.Evaluate3D(lightT, lightZTrackBar.Value / 10f)`).
- Pozycja światła musi być ustawiona na krzywej już w chwili startu aplikacji (zanim nastąpi pierwsze rysowanie).

#### 3. Wizualizacja krzywej i położenia światła (`DrawingPanel.cs`)

- Kolejność warstw w `OnPaint`: tło $\rightarrow$ wypełnione trójkąty $\rightarrow$ siatka $\rightarrow$ wieloboki kontrolne powierzchni $\rightarrow$ osie $\rightarrow$ **krzywa Béziera światła, wielobok kontrolny, uchwyty punktów kontrolnych i marker aktualnego źródła światła** (rysowane na wierzchu, aby zawsze były czytelne i dostępne do edycji).
- **Rysowanie toru światła** (gdy `ShowLightCurve == true`):
  - wielobok kontrolny krzywej światła ($V_0 \to V_1 \to \dots \to V_6$): cienka przerywana linia pomocnicza,
  - krzywa Béziera: łamana z np. 64–100 próbek dla $t \in [0, 1]$ (`DrawLines`, wyraźny kolor, np. `DarkOrange`),
  - uchwyty punktów kontrolnych $V_0 \dots V_5$ (6 widocznych uchwytów, bo $V_6 = V_0$): wyraźne kółka (promień ~5 px z obwódką; wyróżnienie aktualnie przeciąganego punktu).
- **Marker aktualnego położenia źródła światła:**
  - wyraźne kółko (np. promień ~7 px, wypełnione kolorem `Light.LightColor` z ciemną obwódką) rysowane w `ToScreen(Light.Position)` — widoczne na krzywej w czasie animacji i po zatrzymaniu.

#### 4. Interaktywna edycja krzywej myszą (`DrawingPanel.cs`)

- Dodaj odwrotną transformację `FromScreen(Point p)` $\rightarrow$ `Vector2` w jednostkach sceny (z zabezpieczeniem przed `scale <= 0`):
  $$x = \frac{p_x - W/2}{\text{scale}}, \qquad y = \frac{H/2 - p_y}{\text{scale}}$$
- **Automatyczne rozdzielenie LPM (edycja punktu vs obrót sceny):**
  - w `OnMouseDown` (dla `MouseButtons.Left`): jeśli `ShowLightCurve == true`, sprawdź odległość ekranową kursora od uchwytów $V_0 \dots V_5$ (promień trafienia np. `10 px`, wybór najbliższego w promieniu),
  - jeśli trafiono uchwyt `i`: zapamiętaj `draggedControlPoint = i` i przejdź w tryb przeciągania punktu (nie obracaj sceny),
  - jeśli nie trafiono żadnego uchwytu: zachowaj dotychczasowy obrót sceny LPM (PPM bez zmian — obrót wokół osi `Z`).
- w `OnMouseMove`:
  - gdy `draggedControlPoint >= 0` i wciśnięty LPM: przelicz pozycję myszy przez `FromScreen(e.Location)`, wywołaj `LightCurve.SetPoint(draggedControlPoint, pt)`, natychmiast zaktualizuj `Light.Position` dla bieżącego parametru $t$ (tak aby nawet przy zatrzymanej animacji światło i cieniowanie powierzchni na żywo podążały za krzywą!) i wywołaj `Invalidate()`.
- w `OnMouseUp`: zresetuj `draggedControlPoint = -1`.

#### 5. Kontrolki UI (`MainForm.Designer.cs` + `MainForm.cs`)

W `controlPanel` (czystość `MainForm.Designer.cs` — wyłącznie układ kontrolek, `lightAnimationTimer` z `Enabled = false` i podpięcie zdarzeń):

- `CheckBox` „Animacja światła” (`animateLightCheckBox`, domyślnie zaznaczony) — zatrzymuje / wznawia `lightAnimationTimer`,
- `CheckBox` „Pokaż krzywą światła” (`showLightCurveCheckBox`, domyślnie zaznaczony) — włącza / wyłącza rysowanie krzywej i uchwytów (gdy wyłączony, LPM nie chwyta ukrytych punktów kontrolnych).

### Czego NIE robić w Slice 6

- Nie dodawaj dodawania/usuwania punktów kontrolnych krzywej — stałe 7 punktów ($V_0 \dots V_6$, $V_6 = V_0$).
- Nie dodawaj osobnej edycji współrzędnej `z` per punkt krzywej — krzywa leży na płaszczyźnie $z = \text{const}$ sterowanej suwakiem.
- Nie obracaj krzywej światła razem z `Scene.Orientation`.
- Nie zmieniaj algorytmu rasteryzacji AET, Z-buffera ani modelu oświetlenia/NormalMap.
- Nie ustawiaj `lightAnimationTimer.Enabled = true` w `MainForm.Designer.cs`.

### Review / LLM-as-a-judge (OBOWIĄZKOWY w Slice 6)

Po napisaniu kodu przeprowadź niezależną weryfikację (*review* / *judge*) pod kątem najczęstszych błędów w animacji i edycji krzywej Béziera:

1. **Domknięcie krzywej ($V_6 = V_0$) i de Casteljau:** czy krzywa ma co najmniej 7 punktów ($V_0 \dots V_6$), czy `Evaluate(0) == V_0` i `Evaluate(1) == V_6 == V_0` (brak skoku przy zawijaniu $t: 1 \to 0$), oraz czy przesunięcie $V_0$ aktualizuje jednocześnie $V_6$?
2. **Spójność współrzędnych i suwaka `z`:** czy `ToScreen(FromScreen(p))` wraca do punktu `p` (poprawny znak osi `Y`), czy zmiana suwaków `z`, `kd`, `ks`, `m` zachowuje bieżące `(x(t), y(t))` światła zamiast zerować je do `(0, 0)`, oraz czy marker światła zawsze leży dokładnie na narysowanej krzywej?
3. **Edycja myszą vs obrót sceny:** czy LPM na uchwycie przesuwa wyłącznie ten punkt bez obracania sceny, LPM poza uchwytami obraca scenę jak dotąd, PPM zawsze obraca wokół `Z`, a po odznaczeniu „Pokaż krzywą światła” nie da się przypadkowo chwycić niewidocznego uchwytu?
4. **Aktualizacja na żywo przy zatrzymanej animacji:** czy po zatrzymaniu animacji i przeciągnięciu punktu kontrolnego (lub zmianie suwaka `z`) pozycja markera i cieniowanie powierzchni odświeżają się natychmiast?
5. **Zasoby, alokacje i Designer:** czy `Evaluate` nie alokuje tablic na stercie, czy `lightAnimationTimer` jest wyłączony w `MainForm.Designer.cs` i poprawnie zwalniany przy zamknięciu okna, oraz czy `MainForm.Designer.cs` nie zawiera logiki domenowej?

### Po zakończeniu Slice 6 (obowiązkowy wpis do `guidelines.md`)

Dopisz w sekcji **Dziennik slice'ów i decyzji** wpis Slice 6:

- listę utworzonych/zmodyfikowanych plików w `src/`,
- przyjęte założenia (domyślne współrzędne 7 punktów krzywej, interwał timera i krok $\Delta t$, promień trafienia uchwytu, zachowanie widoczności markera po ukryciu krzywej),
- wyniki weryfikacji (w tym podsumowanie review/judge):
  - płynny, cykliczny ruch światła po zamkniętej krzywej bez skoku w punkcie $V_0 = V_6$ (widoczne przemieszczanie się odblasków i cieni na obu powierzchniach, w tym z włączoną NormalMap),
  - zatrzymywanie i wznawianie animacji oraz płynna zmiana wysokości $z$ suwakiem bez przeskoku `X/Y`,
  - edycja każdego z 6 uchwytów ($V_0 = V_6$ oraz $V_1 \dots V_5$) bez konfliktu z obrotem sceny LPM/PPM, zarówno podczas animacji, jak i po jej zatrzymaniu,
  - poprawność ładowania `MainForm` w Designerze Visual Studio 2022,
- podsumowanie ukończenia wszystkich wymagań projektu (Slice 1–6).

## 6. Dziennik slice'ów i decyzji
- **Decyzja 1:** Start na **.NET 9** (`net9.0-windows`, Visual Studio 2022) + **WinForms** z planem łatwej migracji `TargetFramework` na **.NET 10** w przyszłości; praca małymi vertical slices z aktualizacją `guidelines.md` po każdym etapie.
- **Decyzja 2 (Obszar rysowania):** Własny panel WinForms z `DoubleBuffered = true`, początkiem układu współrzędnych `(0, 0)` na środku kontrolki oraz osią `Y` skierowaną w górę.
- **Decyzja 3 (Katalog danych):** Pliki z powierzchniami Béziera (min. 2 powierzchnie 3. stopnia, grupy po 16 wierszy `x y z`) umieszczane są w katalogu `data/` obok `docs/` i `src/`.
- **Decyzja 4 (Slice 2 — macierze, mysz i triangulacja):** Własna struktura `Matrix3x3` z konwencją wektorów kolumnowych ($P' = R \cdot P$, składanie narastające $R_{\text{nowa}} = R_{\text{przyrost}} \cdot R_{\text{aktualna}}$ — przydatna również później przy macierzy przekształcenia NormalMap); obrót myszą dwoma przyciskami (**LPM** dla osi `X` i `Y`, **PPM** dla osi `Z`); suwak dokładności triangulacji w zakresie **4–40** z wartością domyślną **16**.
- **Decyzja 5 (Podział rasteryzacji i oświetlenia na Slice 3 i Slice 4):**
  - **Slice 3:** wypełnianie trójkątów ogólnym algorytmem **z sortowaniem wierzchołków i tablicą aktywnych krawędzi (AET)** do bufora `int[]` + `LockBits`, współrzędne barycentryczne do interpolacji `z` oraz Z-buffer (`+Z` do obserwatora, wygrywa większe `z`), tymczasowe stałe kolory per powierzchnia.
  - **Slice 4:** model oświetlenia (rozproszone Lamberta + zwierciadlane, bez ambient) z interpolacją normalnych i pozycji 3D per piksel.
- **Decyzja 6 (Slice 3 — AET z sortowaniem wierzchołków wg schematu wykładowego):** Zamieniono pierwotny podział trójkąta na część górną/dolną w `TriangleRasterizer` na ogólny algorytm wypełniania wielokątów z sortowaniem indeksów wierzchołków `ind[0..2]` po zaokrąglonym całkowitym `y` i tablicą aktywnych krawędzi (`AET`). Współrzędne barycentryczne pozostają liczone na oryginalnych ekranowych `float` (bez psucia interpolacji), krawędzie poziome (`dy == 0`) są pomijane przez ostry warunek `dy > 0`, a aktualizacja `AET` odbywa się również dla `y < 0` (rysowanie tylko dla `0 <= y < Height`). Reużywane tablice (`px`, `py`, `ind`, `aet` o rozmiarze 4) gwarantują 0 alokacji sterty na trójkąt.
- **Decyzja 7 (Slice 4 — model oświetlenia, dwustronność i statyczne światło):**
  - Równanie bez składowej ambient ($I = k_d I_L I_O \cos(\angle(N, L)) + k_s I_L I_O \cos^m(\angle(V, R))$), liczone w $[0, 1]$ per kanał R, G, B.
  - **Dwustronność (interpretacja skreślonego zdania o ujemnym cosinusie):** zamiast odrzucać tylną stronę powierzchni, odwracamy zinterpolowany wektor normalny ($N = -N$) zawsze, gdy wskazuje od obserwatora ($N_z < 0$). Dla tak zorientowanej widocznej strony, jeśli światło jest za nią ($\langle N, L \rangle \le 0$), składowe wynoszą `0`, a $\langle V, R \rangle$ ograniczamy do $\ge 0$ (chroni przed ujemnym natężeniem przy nieparzystym $m$ i fałszywymi refleksami przy parzystym $m$).
  - Tymczasowe statyczne światło w $(0, 0, z_L)$ z suwakiem $z_L \in [0.5, 5.0]$ (domyślnie `2.0`); kolor/tekstura per powierzchnia i NormalMap wchodzą w **Slice 5**, a ruchoma/edytowalna krzywa Béziera światła w **Slice 6**.
- **Decyzja 8 (Slice 6 — zamknięta krzywa Béziera światła i edycja myszą):** Pojedyncza zamknięta krzywa Béziera stopnia 6 (7 punktów kontrolnych $V_0 \dots V_6$ z niezmiennikiem $V_6 = V_0$, oceniana algorytmem de Casteljau na płaszczyźnie $z = z_L$ w układzie obserwatora); edycja przez przeciąganie istniejących 6 uchwytów $V_0 \dots V_5$ (przesunięcie $V_0$ sprzężone z $V_6$); automatyczne rozdzielenie LPM (trafienie w uchwyt przesuwa punkt krzywej, LPM poza uchwytami obraca scenę jak dotąd, PPM nadal obraca wokół osi `Z`).
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
  - edge case: dla `data/surfaces.txt` normalne `Pu × Pv` wskazują w stronę **−z** (u rośnie wzdłuż `y`, v wzdłuż `x`) — istotne przy oświetleniu w Slice 3/4,
  - edge case: macierz `Orientation` jest składana z tysięcy przyrostów — możliwy powolny dryf ortogonalności (float); na razie bez reortonormalizacji,
  - przy k = 40 i antyaliasingu rysowanie ~6400 krawędzi — do obserwacji płynności.
4. **Punkt wyjścia do Slice 3:** siatki z wektorami `P, Pu, Pv, N` (przed/po obrocie), narastająca orientacja `Matrix3x3`, panel sterowania gotowy na kolejne kontrolki („Wypełnione trójkąty”). Gotowe do przejścia do **Slice 3**.

### Slice 3 — wykonany (2026-11-23, zaktualizowany na AET)
1. **Status / pliki** (`src/BezierSceneApp/`):
  - `TriangleRasterizer.cs` — bufory `int[] colorBuffer` (ARGB) i `float[] zBuffer`, `Bitmap` (`Format32bppArgb`); `EnsureSize(w, h)` (realokacja + `Dispose` starej bitmapy tylko przy zmianie rozmiaru), `Clear(argb)`, `FillTriangle(a, b, c, argb)` (algorytm wykładowy z sortowaniem indeksów wierzchołków `ind[0..2]` + `AET` + barycentryczne `z` + Z-buffer, bez alokacji sterty per trójkąt), `CopyToBitmap()` (`LockBits` + `Marshal.Copy`), `IDisposable`,
  - `DrawingPanel.cs` — właściwość `ShowFilled`, metoda `DrawFilled` (rasteryzacja wszystkich siatek → `DrawImageUnscaled`), `ToScreenDepth` (ekranowe `x, y` + `RotP.Z`), `Dispose(bool)` zwalnia rasteryzer; nowa kolejność warstw: tło → wypełnienie → siatka → wieloboki → osie (osie przeniesione na koniec),
  - `MainForm.Designer.cs` — `showFilledCheckBox` „Wypełnione trójkąty” (domyślnie zaznaczony, pod „Siatka trójkątów”); `MainForm.cs` — handler `showFilledCheckBox_CheckedChanged`.
2. **Przyjęte założenia:**
  - scanlinie `y` używają zaokrąglonych całkowitych `py = (int)MathF.Round(y)` oraz oryginalnych `px` (`float`); dla każdego wierzchołka na `y - 1` krawędzie w dół (`dy > 0`) dodawane są z `invM = (px[neighbor] - px[i]) / (float)dy` i `x = px[i] + invM`, a krawędzie kończące się (`dy < 0`) są usuwane z `AET` (poziome `dy == 0` pomijane),
  - wspólna krawędź sąsiednich trójkątów zawsze startuje z tego samego górnego wierzchołka i przyrasta tym samym `invM`, dając bitowo identyczne `x`; przedział `(yMin, yMax]` i kolumny `ceil(xLeft)..floor(xRight)` eliminują szczeliny między sąsiadami (w tym na wspólnych krawędziach poziomych),
  - współrzędne barycentryczne i `z` liczone są na oryginalnych (niezaokrąglonych i nieprzestawianych) ekranowych `float` (`a, b, c`); `z = RotP.Z` w jednostkach sceny (rzut prostokątny),
  - aktualizacja `AET` biegnie od `yMin` do `min(yMax, Height - 1)` (również dla `y < 0`), rysowanie pikseli tylko dla `0 <= y < Height` oraz `0 <= x < Width`,
  - `AET` jako reużywana tablica `ActiveEdge[4]` (przejściowo max 3 krawędzie podczas przejścia przez wierzchołek, 2 przy rysowaniu) sortowana przez insertion sort — **0 B alokacji** w `FillTriangle`,
  - przy równych `z` wygrywa fragment narysowany wcześniej (`z > zBuffer[idx]`, ostra nierówność),
  - kolory tymczasowe: `Gold`, `SteelBlue`, `IndianRed`, `MediumSeaGreen` (cyklicznie po indeksie siatki),
  - rasteryzacja wykonywana w każdym `OnPaint` (gdy `ShowFilled`); przy zerowym rozmiarze panelu (zminimalizowane okno) pominięta,
  - kopiowanie bufora jednym `Marshal.Copy` — dla `Format32bppArgb` stride = `width * 4`.
3. **Weryfikacja i przypadki brzegowe (review):**
  - build (`dotnet build --no-restore`) bez ostrzeżeń i błędów,
  - testy offscreen (harness w katalogu scratch, poza repozytorium):
    - 20× siatka 13×13 losowo zaburzonych trójkątów (z wymuszonymi poziomymi krawędziami) — **0 dziur** wewnątrz,
    - styk dwóch trójkątów na wspólnej poziomej krawędzi, trójkąty z płaską górą, płaskim dołem, całkowicie poziome (`yMin == yMax`) i współliniowe (`|D| < 1e-6`) — poprawnie i bez dzielenia przez 0,
    - obcinanie poza krawędzią panelu: trójkąt zaczynający się przy `y < 0` daje **0 różnic** pikseli względem referencji przesuniętej w dół; trójkąt przekraczający wszystkie 4 krawędzie oraz trójkąty całkowicie poza panelem — bez wyjątków,
    - Z-buffer i barycentryczne `z` (obie kolejności rysowania, przecinające się nachylone płaszczyzny z ułamkowym `y`, oraz obie strony obrotu sceny `Ry(-1.5)` / `Ry(+1.5)`) — poprawne zasłanianie,
    - powierzchnia Béziera nr 0 z `data/surfaces.txt` (widok na wprost, `k = 16`) — **0 dziur** wewnątrz,
    - wydajność i pamięć: `800×750`, `k = 16`, średni czas rasteryzacji + `CopyToBitmap` **~1,9 ms/klatkę**, alokacje w 1000 wywołaniach `FillTriangle` = **0 B**,
  - review: (1) znak Z — `z > zBuffer`, inicjalizacja `float.NegativeInfinity` ✔; (2) szwy / poziome krawędzie ✔; (3) granice — `yMin..min(yMax, Height-1)`, rysowanie tylko w `[0, W-1] × [0, H-1]`, brak dzielenia przez 0 ✔; (4) `Bitmap` zwalniana w `EnsureSize` i `Dispose`, pliki poza `TriangleRasterizer.cs` i `guidelines.md` nietknięte ✔,
  - do sprawdzenia ręcznie: obrót myszą bez migotania, 3 checkboxy + suwak 4–40, zmiana rozmiaru okna, ładowanie `MainForm` w Designerze VS 2022,
  - edge case: przy przekątnych widokach z boku trójkąty z `|D|` bliskim progu lub `yMin == yMax` po zaokrągleniu znikają (zamierzone); przy przecinających się powierzchniach możliwy lekki „z-fighting” wzdłuż linii przecięcia.
4. **Punkt wyjścia do Slice 4:** szybki rasteryzer AET z barycentrycznymi wagami per piksel (gotowe do interpolacji `N` i innych atrybutów), Z-buffer, checkbox wypełnienia. Gotowe do przejścia do **Slice 4**.


### Slice 4 — wykonany (2026-11-23)
1. **Status / pliki** (`src/BezierSceneApp/`):
  - `Model/LightSettings.cs` (nowy) — klasa z polami `Kd`, `Ks`, `M`, `Position` (`Vector3`, jednostki sceny), `LightColor` (`Vector3` w [0, 1]); metoda `Shade(p, n, objectColor)` → ARGB (dwustronność, Lambert + zwierciadlane, clamp); statyczne `ToVector(Color)`,
  - `TriangleRasterizer.cs` — `FillTriangle(a, b, c, va, vb, vc, color, light)`: atrybuty trójkąta (`RotP`, `RotN` wierzchołków, `I_O`, światło) zapisywane w polach klasy (bez rozbudowy listy parametrów `FillRow`); w `FillRow` kolejność: barycentryczne → `z` → test Z-buffera (`continue` przy przegranej) → interpolacja `P_3D` i `N` + normalizacja → `light.Shade`,
  - `DrawingPanel.cs` — `SurfaceColors` jako `Vector3[]` (te same 4 kolory), właściwość `Light` (`LightSettings`) przekazywana do rasteryzera,
  - `MainForm.Designer.cs` — `controlPanel.AutoScroll = true`; `kdLabel/kdTrackBar` (0–100, 70), `ksLabel/ksTrackBar` (0–100, 50), `mLabel/mTrackBar` (1–100, 20), `lightZLabel/lightZTrackBar` (5–50, 20), `lightColorButton` „Kolor światła”, `lightColorDialog`; tylko układ i podpięcie zdarzeń,
  - `MainForm.cs` — wspólny handler `lightingTrackBar_ValueChanged` (aktualizacja `drawingPanel.Light` + etykiet) i `lightColorButton_Click` (`ColorDialog` → `LightColor`, podgląd jako `BackColor` przycisku).
2. **Przyjęte założenia:**
  - parametry oświetlenia trzymane w `DrawingPanel.Light` (jedna instancja, mutowana przez UI; domyślne wartości pól = domyślne wartości suwaków: `kd = 0.70`, `ks = 0.50`, `m = 20`, `z_L = 2.0`, światło białe),
  - `Color ↔ Vector3`: `R/255, G/255, B/255` (alfa pomijana); wynik `(int)(c * 255 + 0.5)` + `Math.Clamp(0, 255)` — przy `kd + ks > 1` kanały nasycają się do 255 bez przekręcenia bajtów,
  - $\langle V, R \rangle$ liczone bez jawnego `R`: `cosVR = 2·cosNL·N.z − L.z` (dokładne dla wersorów `N`, `L`), potem `max(0, ·)` i `MathF.Pow(cosVR, m)` tylko dla `cosVR > 0`,
  - wektory zdegenerowane: `|N_raw|² < 1e-12 → N = (0, 0, 1)`, `|P_L − P|² < 1e-12 → L = (0, 0, 1)`,
  - dwustronność per piksel: `N.z < 0 → N = −N` (w `Shade`); `cosNL ≤ 0` → czarny piksel `0xFF000000`,
  - etykiety formatowane `InvariantCulture` (kropka dziesiętna, spójnie z tekstami w Designerze).
3. **Weryfikacja i przypadki brzegowe:**
  - build Debug/Release: 0 ostrzeżeń, 0 błędów; smoke test — aplikacja startuje,
  - harness offscreen (scratch, poza repo; `800×750`, `k = 16`, render do PNG):
    - gładkie cieniowanie bez krawędzi trójkątów; połysk szeroki przy `m = 1` (max kanał 248), węższy i słabszy przy `m = 100`,
    - `kd = 0` → tylko refleks na czarnym tle (6184 kolorowych pikseli, reszta czarna), `kd = ks = 0` → 100% czarnych pikseli, `kd = ks = 1` → nasycenie do 255 bez artefaktów,
    - czerwone światło: niebieska powierzchnia praktycznie czarna, złota czerwonawa; `z_L = 0.5` → wyraźnie więcej cienia (18 242 czarnych px), `z_L = 5` → równomierne oświetlenie,
    - obrót o 180° wokół `Y`: 0 czarnych pikseli z cienia — obie strony oświetlone,
    - `Shade`: `P = P_L` → bez NaN (biały), `N` i `−N` dają ten sam kolor, światło za powierzchnią → czarny,
    - wydajność: **~5,8 ms/klatkę** (Release; wcześniej ~1,9 ms bez oświetlenia), alokacje w 1000× `FillTriangle` = **0 B**,
  - review / judge: (1) `P_3D` interpolowane z `RotP`, światło w tych samych jednostkach sceny, `L = P_L − P` ✔; (2) `N` z obróconych `RotN`, normalizacja po interpolacji z zabezpieczeniem ✔; (3) odwrócenie `N` przy `N.z < 0`, `cosNL ≤ 0` → czarny, `cosVR ≥ 0` ✔; (4) test Z-buffera przed interpolacją i `MathF.Pow` ✔; (5) obliczenia w [0, 1], clamp 0..255 ✔; (6) 0 B alokacji, Designer bez logiki ✔,
  - do sprawdzenia ręcznie: ładowanie `MainForm` w Designerze VS 2022, płynność obrotu myszą, `AutoScroll` panelu przy małym oknie,
  - edge case: przy `z_L` bliskim powierzchni (0.5) i silnie nachylonych fragmentach duże obszary w cieniu (zgodne z modelem bez ambient); wokół konturu powierzchni widzianej z boku `N` interpolowane może mieć `N.z ≈ 0` — odwracanie per piksel może dać skok cieniowania na linii konturu (zamierzone przy dwustronności).
4. **Punkt wyjścia do Slice 5:** per-pikselowe `P_3D`, `N` i parametry `u, v` dostępne w wierzchołkach; `LightSettings.Shade` przyjmuje `I_O` per piksel — gotowe do wpięcia tekstury (`I_O` z obrazu wg `u, v`) i NormalMap. Gotowe do przejścia do **Slice 5**.

### Slice 5 — wykonany (2026-11-23)
1. **Status / pliki** (`src/BezierSceneApp/`):
  - `Model/TextureData.cs` (nowy) — `LoadColor(path)` / `LoadNormalMap(path)`: jednorazowy odczyt `Bitmap` (`using`) + `LockBits(Format32bppArgb)` + `Marshal.Copy` do płaskiej `Vector3[]`; `Sample(u, v)` nearest-neighbor z clampingiem; brak pliku → `FileNotFoundException` (GDI+ zgłaszałby nieczytelne „Parameter is not valid”),
  - `Model/SurfaceAppearance.cs` (nowy) — pola `UseTexture`, `SolidColor` (`Vector3`), `Texture` (`TextureData?`),
  - `Model/Scene.cs` — `Appearances` (`Appearances[i]` ↔ `Surfaces[i]` ↔ `Meshes[i]`, kolory startowe z palety `Gold`, `SteelBlue`, `IndianRed`, `MediumSeaGreen` przeniesionej z `DrawingPanel`), `UseNormalMap`, `NormalMap`,
  - `Model/Matrix3x3.cs` — `FromColumns(c0, c1, c2)`,
  - `TriangleRasterizer.cs` — `FillTriangle(..., color, tex, normals, light)`: dodatkowe pola trójkąta (`RotPu`, `RotPv`, `U`, `V`, tekstura, NormalMap); w `FillRow` po teście Z-buffera: `u, v` (tylko gdy jest tekstura lub NormalMap) → `I_O` → `PerturbNormal` (tylko przy włączonej NormalMap) → `Shade`,
  - `DrawingPanel.cs` — `DrawFilled` przekazuje `SolidColor`, teksturę (`null` w trybie koloru) i NormalMap (`null` gdy wyłączona),
  - `MainForm.Designer.cs` — `surfaceComboBox`, `objectColorGroupBox` („Kolor obiektu”: `solidColorRadioButton`, `textureRadioButton`, `objectColorButton`, `loadTextureButton`), `useNormalMapCheckBox`, `loadNormalMapButton`, `objectColorDialog`, `imageOpenFileDialog` (filtr `*.png;*.jpg;*.jpeg;*.bmp`); tylko układ i podpięcie zdarzeń,
  - `MainForm.cs` — wczytanie domyślnych obrazów, `LoadImage` (błąd → `MessageBox`, zwraca `null`, dotychczasowy obraz zostaje), synchronizacja UI z flagą `isSyncingUi`, handlery kontrolek,
  - `data/texture.png` (nowy, 512×512 PNG, cegły 128×64 px z fugą 6 px, wygenerowany jednorazowo skryptem poza repo), `data/normalmap.png` (kopia `data/normalMapCircle.png`, 512×512 — pojedyncza półkula, najłatwiej ocenić kierunek reliefu); przykładowe mapy `normal_4.png`, `floorNormal.png`, `15574-normal.jpg`, `normalMapCircle.png` zostają do testów,
  - `data/surfaces.txt` — **transpozycja kolejności punktów kontrolnych** w obu powierzchniach (ta sama geometria): teraz `u` rośnie wzdłuż `+x`, `v` wzdłuż `+y`, a `N = Pu × Pv` wskazuje do obserwatora (`+z`) w widoku startowym (wcześniej `−z`, patrz edge case w Slice 2).
2. **Przyjęte założenia:**
  - orientacja tekstury: `tx = u · (W − 1)`, `ty = (1 − v) · (H − 1)` — `v = 1` to górny wiersz obrazu; zgodne z konwencją map normalnych OpenGL (zielony = „w górę” = `+Pv`), więc kolumny macierzy `[Pu, Pv, N]` odpowiadają osiom obrazu `x`, `y` i kanałowi `B`,
  - start: wszystkie powierzchnie mają przypisaną `data/texture.png`; Powierzchnia 1 — jednolity `Gold`, Powierzchnia 2 — tekstura; NormalMap wczytana z `data/normalmap.png`, ale wyłączona (zaznaczenie checkboxa od razu ją aktywuje),
  - brak domyślnego pliku przy starcie → `MessageBox` i dalsza praca z `null` (powierzchnia w trybie tekstury bez tekstury rysowana jest kolorem jednolitym; brak NormalMap = brak perturbacji),
  - udane „Wczytaj teksturę...” przełącza wybraną powierzchnię na tryb tekstury, udane „Wczytaj NormalMap...” zaznacza „Mapa normalnych” (inaczej wczytanie nie dawałoby widocznego efektu); wybór koloru nie zmienia trybu,
  - NormalMap jest wspólna dla sceny, tekstura i kolor — per powierzchnia (`SurfaceAppearance` trzymane w `Scene`, UI edytuje wygląd wskazany przez `surfaceComboBox`),
  - perturbacja działa na `N_powierzchni` przed dwustronnością; `Shade` odwraca wynikowe `N` przy `N.z < 0`, więc z tyłu płata relief jest fizycznie spójny (wypukłość z przodu = wklęsłość z tyłu).
3. **Weryfikacja i przypadki brzegowe:**
  - build Debug: 0 ostrzeżeń, 0 błędów; smoke test — aplikacja startuje bez komunikatów, nowe kontrolki mieszczą się w panelu (zrzut ekranu),
  - harness offscreen (scratch, poza repo; `800×750`, `k = 16`, Release, render do PNG):
    - `RGB(127, 127, 255)` → `(−0.0039, −0.0039, 0.99998)`; płaska NormalMap vs wyłączona: max różnica **3/255** na kanał (zaokrąglenie `127/255` wzmocnione przez `m = 20`),
    - obraz `normalMapCircle.png` użyty jako tekstura koloru: zielony u góry, czerwony po prawej — tekstura nie jest odbita ani obrócona; cegły poziome,
    - NormalMap „półkula” w widoku startowym: ciemny brzeg po stronie odwróconej od światła → wypukłość (przed transpozycją `surfaces.txt` była widoczna jako wklęsłość i tekstura była transponowana); `normal_4.png` daje wyraźny relief kamieni na obu powierzchniach,
    - `Sample` poza `[0, 1]` i z `NaN` — bez wyjątków; uszkodzony plik → `ArgumentException`, brak pliku → `FileNotFoundException` (oba łapane w `LoadImage`); plik źródłowy da się usunąć zaraz po wczytaniu (bitmapa zwolniona),
    - czas klatki (rasteryzacja + `CopyToBitmap` + odczyt testowy): kolor ~5,5 ms, tekstura ~6 ms, tekstura + NormalMap ~10 ms; alokacje w 1000× `FillTriangle` z teksturą i NormalMap = **0 B**,
  - review / judge: (1) `RotPu`, `RotPv` interpolowane i normalizowane przed `FromColumns`, wynik `M · N_tekstury` normalizowany, zabezpieczenia `1e-12` ✔; (2) dekodowanie `2c/255 − 1` + normalizacja, płaska mapa ≈ wyłączona ✔; (3) clamp `u, v` w rasteryzerze i w `Sample`, `LockBits` zamiast `GetPixel`, `Pu/Pv` liczone tylko przy włączonej NormalMap ✔; (4) osobny `SurfaceAppearance` na powierzchnię, `isSyncingUi` blokuje zapis przy przełączaniu combo ✔; (5) `using` na `Bitmap`, błędy plików → `MessageBox`, 0 B alokacji, Designer bez logiki ✔,
  - do sprawdzenia ręcznie: ładowanie `MainForm` w Designerze VS 2022, płynność obrotu myszą przy `k = 16` z teksturą i NormalMap, niezależne przełączanie trybu/koloru/tekstury dla Powierzchni 1 i 2 w UI,
  - edge case: dla innych plików powierzchni orientacja `u, v` zależy od kolejności punktów — przy `Pu × Pv` od obserwatora tekstura będzie odbita, a relief odwrócony (do obrotu o 180°); przy konturze (`N.z ≈ 0`) perturbowana normalna może zmienić znak `z` inaczej niż `N_powierzchni` — możliwe pojedyncze piksele z odwróconym reliefem na linii konturu.
4. **Punkt wyjścia do Slice 6:** `I_O` per piksel z koloru lub tekstury per powierzchnia, opcjonalna NormalMap w przestrzeni stycznej, pozycja światła w `LightSettings.Position` (na razie `(0, 0, z_L)` z suwaka). Gotowe do przejścia do **Slice 6** (animowane i edytowalne źródło światła na zamkniętej krzywej Béziera).
