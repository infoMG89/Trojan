# Přehled příkazů pluginu LineProperties (Moravio Demo)

| Příkaz | Popis |
|--------|-------|
| **SMARTJOIST** | Nakreslí joist mezi dvěma body, otevře Edit Joist Smart Data dialog. Přiřadí Mark (J1, J2, … nebo TJ1 pro tie joists). |
| **EDITJOIST** | Vyberte joist čáru(y) – otevře dialog s XData, umožní editaci. |
| **SMARTDECK** | Vyberte typ decku (B/C/N, gauge), pak dva body – nakreslí pole deck čar s XData. |
| **EXPORTBOM** | Exportuje BOM do Excel (.xlsx) – scan entit s MORAVIO_SMART, Save File dialog. |
| **AUTOMARK** | Automaticky přiřadí Mark čísla (J1, J2, … TJ1) shodným joistům. |
| **LINEPROPLIST** | Seznam všech joistů a deck prvků s MORAVIO_SMART XData (Mark, Type, Designation, Layer, Span). |
| **CONNECTIONS** | Detekuje spoje mezi joisty a deckem, nakreslí markery (kruhy) s ID (CONN-1, …). |
| **CONNECTION_MARKERS** | Vyberte marker – přiřaďte kód spoje (A1, B3, C7, …) v dialogu. |
| **CONNECTION_LIST** | Vytvoří sheet ze šablony pro každý connection marker (Connection_A/B/C_Template.dwg). |
| **TEMPLATE_CHECK** | Kontrola šablon – layout, počet entit, bloky. |

## Vrstvy

- **JOISTS** – červená, joist čáry
- **DECK** – zelená, deck čáry
- **WALLS** – bílá
- **COLUMNS** – žlutá
