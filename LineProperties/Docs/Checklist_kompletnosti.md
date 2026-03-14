# Checklist kompletnosti úkolu (Trojan / Moravio Demo)

Podle specifikace Demo Plugin Engineering One-Pager (Trojan Steel / Moravio) a rozšíření o CONNECTIONS.

---

## 1. Doména a entity

| # | Požadavek | Stav | Poznámka |
|---|-----------|------|----------|
| 1.1 | SJI joists (K-Series, LH-Series, DLH-Series) | ✅ | SjiJoistLibrary.cs |
| 1.2 | Metal deck (B-Deck, C-Deck, N-Deck) | ✅ | DeckLibrary.cs |
| 1.3 | Gauges 18, 20, 22 | ✅ | DeckLibrary |
| 1.4 | Profily decku 1"–3" | ✅ | DeckLibrary |

---

## 2. XData (MORAVIO_SMART)

| # | Požadavek | Stav | Poznámka |
|---|-----------|------|----------|
| 2.1 | RegApp MORAVIO_SMART | ✅ | MoravioSmartXDataService |
| 2.2 | Pole: MemberType, Mark, Designation | ✅ | |
| 2.3 | Pole: ExtensionLeft/Right, CantileverLeft/Right | ✅ | |
| 2.4 | Pole: ShoeLeft/Right, BridgingLeft/Right | ✅ | |
| 2.5 | Pole: IsTieJoist, ActiveLoads, SequenceZone | ✅ | |
| 2.6 | Pole: SpanLength, Depth, Weight | ✅ | |

---

## 3. Příkazy

| # | Příkaz | Stav | Poznámka |
|---|--------|------|----------|
| 3.1 | SMARTJOIST | ✅ | Kreslení joistu, dialog |
| 3.2 | EDITJOIST | ✅ | Editace XData joistu |
| 3.3 | SMARTPATTERN | ✅ | Kreslení deck čar |
| 3.4 | AISC_SHAPE | ✅ | AISC 14th W/C shapes |
| 3.5 | EXPORTBOM | ✅ | Excel export (ClosedXML) |
| 3.6 | AUTOMARK | ✅ | Mark J1, J2…, délky |
| 3.7 | SMARTJOIST_LIST | ✅ | Seznam joistů a deck |
| 3.8 | CONNECTIONS | ✅ | Detekce spojů, markery |
| 3.9 | CONNECTION_MARKERS | ✅ | Přiřazení kódu spoje |
| 3.10 | CONNECTION_LIST | ✅ | Přehled spojů (popup) |
| 3.11 | CONNECTION_SHEET | ✅ | Sheety ze šablon |
| 3.12 | TEMPLATE_CHECK | ✅ | Kontrola šablon |

---

## 4. Vrstvy

| # | Vrstva | Barva | Stav |
|---|--------|-------|------|
| 4.1 | JOISTS | červená | ✅ |
| 4.2 | DECK | zelená | ✅ |
| 4.3 | BEAMS | cyan | ✅ |
| 4.4 | WALLS | bílá | ✅ |
| 4.5 | COLUMNS | žlutá | ✅ |

---

## 5. Spoje (CONNECTIONS)

| # | Požadavek | Stav | Poznámka |
|---|-----------|------|----------|
| 5.1 | Typ A (deck–deck) | ✅ | ConnectionDetectionService |
| 5.2 | Typ B (joist–deck) | ✅ | |
| 5.3 | Typ C (joist–joist) | ✅ | |
| 5.4 | Unikátní Connection ID (CONN-1, …) | ✅ | ConnectionIdService |
| 5.5 | Kódy druhu spoje (A1, B3, C7, …) | ✅ | ConnectionLibrary |
| 5.6 | Markery (kruhy) ve výkresu | ✅ | ConnectionDrawingService |
| 5.7 | Dialog pro výběr kódu | ✅ | ConnectionCodeDialog |

---

## 6. Sheety ze šablon (CONNECTION_SHEET)

| # | Požadavek | Stav | Poznámka |
|---|-----------|------|----------|
| 6.1 | Šablony A, B, C (DWG) | ✅ | Templates/ |
| 6.2 | Kopírování layoutu ze šablony | ✅ | ConnectionLayoutService |
| 6.3 | Vyplnění CONNECTION_ID | ✅ | ConnectionAttributeService |
| 6.4 | Vyplnění CONNECTION_CODE | ✅ | |
| 6.5 | Parametrické kóty (DIM_*) z MORAVIO_SMART | ✅ | BuildDimensionValues |
| 6.6 | Geometrie v paper space | ✅ | Dle Konvence_sablon |

---

## 7. BOM

| # | Požadavek | Stav | Poznámka |
|---|-----------|------|----------|
| 7.1 | Export do Excel (.xlsx) | ✅ | BomExcelService, ClosedXML |
| 7.2 | Seznam joistů a deck | ✅ | BomService.CollectBomRows |
| 7.3 | Save File dialog | ✅ | ExportBomCommand |

---

## 8. Technické

| # | Požadavek | Stav | Poznámka |
|---|-----------|------|----------|
| 8.1 | AutoCAD 2026, .NET 8 | ✅ | |
| 8.2 | Deploy do ApplicationPlugins | ✅ | Release build |
| 8.3 | Všechny závislosti (ClosedXML, ExcelNumberFormat) | ✅ | CopyLocalLockFileAssemblies |
| 8.4 | AssemblyResolve pro závislosti | ✅ | PluginInit.cs |
| 8.5 | LoadOnAutoCADStartup | ✅ | PackageContents.xml |

---

## 9. Dokumentace

| # | Dokument | Stav |
|---|----------|------|
| 9.1 | Prehled_moznosti.md | ✅ |
| 9.2 | Prikazy.md | ✅ |
| 9.3 | Knihovna_nosniku.md | ✅ |
| 9.4 | Knihovna_plechu.md | ✅ |
| 9.5 | Knihovna_spoju.md | ✅ |
| 9.6 | Konvence_sablon.md | ✅ |
| 9.7 | Navod_priprava_šablon.md | ✅ |
| 9.8 | Priprava_sablon_na_transport.md | ✅ |

---

## Shrnutí

| Kategorie | Splněno | Celkem |
|-----------|---------|--------|
| Doména | 4 | 4 |
| XData | 6 | 6 |
| Příkazy | 10 | 10 |
| Vrstvy | 4 | 4 |
| Spoje | 7 | 7 |
| Sheety | 6 | 6 |
| BOM | 3 | 3 |
| Technické | 5 | 5 |
| Dokumentace | 8 | 8 |
| **Celkem** | **53** | **53** |

**Stav: 100 % splněno** (dle dostupné specifikace)

---

*Poznámka: Notion zadání nebylo nalezeno v workspace. Checklist vychází z konverzačního shrnutí, Prehled_moznosti.md a analýzy kódu. Pokud máte konkrétní Notion stránku se zadáním, uveďte URL pro doplnění checklistu.*
