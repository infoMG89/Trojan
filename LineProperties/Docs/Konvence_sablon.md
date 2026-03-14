# Konvence šablon pro CONNECTION_SHEET

## Umístění šablon

`C:\Users\marti\OneDrive\Documents_2\Cursor\AutoCAD\Templates`

- `Connection_A_Template.dwg` – spoj plech–plech
- `Connection_B_Template.dwg` – spoj nosník–plech
- `Connection_C_Template.dwg` – spoj nosník–nosník

## Struktura šablony

V každé šabloně musí být alespoň jeden layout (paper space) kromě Model. Příkaz CONNECTION_SHEET zkopíruje obsah prvního takového layoutu.

**Důležité:** Veškerá geometrie detailu spoje (průřezy nosníků, plechy, texty, bloky) musí být v **paper space layoutu**, nikoli v model space. Viewport zobrazující model space by po zkopírování ukazoval na model space cílového výkresu, ne šablony – proto by geometrie nebyla vidět.

## Odkazová značka

Vložte blok s atributem, jehož tag je `CONNECTION_ID` nebo `ODKAZ`. Plugin při CONNECTION_LIST nastaví hodnotu atributu na Connection ID (např. CONN-1).

**Postup v AutoCADu:**
1. Vytvořte blok s jedním atributem (tag: CONNECTION_ID, výchozí: CONN-0)
2. Vložte instanci bloku do layoutu šablony na místo odkazové značky

## Kód druhu spoje

Vložte blok s atributem. Plugin při CONNECTION_LIST nastaví hodnotu na kód druhu spoje (např. A15, B3, C7), který uživatel vybral při kliknutí na marker v příkazu CONNECTIONS.

**Atribut:** Tag musí být `CONNECTION_CODE`, `DRUH_SPOJE` nebo `CODE`. Alternativně: blok pojmenovaný `CONNECTION_CODE` – plugin aktualizuje první atribut.

**Postup v AutoCADu:**
1. Příkaz ATTDEF – vytvořte atribut (tag: **CONNECTION_CODE**, prompt: kód spoje, výchozí: A0)
2. Příkaz BLOCK – vytvořte blok obsahující tento atribut (název např. CONNECTION_CODE)
3. Vložte instanci bloku do layoutu šablony na místo, kde má být kód spoje zobrazen

Pokud uživatel nevybral druh spoje u markeru, zobrazí se prázdný řetězec.

## Parametrické kóty

Pro automatické vyplnění rozměrů z LINEPROP použijte bloky s atributem tag `VALUE`. Název bloku určuje, který rozměr se vyplní:

### Nosník (beam)

| Název bloku | Rozměr | Jednotka |
|-------------|--------|----------|
| DIM_H1, DIM_H2 | výška h | mm |
| DIM_B1, DIM_B2 | šířka příruby b | mm |
| DIM_TW1, DIM_TW2 | tloušťka stojiny tw | mm |
| DIM_TF1, DIM_TF2 | tloušťka příruby tf | mm |

Index 1 = první nosník ve spoji, 2 = druhý (u typu C).

### Plech (plate)

| Název bloku | Rozměr | Jednotka |
|-------------|--------|----------|
| DIM_L1, DIM_L2 | délka L | mm |
| DIM_B1_1, DIM_B1_2 | šířka horní hrany b1 | mm |
| DIM_B2_1, DIM_B2_2 | šířka dolní hrany b2 | mm |
| DIM_T1, DIM_T2 | tloušťka t | mm |

**Postup:** Vytvořte bloky (např. DIM_H1, DIM_B1, …) s jedním atributem tag VALUE. Při CONNECTION_SHEET plugin vyplní hodnotu podle LINEPROP entit připojených ke spoji.
