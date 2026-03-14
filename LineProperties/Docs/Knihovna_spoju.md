# Knihovna spojů

Pro CONNECTIONS: druhy spojů podle typu kontaktu (A = plech–plech, B = nosník–plech, C = nosník–nosník) a kombinace typů z knihoven nosníků a plechů. Buňka tabulky = kód druhu spoje (A1, A15, B3, C7, …).

## Referenční seznam kódů (kód → popis)

| Kód | Popis |
|-----|--------|
| A1 | Svařované tupo |
| A2 | Svařované na tupo s výztuhou |
| A15 | Šroubované M12 |
| A3 | Lepené |
| A4 | Jiné (plech–plech) |
| B1 | Svařované koutové |
| B3 | Šroubované přes styčník |
| B7 | Nýtované |
| B2 | Svařované čelně |
| B4 | Jiné (nosník–plech) |
| C1 | Styčník šroubovaný |
| C7 | Svařovaný styčník |
| C2 | Čepový spoj |
| C3 | Překryv svařovaný |
| C4 | Jiné (nosník–nosník) |

## Tabulka typu A (plech – plech)

Řádky = typ plechu 1, sloupce = typ plechu 2. Buňka = kód spoje.

|       | TP-1 | TP-2 | TP-3 | TP-4 | TP-5 |
|-------|------|------|------|------|------|
| TP-1  | A1   | A15  | A1   | A15  | A2   |
| TP-2  | A15  | A1   | A15  | A1   | A15  |
| TP-3  | A1   | A15  | A1   | A2   | A1   |
| TP-4  | A15  | A1   | A2   | A1   | A15  |
| TP-5  | A2   | A15  | A1   | A15  | A1   |

**Plugin pokrývá všechny kombinace TP-1..TP-10** (10×10 = 100 buněk). Pro TP-6..TP-10 se použije vzor z tabulky (A1, A15, A2, A3).

## Tabulka typu B (nosník – plech)

Řádky = typ nosníku, sloupce = typ plechu. **U každé kombinace má uživatel na výběr 5 možností** (B1, B3, B7, B2, B4).

|       | TP-1 | TP-2 | TP-3 | TP-4 | TP-5 |
|-------|------|------|------|------|------|
| I 100 | B1   | B3   | B1   | B7   | B2   |
| I 200 | B3   | B1   | B7   | B1   | B3   |
| I 240 | B1   | B7   | B1   | B3   | B1   |
| I 300 | B7   | B1   | B3   | B1   | B7   |

**Plugin pokrývá všechny kombinace** – 12 nosníků × 10 plechů = 120 buněk. Pro I 120, 140, 160, 180, 220, 260, 360, 400 a TP-6..TP-10 se použije vzor z tabulky.

## Tabulka typu C (nosník – nosník)

Řádky = typ nosníku 1, sloupce = typ nosníku 2. **U každé kombinace má uživatel na výběr 5 možností** (C1, C7, C2, C3, C4).

|       | I 100 | I 200 | I 240 | I 300 |
|-------|-------|-------|-------|-------|
| I 100 | C1    | C7    | C1    | C2    |
| I 200 | C7    | C1    | C7    | C1    |
| I 240 | C1    | C7    | C1    | C3    |
| I 300 | C2    | C1    | C3    | C1    |

**Plugin pokrývá všechny kombinace** – 12×12 = 144 buněk (všech 12 nosníků). Pro I 120, 140, 160, 180, 220, 260, 360, 400 se použije vzor z tabulky.
