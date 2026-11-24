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

## 5. Aktualny etap: SLICE 5 — Kolor/tekstura per powierzchnia ($I_O$) i mapa wektorów normalnych (NormalMap)

### Cel Slice 5

Umożliwić niezależny wybór źródła koloru obiektu $I_O$ dla każdej powierzchni (jednolity kolor lub tekstura z obrazu mapowana przez parametry $(u, v) \in [0, 1]^2$) oraz dodać opcjonalną modyfikację wektora normalnego na podstawie wczytanej mapy wektorów normalnych (**NormalMap**) w przestrzeni stycznej $[P_u, P_v, N_{\text{powierzchni}}]$.

### Zakres prac w Slice 5

#### 1. Szybki odczyt obrazów (`TextureData` / `PixelMap` — bez `Bitmap.GetPixel`)

- Utwórz prostą klasę (np. `Model/TextureData.cs`) wczytującą obraz z pliku (`png`, `jpg`, `bmp`) jednorazowo przez `Bitmap` + `LockBits` + `Marshal.Copy` do płaskiej tablicy `Vector3[]` o rozmiarze `Width * Height` (zwalniając tymczasowy obiekt `Bitmap` przez `using` zaraz po skopiowaniu pikseli — zero wycieków GDI+):
  - **Dla tekstury koloru ($I_O$):** zdekoduj każdy piksel z `0..255` na `Vector3(R / 255f, G / 255f, B / 255f)` w zakresie $[0, 1]$.
  - **Dla mapy normalnych (NormalMap):** zdekoduj każdy piksel z `0..255` na wersor $N_{\text{tekstury}} \in [-1, +1]^3$:
    $$N_x = \frac{2R}{255} - 1, \qquad N_y = \frac{2G}{255} - 1, \qquad N_z = \frac{2B}{255} - 1$$
    i znormalizuj wektor przy ładowaniu (dla `RGB(127, 127, 255)` daje to $\approx (0, 0, 1)$, dla niebieskiego kanału `128..255` składowa $N_z > 0$; w razie zdegenerowanego wektora zerowego podstaw `(0, 0, 1)`).
- **Próbkowanie `Sample(float u, float v)`:**
  - zaciśnij $(u, v)$ do $[0, 1]$ (`Math.Clamp`), ponieważ wagi barycentryczne na krawędzi trójkąta mogą minimalnie wykraczać poza $[0, 1]$,
  - przeskaluj na indeksy pikseli obrazu: `tx = Math.Clamp((int)(u * (Width - 1) + 0.5f), 0, Width - 1)` oraz `ty = Math.Clamp((int)((1f - v) * (Height - 1) + 0.5f), 0, Height - 1)` (lub konsekwentnie `v` — udokumentuj orientację w dzienniku),
  - nigdy nie wywołuj `Bitmap.GetPixel()` w pętli renderowania.
- **Domyślne pliki w `data/`:**
  - przygotuj dwa czytelne domyślne obrazy w katalogu `data/`: `data/texture.png` (np. wyraźny wzór cegieł / kafelków) oraz `data/normalmap.png` (pasująca mapa normalnych z kanałem Blue w `128..255` i wyraźnym reliefem),
  - pliki w `data/` są automatycznie kopiowane przez `.csproj` do katalogu wynikowego; przy starcie wczytaj oba pliki z `AppContext.BaseDirectory/data/` (z czytelną obsługą błędu, gdyby pliku brakowało).

#### 2. Materiał per powierzchnia (`SurfaceAppearance` lub pola przy powierzchni)

- Każda powierzchnia w scenie musi mieć **niezależny** stan koloru obiektu $I_O$:
  - tryb: `UseTexture` (`bool`: `false` = jednolity kolor, `true` = tekstura),
  - `SolidColor` (`Vector3` w $[0, 1]$, początkowo z dotychczasowej palety `Gold`, `SteelBlue`, `IndianRed`, `MediumSeaGreen`),
  - `Texture` (`TextureData?`, początkowo przypisana domyślna tekstura `data/texture.png`).
- **Ustawienie startowe:** przypisz domyślną teksturę wszystkim powierzchniom; np. 1. powierzchnia startuje z jednolitym kolorem (`Gold`), a 2. powierzchnia z włączoną domyślną teksturą (lub wszystkie z domyślną teksturą — tak aby po uruchomieniu od razu było widać niezależność $I_O$ obu powierzchni).
- **Mapa normalnych (NormalMap):**
  - przechowuj w `Scene` (lub `DrawingPanel`) flagę `UseNormalMap` (`bool`, domyślnie `false` lub `true` — z domyślnie wczytaną `data/normalmap.png`, aby samo zaznaczenie checkboxa natychmiast aktywowało efekt) oraz aktualnie załadowaną `NormalMap` (`TextureData?`).

#### 3. Macierz przekształcenia $M_{3\times3}$ i modyfikacja wektora normalnego

- Dodaj w `Matrix3x3.cs` prostą metodę fabryczną budującą macierz z trzech wektorów kolumnowych (spójnie z konwencją kolumnową $P' = M \cdot P$ z Decyzji 4):

```csharp
public static Matrix3x3 FromColumns(Vector3 c0, Vector3 c1, Vector3 c2)
```

- Dla każdego widocznego piksela (dopiero po wygraniu testu Z-buffera!):
  1. Jeśli potrzebne są $(u, v)$ (aktywna tekstura dla tej powierzchni lub włączona NormalMap), zinterpoluj parametry powierzchni:
     $$u(P) = \text{Clamp}(\alpha u_A + \beta u_B + \gamma u_C,\ 0,\ 1), \qquad v(P) = \text{Clamp}(\alpha v_A + \beta v_B + \gamma v_C,\ 0,\ 1)$$
  2. Wyznacz kolor obiektu $I_O(P)$:
     - dla trybu tekstury: `texture.Sample(u, v)`,
     - dla trybu jednolitego koloru: stałe `solidColor`.
  3. Zinterpoluj i znormalizuj wektor normalny powierzchni $N_{\text{powierzchni}}$ (jak w Slice 4).
  4. Jeśli włączona jest NormalMap (gdy wyłączona — pomiń poniższe obliczenia, aby nie tracić wydajności):
     - zinterpoluj i znormalizuj obrócone wektory styczne w punkcie $P$ (pamiętaj: `RotPu` i `RotPv` są pochodnymi i nie mają długości 1, dlatego normalizacja przed budową macierzy $M$ jest obowiązkowa!):
       $$\hat{P}_u = \text{normalize}(\alpha \cdot \text{RotPu}_A + \beta \cdot \text{RotPu}_B + \gamma \cdot \text{RotPu}_C)$$
       $$\hat{P}_v = \text{normalize}(\alpha \cdot \text{RotPv}_A + \beta \cdot \text{RotPv}_B + \gamma \cdot \text{RotPv}_C)$$
       (z zabezpieczeniem: jeśli $\|\hat{P}_u\|^2 < 10^{-12}$ lub $\|\hat{P}_v\|^2 < 10^{-12}$, pomiń perturbację i zostaw $N = N_{\text{powierzchni}}$),
     - zbuduj macierz przestrzeni stycznej:
       $$M_{3\times3} = [\hat{P}_u,\ \hat{P}_v,\ N_{\text{powierzchni}}]$$
     - odczytaj z NormalMap wersor $N_{\text{tekstury}} = \text{normalMap.Sample}(u, v)$,
     - wyznacz zmodyfikowany wektor normalny i znormalizuj go:
       $$N = \text{normalize}(M \cdot N_{\text{tekstury}})$$
       (z zabezpieczeniem na wektor zerowy $\rightarrow N_{\text{powierzchni}}$).
  5. Przekaż `p3D`, `n`, `objectColor` do istniejącego `light.Shade(...)`.
- Zachowaj `0` alokacji sterty w `FillTriangle` / `FillRow`.

#### 4. Kontrolki UI (`MainForm.Designer.cs` + `MainForm.cs`)

W `controlPanel` (utrzymuj czystość `MainForm.Designer.cs` — wyłącznie układ kontrolek i podpięcie zdarzeń; `AutoScroll = true` już działa):

- **Sekcja koloru obiektu $I_O$ (niezależnie dla każdej powierzchni):**
  - `ComboBox` wyboru powierzchni (`surfaceComboBox`, `DropDownStyle = DropDownList`, wypełniany dynamicznie przy starcie: „Powierzchnia 1”, „Powierzchnia 2”, ...),
  - dwa przyciski radiowe (`RadioButton`, np. w `GroupBox` „Kolor obiektu”):
    - „Jednolity kolor” (`solidColorRadioButton`),
    - „Tekstura” (`textureRadioButton`),
  - przycisk „Kolor obiektu” (`objectColorButton`, otwiera `ColorDialog` i pokazuje podgląd koloru wybranej powierzchni),
  - przycisk „Wczytaj teksturę...” (`loadTextureButton`, otwiera `OpenFileDialog` z filtrem obrazów `*.png;*.jpg;*.jpeg;*.bmp` — w razie błędu pliku pokaż `MessageBox` bez nadpisywania aktualnej tekstury).
- **Sekcja mapy wektorów normalnych:**
  - `CheckBox` „Mapa normalnych” (`useNormalMapCheckBox`),
  - przycisk „Wczytaj NormalMap...” (`loadNormalMapButton`, `OpenFileDialog` z obsługą błędu pliku).
- **Synchronizacja UI przy zmianie wybranej powierzchni:** podczas przełączania `surfaceComboBox` ustaw flagę blokującą (np. `isSyncingUi = true`), aby aktualizacja `Checked` w `RadioButton` nie nadpisała ustawień innej powierzchni.

### Czego NIE robić w Slice 5

- Nie implementuj jeszcze animacji źródła światła ani zamkniętej krzywej Béziera dla światła (to zostaje na Slice 6).
- Nie używaj `Bitmap.GetPixel()` w pętli rasteryzacji.
- Nie zmieniaj algorytmu AET ani logiki Z-buffera.
- Nie dodawaj filtrowania dwuliniowego (bilinear) ani mipmap — prosty nearest-neighbor z clampingiem `[0, 1]` w zupełności wystarcza i zachowuje prostotę (*Simplicity First*).

### Review / LLM-as-a-judge (OBOWIĄZKOWY w Slice 5)

Po napisaniu kodu przeprowadź niezależną weryfikację (*review* / *judge*) pod kątem najczęstszych błędów w teksturowaniu i NormalMap:

1. **Normalizacja kolumn macierzy $M$:** czy `RotPu` i `RotPv` są interpolowane z obróconych wektorów i normalizowane przed wstawieniem jako kolumny `Matrix3x3.FromColumns(pu, pv, nSurf)`, oraz czy wynik $N = M \cdot N_{\text{tekstury}}$ jest normalizowany?
2. **Dekodowanie NormalMap:** czy `RGB(127, 127, 255)` daje $N_{\text{tekstury}} \approx (0, 0, 1)$ i czy dla płaskiej mapy normalnych (całej `(127, 127, 255)`) wynik cieniowania jest identyczny jak przy wyłączonej NormalMap?
3. **Bezpieczeństwo $(u, v)$ i wydajność:** czy $(u, v)$ są zaciskane do $[0, 1]$ przed indeksowaniem tablicy (brak `IndexOutOfRangeException` na krawędziach trójkątów), czy piksele obrazu są wyciągane raz przez `LockBits` (zero `GetPixel`), oraz czy przy wyłączonej NormalMap pomijana jest interpolacja `Pu`, `Pv`?
4. **Niezależność powierzchni i synchronizacja UI:** czy zmiana trybu (kolor/tekstura), koloru lub wczytanie innej tekstury dla Powierzchni 1 nie wpływa na Powierzchnię 2, oraz czy przełączanie `surfaceComboBox` poprawnie odzwierciedla stan każdej powierzchni bez nadpisywania go?
5. **Zasoby, błędy plików i Designer:** czy tymczasowe `Bitmap` przy ładowaniu tekstur są zwalniane (`using`), czy uszkodzony plik obrazu nie crashuje aplikacji, czy `FillTriangle` zachowuje `0 B` alokacji oraz czy `MainForm.Designer.cs` pozostał czysty?

### Po zakończeniu Slice 5 (obowiązkowy wpis do `guidelines.md`)

Dopisz w sekcji **Dziennik slice'ów i decyzji** wpis Slice 5:

- listę utworzonych/zmodyfikowanych plików w `src/` oraz dodanych domyślnych plików obrazów w `data/`,
- przyjęte założenia (format i rozdzielczość domyślnych plików w `data/`, mapowanie `v` vs `1 - v`, zachowanie startowe powierzchni, sposób zarządzania `SurfaceAppearance`),
- wyniki weryfikacji (w tym podsumowanie review/judge i pomiar czasu klatki z włączoną teksturą oraz NormalMap):
  - niezależne przełączanie jednolity kolor $\leftrightarrow$ tekstura oraz wczytywanie osobnych tekstur/kolorów dla Powierzchni 1 i Powierzchni 2,
  - poprawny efekt 3D reliefu po włączeniu NormalMap (wyraźna reakcja wypukłości/rowków na obrót sceny myszą i na suwaki $k_d, k_s, m, z_L$),
  - brak różnicy (poza zaokrągleniem `127/255`) między płaską NormalMap `(127, 127, 255)` a wyłączoną NormalMap,
  - płynność obrotu myszą przy $k = 16$ z włączoną teksturą i NormalMap,
  - poprawność ładowania `MainForm` w Designerze Visual Studio 2022,
- potwierdzenie gotowości do przejścia do Slice 6 (animowane i edytowalne źródło światła na zamkniętej krzywej Béziera).

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
