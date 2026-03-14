# Přehled příkazů pluginu LineProperties (Moravio Demo)

| Příkaz | Popis |
|--------|-------|
| **SMARTJOIST** | Nakreslí joist nebo AISC nosník mezi dvěma body. Knihovna: SJI Joists, AISC W-Shapes, AISC C-Channels. Mark J1/B1/TJ1 podle typu. |
| **EDITJOIST** | Vyberte joist čáru(y) – otevře dialog s XData, umožní editaci. |
| **SMARTMATCH** | Zkopírujte vlastnosti z jedné čáry/plechu na jiné – 2 kroky, bez dialogu. |
| **SMARTASSIGN** | Přiřaďte smart vlastnosti vybraným čárám/plechům – výběr + kompaktní dialog. |
| **SMARTDECK** | Nakreslí obdélník (plech) mezi dvěma rohy. Výběr typu plechu (PL 6–30 mm), Mark P1, P2… |
| **SMARTPATTERN** | Vyberte typ decku (B/C/N, gauge), pak dva body – nakreslí pole deck čar s XData. |
| **AISC_SHAPE** | AISC 14th Ed steel shapes – vyberte W nebo C, designaci (W14x22, C12x25…), pak dva body – nakreslí nosník s MORAVIO_SMART. |
| **EXPORTBOM** | Exportuje BOM do Excel (.xlsx) – scan entit s MORAVIO_SMART, Save File dialog. |
| **AUTOMARK** | Automaticky přiřadí Mark čísla (J1, J2, … TJ1) shodným joistům. |
| **SMARTJOIST_LIST** | Seznam všech joistů a deck prvků s MORAVIO_SMART XData (Mark, Type, Designation, Layer, Span). |
| **CONNECTIONS** | Detekuje spoje mezi joisty, deckem a plechy, nakreslí markery (kruhy) s ID (CONN-1, …). |
| **CONNECTION_MARKERS** | Vyberte marker – přiřaďte kód spoje (A1, B3, C7, …) v dialogu. |
| **CONNECTION_LIST** | Popup okno s přehledem všech spojů – ID, typ, kód, mezi kterými prvky. |
| **CONNECTION_SHEET** | Vytvoří sheet ze šablony pro každý connection marker (Connection_A/B/C_Template.dwg). |
| **TEMPLATE_CHECK** | Kontrola šablon – layout, počet entit, bloky. |

## Vrstvy

- **JOISTS** – červená, joist čáry
- **DECK** – zelená, deck čáry
- **PLATES** – žlutá, plechy (obdélníky)
- **WALLS** – bílá
- **COLUMNS** – žlutá
