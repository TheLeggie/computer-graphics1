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

## 5. Aktualny etap: SLICE 4 — Model oświetlenia (Lambert + zwierciadlane) i interpolacja normalnych

### Cel Slice 4
Rozszerzyć istniejący rasteryzer AET + Z-buffer o wyznaczanie dokładnego koloru każdego widocznego piksela wewnątrz trójkąta na podstawie **interpolacji wektorów normalnych** $N$ i pozycji $P_{3D}$ przez współrzędne barycentryczne oraz dwustronnego modelu oświetlenia (składowa rozproszona Lamberta + składowa zwierciadlana, bez ambient) dla statycznego punktowego źródła światła, wraz z suwakami $k_d, k_s, m$, suwakiem wysokości $z$ światła oraz wyborem koloru światła $I_L$.

### Zakres prac w Slice 4

#### 1. Interpolacja atrybutów w rasteryzerze (`TriangleRasterizer`)
- Rozszerz `FillTriangle` tak, aby oprócz ekranowych współrzędnych wierzchołków $(x_A, y_A), (x_B, y_B), (x_C, y_C)$ otrzymywał wierzchołki trójkąta (`Vertex` lub ich obrócone `RotP` i `RotN`) oraz parametry oświetlenia i kolor obiektu $I_O$ (zapisany jako `Vector3` z zakresu $[0, 1]$).
- **Kolejność obliczeń w `FillRow` (wydajność):**
  1. Wylicz wagi barycentryczne $(\alpha, \beta, \gamma)$ i głębokość $z = \alpha z_A + \beta z_B + \gamma z_C$.
  2. Wykonaj **test Z-buffera** (`if (z > zBuffer[idx])`). Jeśli piksel przegrywa test głębokości — natychmiast przejdź do kolejnego `x` (nie licz normalnych ani oświetlenia dla zasłoniętych pikseli).
  3. Dopiero dla piksela wygrywającego test głębokości zapisz `zBuffer[idx] = z` i wylicz atrybuty w punkcie $P$:
     - pozycja 3D punktu w jednostkach sceny (taka sama przestrzeń jak `RotP` i pozycja światła):
       $$P_{3D} = \alpha \cdot P_A + \beta \cdot P_B + \gamma \cdot P_C$$
     - zinterpolowany wektor normalny (obowiązkowo znormalizowany do długości 1):
       $$N_{\text{raw}} = \alpha \cdot N_A + \beta \cdot N_B + \gamma \cdot N_C, \qquad N = \text{normalize}(N_{\text{raw}})$$
       (z zabezpieczeniem na przypadek $\|N_{\text{raw}}\|^2 < 10^{-12} \implies N = (0, 0, 1)$).
- Zachowaj **0 alokacji sterty** wewnątrz `FillTriangle` / `FillRow` (używaj wyłącznie typów wartościowych: `Vector3`, `struct`).

#### 2. Model oświetlenia (składowa rozproszona Lamberta + zwierciadlana)
- Wyznaczaj kolor piksela w osobnej, prostej klasie/strukturze (np. `LightSettings` + metoda licząca kolor lub `PhongShading`) z równania z treści zadania (**bez** składowej ambient $k_a$), osobno dla każdej składowej koloru $c \in \{R, G, B\}$ w zakresie $[0, 1]$:
  $$I_c = k_d \cdot I_{L,c} \cdot I_{O,c} \cdot \cos(\angle(N, L)) + k_s \cdot I_{L,c} \cdot I_{O,c} \cdot \cos^m(\angle(V, R))$$
- **Wektory (wszystkie muszą być wersorami — długość 1):**
  - $V = (0, 0, 1)$ — stały wersor wskazujący w stronę obserwatora,
  - $L = \text{normalize}(P_{\text{światła}} - P_{3D})$ — wersor od punktu na powierzchni **do** źródła światła (gdy $\|P_{\text{światła}} - P_{3D}\|^2 < 10^{-12}$, przyjmij $L = (0, 0, 1)$),
  - $N$ — znormalizowany, zinterpolowany wektor normalny w punkcie $P$.
- **Dwustronność powierzchni (obsługa obu stron powierzchni):**
  - Zgodnie z uwagą w zadaniu należy uwzględnić obie strony powierzchni (w `data/surfaces.txt` początkowe $P_u \times P_v$ wskazują w stronę $-z$).
  - Jeśli zinterpolowany wektor normalny wskazuje od obserwatora, tzn. $\langle N, V \rangle < 0$ (równoważnie $N_z < 0$), **odwróć jego zwrot**: $N = -N$. Dzięki temu $N$ jest zawsze skierowany na stronę powierzchni widoczną dla obserwatora.
- **Wyznaczenie cosinusów i wektora odbitego $R$:**
  - $\text{cosNL} = \langle N, L \rangle = N_x L_x + N_y L_y + N_z L_z$,
  - jeśli $\text{cosNL} \le 0$ (źródło światła jest za widoczną stroną powierzchni), punkt jest w cieniu: $I = (0, 0, 0)$,
  - jeśli $\text{cosNL} > 0$:
    - wyznacz wersor odbity:
      $$R = 2 \cdot \langle N, L \rangle \cdot N - L$$
      (dla bezpieczeństwa numerycznego można go znormalizować lub utrzymać przy znormalizowanych $N, L$),
    - $\text{cosVR} = \max(0, \langle V, R \rangle) = \max(0, R_z)$ (ponieważ $V = (0, 0, 1)$; zaciskanie $\ge 0$ zapobiega ujemnym kolorom dla nieparzystych $m$ i fałszywym refleksom z tyłu dla parzystych $m$),
    - $\text{spec} = (\text{cosVR} > 0) \ ? \ \text{MathF.Pow}(\text{cosVR}, m) : 0$,
    - oblicz natężenie (w C# iloczyn po składowych `I_L * I_O` na `Vector3`):
      $$I = (I_L \odot I_O) \cdot (k_d \cdot \text{cosNL} + k_s \cdot \text{spec})$$
- **Konwersja wyniku na `int` ARGB:**
  - dopiero na końcu przelicz składowe $I_R, I_G, I_B$ z $[0, 1]$ na zakres `0..255` i ogranicz (clamp) do `[0, 255]`:
    `int r = Math.Clamp((int)(color.X * 255f + 0.5f), 0, 255);` (analogicznie `g`, `b`),
  - złóż piksel jako `(255 << 24) | (r << 16) | (g << 8) | b`.

#### 3. Statyczne źródło światła (tymczasowe dla Slice 4) i kolory obiektów
- **Położenie źródła światła:** na płaszczyźnie $z = \text{const}$ w jednostkach sceny: $P_{\text{światła}} = (0, 0, z_L)$, z domyślnym $z_L = 2.0$ (regulowane suwakiem w zakresie `0.5`–`5.0`).
- **Kolor światła $I_L$:** domyślnie biały `(1, 1, 1)` (`Color.White`), zmieniany przez `ColorDialog`.
- **Kolor obiektu $I_O$:** w Slice 4 pozostaje jednolity na powierzchnię z dotychczasowej palety (`Gold`, `SteelBlue`, `IndianRed`, `MediumSeaGreen` przeliczone na `Vector3` w $[0, 1]$); niezależny wybór kolor/tekstura per powierzchnia wejdzie w Slice 5.

#### 4. Kontrolki UI (`MainForm.Designer.cs` + `MainForm.cs`)
- W `controlPanel` (utrzymuj czystość `MainForm.Designer.cs` — wyłącznie układ kontrolek i podpięcie zdarzeń; w razie potrzeby włącz `controlPanel.AutoScroll = true`, aby kontrolki nie były ucinane na mniejszym oknie):
  - suwak **$k_d$**: zakres `0.00`–`1.00` (`TrackBar` `0`–`100`, domyślnie `70` → `0.70`) + etykieta z bieżącą wartością,
  - suwak **$k_s$**: zakres `0.00`–`1.00` (`TrackBar` `0`–`100`, domyślnie `50` → `0.50`) + etykieta z bieżącą wartością,
  - suwak **$m$**: zakres `1`–`100` (`TrackBar` `1`–`100`, domyślnie `20`) + etykieta z bieżącą wartością,
  - suwak **$z$ światła**: zakres `0.5`–`5.0` (`TrackBar` `5`–`50`, domyślnie `20` → `2.0`) + etykieta z bieżącą wartością,
  - przycisk **„Kolor światła”** otwierający `ColorDialog` (z czytelnym podglądem wybranego koloru, np. mały panel/swatch lub kolor przycisku).
- Każda zmiana parametru oświetlenia odświeża `drawingPanel` (`Invalidate()`).

### Czego NIE robić w Slice 4
- Nie implementuj jeszcze animacji źródła światła ani zamkniętej krzywej Béziera dla światła (zrobimy to w **Slice 6**).
- Nie implementuj jeszcze tekstur (obrazów dla $I_O$), niezależnego wyboru kolor/tekstura per powierzchnia ani mapy wektorów normalnych (NormalMap) — te elementy wejdą w **Slice 5**.
- Nie dodawaj składowej ambient ($k_a$) do równania oświetlenia.
- Nie zmieniaj algorytmu wypełniania AET ani struktury bufora `int[]` + `LockBits`.
- Nie twórz przedwczesnych interfejsów ani abstrakcji pod przyszłe tekstury/NormalMap.

### Review / LLM-as-a-judge (OBOWIĄZKOWY w Slice 4)
Po napisaniu kodu przeprowadź niezależną weryfikację (review / judge) pod kątem najczęstszych błędów w cieniowaniu:
1. **Spójność układu współrzędnych dla $L$:** czy $P_{3D}$ i $P_{\text{światła}}$ są w tych samych jednostkach sceny (nie ekranowych pikselach!), a wektor $L$ wskazuje od punktu **do** światła ($P_{\text{światła}} - P_{3D}$)?
2. **Interpolacja i normalizacja $N$:** czy $N(P)$ jest interpolowane z **obróconych** `RotN` i obowiązkowo normalizowane po interpolacji, z bezpieczną obsługą wektora zerowego?
3. **Dwustronność i znaki cosinusów:** czy przy $N_z < 0$ następuje odwrócenie $N = -N$ (obie strony powierzchni są oświetlane po obrocie o 180°), czy przy $\langle N, L \rangle \le 0$ punkt daje czarny kolor, oraz czy $\langle V, R \rangle$ jest ograniczane do $\ge 0$ (brak ujemnych kolorów dla nieparzystych $m$ i brak fałszywych refleksów z tyłu dla parzystych $m$)?
4. **Kolejność testu Z-buffera:** czy test `z > zBuffer[idx]` wykonuje się **przed** interpolacją $P_{3D}, N$ i liczeniem `MathF.Pow`?
5. **Zakres kolorów i clamping:** czy wszystkie obliczenia oświetlenia odbywają się w $[0, 1]$, a wynik końcowy jest poprawnie zaciskany do `0..255` (bez przekręcenia bajtów gdy $k_d + k_s > 1$)?
6. **Alokacje i Designer:** czy `FillTriangle` nadal ma 0 alokacji sterty oraz czy `MainForm.Designer.cs` pozostał czysty?

### Po zakończeniu Slice 4 (obowiązkowy wpis do `guidelines.md`)
Dopisz w sekcji **Dziennik slice'ów i decyzji** wpis `Slice 4`:
- listę utworzonych/zmodyfikowanych plików w `src/`,
- przyjęte założenia (struktura parametrów oświetlenia, domyślne $k_d, k_s, m, z_L$, detale konwersji `Color ↔ Vector3`, obsługa wektorów zdegenerowanych),
- wyniki weryfikacji (w tym podsumowanie review/judge i pomiar czasu klatki):
  - gładkie cieniowanie bez widocznych krawędzi pojedynczych trójkątów (potwierdzenie interpolacji normalnych per piksel),
  - wyraźny połysk zwierciadlany reagujący na $m$ (małe $m = 1..5$ → szeroki połysk, duże $m = 60..100$ → ostry punktowy refleks),
  - reakcja na $k_d = 0$ (tylko refleks zwierciadlany na czarnym tle), $k_s = 0$ (czysty matowy Lambert), $k_d = k_s = 0$ (czarne powierzchnie),
  - zmiana koloru światła $I_L$ (np. czerwone światło na niebieskiej powierzchni daje ciemny wynik, białe daje pełny kolor obiektu) oraz zmiana wysokości $z$ światła,
  - poprawne oświetlenie **obu stron** powierzchni po obróceniu sceny o 180° (brak całkowicie czarnej drugiej strony),
  - poprawne zasłanianie Z-bufferem oraz płynność obrotu myszą przy $k = 16$,
  - poprawność ładowania `MainForm` w Designerze Visual Studio 2022,
- potwierdzenie gotowości do przejścia do **Slice 5** (kolor/tekstura per powierzchnia + NormalMap).

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
