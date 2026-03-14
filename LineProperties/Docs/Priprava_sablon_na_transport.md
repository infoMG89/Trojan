# Příprava šablon na transport

Návod, jak připravit šablony spojů pro převoz na jiný počítač nebo pro sdílení s kolegy.

---

## 1. Co transportovat

Složka šablon:
```
C:\Users\marti\OneDrive\Documents_2\Cursor\AutoCAD\Templates\
├── Connection_A_Template.dwg   (plech–plech)
├── Connection_B_Template.dwg   (nosník–plech)
└── Connection_C_Template.dwg   (nosník–nosník)
```

Zkopírujte celou složku `Templates` včetně všech tří DWG souborů.

**Na cílovém počítači** musí být cesta k šablonám stejná, nebo upravte v kódu `ConnectionLayoutService.cs` konstantu `TemplatesPath`.

---

## 2. Struktura každé šablony – krok za krokem

### 2.1 Layout (paper space)

1. Otevřete šablonu v AutoCADu.
2. Zkontrolujte, že existuje **alespoň jeden layout** kromě Model (např. „Layout1“, „A0“, „Detail“).
3. **První layout** (v pořadí za Modelem) je ten, který plugin zkopíruje – jeho pořadí záleží na pořadí v Layout tabu.

### 2.2 Geometrie musí být v paper space

**Kritické:** Veškerá geometrie detailu spoje musí být přímo v **paper space layoutu**, ne v model space.

| Kde je geometrie | Výsledek po CONNECTION_LIST |
|------------------|----------------------------|
| V paper space layoutu | ✅ Zkopíruje se do sheetu |
| V model space + viewport | ❌ Prázdný sheet – viewport ukazuje na model cílového výkresu |

**Postup v AutoCADu:**
1. Přepněte na layout (klik na záložku Layout1 / A0 / …).
2. Ujistěte se, že kreslíte v **paper space** (ne uvnitř viewportu).
3. Všechny čáry, kruhy, bloky, texty umístěte přímo do tohoto layoutu.
4. Pokud máte viewport – může zůstat (prázdný), ale **geometrie detailu musí být mimo něj** v paper space.

**Rychlá kontrola:** V layoutu zvolte libovolnou entitu. V Properties by mělo být „Space: Paper“ (ne Model).

### 2.3 Blok odkazové značky (CONNECTION_ID)

1. **ATTDEF** – vytvořte atribut:
   - Tag: `CONNECTION_ID` nebo `ODKAZ`
   - Prompt: např. „ID spoje“
   - Default: `CONN-0`
2. **BLOCK** – vytvořte blok obsahující tento atribut (název libovolný, např. `ODKAZ_ZNAMKA`).
3. **INSERT** – vložte blok do layoutu na místo, kde má být odkazová značka (např. CONN-1, CONN-2, …).

Plugin při CONNECTION_LIST nahradí hodnotu atributu skutečným ID spoje.

### 2.4 Blok kódu druhu spoje (CONNECTION_CODE)

1. **ATTDEF** – atribut:
   - Tag: `CONNECTION_CODE` nebo `DRUH_SPOJE`
   - Default: např. `A0` / `B0` / `C0` podle typu šablony
2. **BLOCK** – blok s tímto atributem (název např. `CONNECTION_CODE`).
3. **INSERT** – vložte do layoutu tam, kde má být kód (A15, B3, C7, …).

### 2.5 Parametrické kóty (volitelné)

Pro automatické vyplnění rozměrů z LINEPROP:

- Vytvořte bloky s **přesným názvem** podle tabulky v `Konvence_sablon.md` (DIM_H1, DIM_B1, DIM_T1, …).
- Každý blok má jeden atribut s tagem `VALUE`.
- Vložte instance do layoutu na místa kót.

---

## 3. Kontrola před transportem

V AutoCADu s načteným pluginem spusťte příkaz **TEMPLATE_CHECK**.

Výstup by měl vypadat např.:
```
--- A (Connection_A_Template.dwg) ---
  Layouty (paper space): Layout1
  První layout: 'Layout1'
  Počet entit v layoutu: 12
  Typy entit: BlockReference=3 Viewport=1 Line=5 ...
  Bloky: ODKAZ_ZNAMKA, CONNECTION_CODE, DIM_H1
```

**Varování:** Pokud je „Počet entit v layoutu: 0“, layout je prázdný – geometrie je pravděpodobně v model space. Přesuňte ji do paper space.

---

## 4. Shrnutí checklistu

- [ ] Existuje layout (paper space) kromě Model
- [ ] Geometrie je v paper space, ne v model space
- [ ] Blok s atributem CONNECTION_ID nebo ODKAZ
- [ ] Blok s atributem CONNECTION_CODE nebo DRUH_SPOJE
- [ ] (Volitelně) Bloky DIM_* s atributem VALUE pro parametrické kóty
- [ ] TEMPLATE_CHECK ukazuje počet entit > 0
- [ ] Zkopírována celá složka Templates se všemi třemi DWG

---

## 5. Cílová cesta

Na novém počítači umístěte šablony např. do:
```
C:\Users\<uživatel>\...\AutoCAD\Templates\
```

A v `ConnectionLayoutService.cs` nastavte `TemplatesPath` na tuto cestu (nebo použijte relativní cestu, pokud ji plugin podporuje).
