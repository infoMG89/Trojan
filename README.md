# LineProperties – AutoCAD 2026 Plugin

Plugin pro úpravu vlastností úseček v AutoCADu 2026. Po výběru úsečky otevře dialogové okno, kde lze nastavit délku, hladinu, barvu, typ čáry a tloušťku čáry. Při změně délky zůstává střed úsečky fixní.

## Požadavky

- AutoCAD 2026
- .NET 8 SDK (pro build)
- Visual Studio 2022 17.8+ (doporučeno) nebo `dotnet build`

## Build

**Kompilace a nasazení (doporučeno: zavřete AutoCAD před buildem):**
```bash
dotnet build LineProperties.sln -c Release
```
Release build zároveň zkopíruje plugin do `%APPDATA%\Autodesk\ApplicationPlugins\LineProperties.bundle\`. Po buildu restartujte AutoCAD, aby načetl novou verzi. (Během běhu AutoCADu nelze přepsat načtenou DLL.)

Pouze kompilace bez deploye: `dotnet build LineProperties.sln -c Debug` (výstup jen do `bin\Debug\...`).

> **Poznámka:** Cesty k AutoCAD DLL jsou v `LineProperties.csproj` nastaveny na `C:\Program Files\Autodesk\AutoCAD 2026\`. Pokud máte AutoCAD jinde, upravte `HintPath`.

## Načtení pluginu do AutoCADu

### Automatické načtení (doporučeno)

Při každém **Release** buildu se plugin zkopíruje sem:

```
%APPDATA%\Autodesk\ApplicationPlugins\LineProperties.bundle\Contents\LineProperties.dll
```

AutoCAD 2026 načte tento plugin **automaticky při startu**. Nemusíte nic loadovat ručně.

**Workflow při ladění:** zavřete AutoCAD → upravíte kód → `dotnet build -c Release` (build + deploy) → spusťte AutoCAD = běží nová verze.

### Ruční načtení (NETLOAD)

Pokud chcete loadovat jen občas ručně: v AutoCADu napište `NETLOAD` a vyberte `LineProperties.dll` (např. z `LineProperties\bin\Release\net8.0-windows\` nebo z `%APPDATA%\Autodesk\ApplicationPlugins\LineProperties.bundle\Contents\`).

### Příkaz LINEPROP se neobjevuje

1. **Zkuste napsat do příkazového řádku přímo `LINEPROP`** – příkaz může být načtený, ale nemusí být v CUI/ribbonu.
2. **Ruční načtení:** V AutoCADu zadejte `NETLOAD` a vyberte `LineProperties.dll` z `%APPDATA%\Autodesk\ApplicationPlugins\LineProperties.bundle\Contents\`. Pokud se zobrazí chyba, plugin se při automatickém startu nenačetl (např. chyba v kódu nebo .NET).
3. **Restart AutoCADu** – po nasazení nové verze (DeployToAutoCAD) musíte AutoCAD úplně zavřít a znovu spustit, aby se načetla nová DLL.
4. **Kontrola složky:** Ověřte, že existuje `%APPDATA%\Autodesk\ApplicationPlugins\LineProperties.bundle\PackageContents.xml` a `...\Contents\LineProperties.dll`.

## Použití

1. Nakreslete úsečku standardním způsobem (příkaz `LINE`)
2. Napište do příkazového řádku: `LINEPROP`
3. Klikněte na úsečku, kterou chcete upravit
4. V dialogovém okně upravte požadované vlastnosti:
   - **Délka** – nová délka v mm (střed úsečky zůstane na místě)
   - **Hladina** – výběr z existujících hladin
   - **Barva** – ACI index (0 = ByBlock, 256 = ByLayer, 1–255 = konkrétní barva)
   - **Typ čáry** – výběr z načtených typů čar
   - **Tloušťka čáry** – standardní hodnoty AutoCADu
5. Potvrďte tlačítkem **OK**

### Kóty u objektů (nosník / plech)

LINEPROP přiřadí objektu kótu délky (u plechu dvě kóty). Plugin nastaví DIMASSOC=2 a po vytvoření kót spustí **DIMREASSOCIATE** s předvybranými kótami a souřadnicemi koncových bodů, takže kóty by měly být asociativní a putovat s objektem při přesunu. Pokud kóta s objektem stále neputuje (např. jiná verze AutoCADu):

1. Zadejte příkaz **DIMREASSOCIATE**
2. Vyberte kótu (kóty)
3. U každé kóty zadejte body přiřazení – klikněte na koncové body úsečky nebo hrany plechu (zachytit *Konec*)
4. Kóta bude asociativní a při přesunu objektu se přepočítá / posune s ním

### Seznamy

- **LINEPROPLIST** – seznam všech prvků s LINEPROP ID (nosníky, plechy)
- **CONNECTION_MARKERS** – seznam všech markerů spojů (CONN-1, CONN-2, …) s typem, kódem a popisem

## Nastavení klávesové zkratky (Ctrl+Shift+L)

1. V AutoCADu otevřete: **Manage** → **Customize User Interface** (nebo napište `CUI`)
2. V levém panelu rozbalte **Keyboard Shortcuts** → **Shortcut Keys**
3. Klikněte pravým tlačítkem → **New Shortcut Key**
4. V poli **Command** vyhledejte `LINEPROP`
5. V poli **Keys** stiskněte `Ctrl+Shift+L`
6. Potvrďte tlačítkem **Apply** a **OK**

## Struktura projektu

```
LineProperties.sln
LineProperties/
  LineProperties.csproj           – projekt (.NET 8, WPF)
  Commands/
    LinePropertiesCommand.cs      – registrace příkazu LINEPROP
  UI/
    LinePropertiesDialog.xaml      – WPF dialog
    LinePropertiesDialog.xaml.cs   – code-behind
  Models/
    LinePropertyModel.cs           – datový model
  Services/
    LineModifier.cs                – logika změny délky
```

`PackageContents.xml` je v projektu (`LineProperties/PackageContents.xml`) a při Release buildu se spolu s DLL automaticky zkopíruje do `%APPDATA%\Autodesk\ApplicationPlugins\LineProperties.bundle\`.
