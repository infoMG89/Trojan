# LineProperties – přehled možností pluginu

Kompletní popis toho, co plugin umí.

---

## 1. Co plugin dělá

Plugin rozšiřuje AutoCAD o funkce pro ocelové konstrukce (SJI joisty, metal deck):

- **Kreslení joistů** – čáry s MORAVIO_SMART XData (designace, mark, délka, …)
- **Kreslení decku** – pole čar s typem decku a gauge
- **Editace** – úprava vlastností joistů (designace, mark, rozšíření, …)
- **Detekce spojů** – automatické nalezení spojů mezi joisty a deckem (geometrický dotyk)
- **Markery spojů** – každý spoj má ID (CONN-1, …) a volitelný kód (A1, B3, C7, …)
- **Sheety ze šablon** – automatické vytvoření layoutů s detaily spojů
- **Export BOM** – Excel (.xlsx) se seznamem joistů a deck prvků
- **Automatické značení** – Mark (J1, J2, …) a délky u joistů

---

## 2. Příkazy

| Příkaz | Co dělá |
|--------|---------|
| **SMARTJOIST** | Nakreslí joist mezi dvěma body. Otevře dialog – designace (10K1, 18LH02, …), mark, rozšíření, cantilever, shoe, bridging, tie joist. |
| **EDITJOIST** | Vyberte joist čáru(y) – otevře dialog s XData, umožní editaci všech vlastností. |
| **SMARTDECK** | Vyberte typ decku (B/C/N, gauge, profil), pak dva body – nakreslí pole deck čar s rozestupem podle rib spacing. |
| **EXPORTBOM** | Exportuje BOM do Excel – všechny entity s MORAVIO_SMART (joists, deck). Save File dialog. |
| **AUTOMARK** | Automaticky přiřadí Mark (J1, J2, … TJ1 pro tie joists) shodným joistům. Přidá značení délky doprostřed. |
| **LINEPROPLIST** | Seznam všech joistů a deck prvků s MORAVIO_SMART (Mark, Type, Designation, Layer, Span). |
| **CONNECTIONS** | Detekuje spoje mezi joisty a deckem, nakreslí modré markery (kruhy) s ID. Pak výběr markeru pro přiřazení kódu spoje. |
| **CONNECTION_MARKERS** | Vyberte marker – přiřaďte kód spoje (A1, B3, C7, …) v dialogu. |
| **CONNECTION_LIST** | Pro každý connection marker vytvoří layout ze šablony. Vyplní CONNECTION_ID, CONNECTION_CODE a parametrické rozměry (DIM_*). |
| **TEMPLATE_CHECK** | Kontrola šablon – layout, počet entit, bloky. |

---

## 3. Typický workflow

```
1. SMARTJOIST / SMARTDECK   → Nakreslete joisty a deck čáry
2. EDITJOIST                → Upravte designace, marky (volitelně)
3. AUTOMARK                 → Automatické značení J1, J2, … a délek
4. CONNECTIONS              → Detekce spojů, markery, přiřazení kódů
5. CONNECTION_LIST          → Vytvoření sheetů ze šablon
6. EXPORTBOM                → Export kusovníku do Excelu
```

---

## 4. MORAVIO_SMART XData

Každá joist a deck čára nese XData s těmito poli:

| Pole | Popis |
|------|-------|
| MemberType | JOIST nebo DECK |
| Mark | J1, J2, TJ1, … |
| Designation | 10K1, B-Deck 1.5" 18ga, … |
| ExtensionLeft/Right | Rozšíření (in) |
| CantileverLeft/Right | Konzola (in) |
| ShoeLeft/Right | Typ boty |
| BridgingLeft/Right | Typ bridging |
| IsTieJoist | Tie joist ano/ne |
| ActiveLoads | Zatížení |
| SequenceZone | Zóna |
| SpanLength | Délka rozpětí (in) |
| Depth | Hloubka (in) |
| Weight | Hmotnost (plf) |

---

## 5. Knihovna joistů (SJI)

**K-Series** – 8K1 až 30K12 (depth 8–30 in, chord 1–12)  
**LH-Series** – 18LH02, 20LH03, … (long span)  
**DLH-Series** – deep long span

Jednotky: palce (depth), lbs/ft (weight plf). Viz `Knihovna_nosniku.md`.

---

## 6. Knihovna decku

**B-Deck, C-Deck, N-Deck** – gauges 18, 20, 22 – profily 1", 1.5", 2", 3"

Příklad: B-Deck 1.5" 18ga, C-Deck 2" 20ga. Viz `Knihovna_plechu.md`.

---

## 7. Spoje a kódy

**Typy spojů:**
- **A** – deck–deck
- **B** – joist–deck
- **C** – joist–joist

**Kódy druhu spoje:**

| Typ | Kódy | Příklady |
|-----|------|----------|
| A | A1, A2, A15, A3, A4 | Svařované tupo, šroubované M12, … |
| B | B1, B2, B3, B7, B4 | Svařované koutové, šroubované přes styčník, … |
| C | C1, C2, C3, C7, C4 | Styčník šroubovaný, čepový spoj, … |

Viz `Knihovna_spoju.md`.

---

## 8. Šablony pro CONNECTION_LIST

**Umístění:** `C:\Users\marti\OneDrive\Documents_2\Cursor\AutoCAD\Templates\`

**Soubory:**
- `Connection_A_Template.dwg` – deck–deck
- `Connection_B_Template.dwg` – joist–deck
- `Connection_C_Template.dwg` – joist–joist

**Struktura šablony:**
- Geometrie v **paper space layoutu** (ne v model space)
- Blok s atributem **CONNECTION_ID** nebo **ODKAZ** – ID spoje (CONN-1, …)
- Blok s atributem **CONNECTION_CODE** nebo **DRUH_SPOJE** – kód (A1, B3, …)
- Bloky **DIM_H1**, **DIM_B1**, **DIM_T1**, … s atributem **VALUE** – parametrické rozměry (mm)

Viz `Konvence_sablon.md`, `Navod_priprava_šablon.md`.

---

## 9. Vrstvy

- **JOISTS** – červená, joist čáry
- **DECK** – zelená, deck čáry
- **WALLS** – bílá
- **COLUMNS** – žlutá

---

## 10. Connection markery

Markery (modré kruhy) nesou XData:
- Handle1, Handle2 – entity ve spoji
- Type – A, B, C
- ConnectionId – CONN-1, CONN-2, …
- ChosenCode – A1, B3, C7, … (volitelně)

CONNECTION_LIST vytvoří pro každý marker nový layout, zkopíruje obsah šablony a vyplní atributy včetně rozměrů z MORAVIO_SMART připojených entit.
