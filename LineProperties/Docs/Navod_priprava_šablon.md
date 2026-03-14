# Podrobný návod: Příprava šablon pro CONNECTION_LIST

Kompletní návod, jak vytvořit a připravit šablony spojů pro příkaz CONNECTION_LIST.

---

## 1. Umístění a soubory

**Složka:**
```
C:\Users\marti\OneDrive\Documents_2\Cursor\AutoCAD\Templates\
```

**Tři šablony:**
| Soubor | Typ spoje |
|--------|-----------|
| Connection_A_Template.dwg | plech–plech |
| Connection_B_Template.dwg | nosník–plech |
| Connection_C_Template.dwg | nosník–nosník |

---

## 2. Struktura šablony – základní pravidla

### 2.1 Layout (paper space)

- Každá šablona musí mít **alespoň jeden layout** kromě Model (např. Layout1, A0, Detail).
- Plugin zkopíruje **první neprázdný layout** – pokud je Layout1 prázdný, použije se Layout2 nebo další.

### 2.2 Geometrie v paper space

**Kritické:** Veškerá geometrie detailu spoje musí být přímo v **paper space layoutu**, ne v model space.

| Kde je geometrie | Výsledek |
|------------------|----------|
| V paper space layoutu | ✅ Zkopíruje se do sheetu |
| V model space + viewport | ❌ Prázdný sheet – viewport ukazuje na model cílového výkresu |

**Proč:** Viewport ukazuje na model space cílového výkresu, ne šablony. Geometrie v model space šablony se tedy nezkopíruje.

---

## 3. Postup vytvoření šablony – krok za krokem

### Krok 1: Nový výkres

1. Otevřete AutoCAD.
2. Vytvořte nový výkres nebo otevřete existující šablonu.

### Krok 2: Layout

1. Přepněte na záložku **Layout1** (ne Model).
2. Pokud layout neexistuje: pravý klik na záložku Model → New Layout.

### Krok 3: Geometrie v paper space

1. Ujistěte se, že kreslíte v **paper space** (ne uvnitř viewportu).
2. **Kontrola:** Dvojklik **mimo** viewport (do šedé oblasti) – tím jste v paper space.
3. Nakreslete geometrii detailu spoje přímo v layoutu:
   - průřezy nosníků (polyline, čáry)
   - plechy
   - kóty
   - texty

**Pokud máte geometrii v Model:** Zkopírujte ji do layoutu:
- Záložka Model → vyberte vše → Ctrl+C
- Záložka Layout1 → dvojklik mimo viewport → Ctrl+V
- Případně upravte měřítko (SCALE)

### Krok 4: Blok odkazové značky (CONNECTION_ID)

1. Příkaz **ATTDEF** (Attribute Definition):
   - **Tag:** `CONNECTION_ID` nebo `ODKAZ` *(přesně takto)*
   - **Prompt:** např. „ID spoje“
   - **Default:** `CONN-0`
   - **Mode:** zrušte zaškrtnutí „Constant“ (atribut musí být editovatelný)

2. Příkaz **BLOCK**:
   - Vyberte atribut + případně další geometrii
   - Název bloku: libovolný (např. `ODKAZ_ZNAMKA`)

3. Příkaz **INSERT**:
   - Vložte blok do layoutu na místo odkazové značky

**Výsledek:** Plugin při CONNECTION_LIST nastaví hodnotu na CONN-1, CONN-2, … podle spoje.

### Krok 5: Blok kódu druhu spoje (CONNECTION_CODE)

1. Příkaz **ATTDEF**:
   - **Tag:** `CONNECTION_CODE` nebo `DRUH_SPOJE` nebo `CODE` *(přesně)*
   - **Prompt:** např. „kód spoje“
   - **Default:** `A0` (typ A), `B0` (typ B) nebo `C0` (typ C) podle šablony
   - **Mode:** zrušte „Constant“

2. Příkaz **BLOCK**:
   - Vyberte atribut
   - Název bloku: `CONNECTION_CODE` (doporučeno – plugin pak najde blok i podle názvu)

3. Příkaz **INSERT**:
   - Vložte blok do layoutu tam, kde má být kód (A15, B3, C7, …)

**Důležité:** Tag atributu musí být CONNECTION_CODE, ne výchozí hodnota A0. V Properties u bloku zkontrolujte, že atribut má „Tag: CONNECTION_CODE“.

**Výsledek:** Plugin nastaví hodnotu na kód, který uživatel vybral při kliknutí na marker v příkazu CONNECTIONS.

### Krok 6: Parametrické kóty (volitelné)

Pro automatické vyplnění rozměrů z LINEPROP:

1. Vytvořte bloky s **přesným názvem** podle tabulky:

**Nosník:**
| Název bloku | Rozměr |
|-------------|--------|
| DIM_H1, DIM_H2 | výška h | mm |
| DIM_B1, DIM_B2 | šířka příruby b | mm |
| DIM_TW1, DIM_TW2 | tloušťka stojiny tw | mm |
| DIM_TF1, DIM_TF2 | tloušťka příruby tf | mm |

**Plech:**
| Název bloku | Rozměr |
|-------------|--------|
| DIM_L1, DIM_L2 | délka L | mm |
| DIM_B1_1, DIM_B1_2 | šířka horní hrany b1 | mm |
| DIM_B2_1, DIM_B2_2 | šířka dolní hrany b2 | mm |
| DIM_T1, DIM_T2 | tloušťka t | mm |

2. Každý blok má jeden atribut s tagem **VALUE**.
3. Vložte instance do layoutu na místa kót.

### Krok 7: Uložení

1. Uložte jako `Connection_A_Template.dwg`, `Connection_B_Template.dwg` nebo `Connection_C_Template.dwg`.
2. Umístěte do složky Templates.

---

## 4. Kontrola

V AutoCADu s načteným pluginem spusťte příkaz **TEMPLATE_CHECK**.

**Očekávaný výstup:**
```
--- C (Connection_C_Template.dwg) ---
  Layouty (paper space): Layout1
  Layout 'Layout1' [POUŽÍVÁ SE]: 12 entit (BlockReference=3, Line=5, ...)
```

**Problémy:**
- „Počet entit: 0“ nebo „jen Viewport“ → geometrie je v model space, přesuňte do paper space
- „Model space: 8 entit“ → geometrie je v Model, zkopírujte do Layout1

---

## 5. Checklist před použitím

- [ ] Existuje layout (paper space) kromě Model
- [ ] Geometrie je v paper space, ne v model space
- [ ] Blok s atributem **CONNECTION_ID** nebo **ODKAZ** (tag, ne výchozí hodnota)
- [ ] Blok s atributem **CONNECTION_CODE**, **DRUH_SPOJE** nebo **CODE** (tag, ne výchozí hodnota)
- [ ] (Volitelně) Bloky DIM_* s atributem VALUE
- [ ] TEMPLATE_CHECK ukazuje počet entit > 0 v layoutu

---

## 6. Časté chyby

| Chyba | Řešení |
|-------|--------|
| Prázdný sheet po CONNECTION_LIST | Geometrie je v model space – přesuňte do paper space |
| Kód zůstává A0 místo C7 | Atribut má špatný tag – musí být CONNECTION_CODE, ne výchozí hodnota |
| Text „CONNECTION_CODE“ se nemění | Je to obyčejný text (MText/Text), ne atribut bloku – vytvořte blok s atributem |
| Layout se nezkopíruje | Zkontrolujte cestu k šablonám v ConnectionLayoutService.cs |
