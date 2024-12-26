# EasyDbc

<p align="right">
  <a href="./README.en.md">English</a> |
  <a href="./README.zh.md">中文</a> |
  <a >Deutsch</a> |
  <a href="./README.kr.md">한국어</a> |
  <a href="./README.jp.md">日本語</a> |
  <a href="./README.fr.md">Français</a>
</p>

[![](https://img.shields.io/nuget/dt/EasyDbc?color=004880&label=downloads&logo=NuGet)](https://www.nuget.org/packages/EasyDbc/)
[![](https://img.shields.io/nuget/vpre/EasyDbc?color=%23004880&label=NuGet&logo=NuGet)](https://www.nuget.org/packages/EasyDbc/)
[![GitHub](https://img.shields.io/github/license/Vico-wu/EasyDbc?color=%231281c0)](LICENSE)

## Einführung  
Kurze Einführung in die Funktionen und Ziele des Projekts.  
Dieses Projekt basiert auf [`DbcParserLib`](https://github.com/EFeru/DbcParser) und erweitert dessen Funktionalitäten um folgende Features:

- **Zusammenführung von DBC-Dateien**：Unterstützung für das Zusammenführen mehrerer DBC-Dateien.
- **Erstellung von DBC-Dateien**：Möglichkeit, neue DBC-Dateien basierend auf Anforderungen zu generieren.
- **Konvertierung von Excel in DBC**：Funktion zur Analyse von Excel-Dateien und deren Umwandlung in DBC-Dateien, inklusive flexibler Zuordnung und Transformation.
- **Erstellung von Excel-Dateien**：Unterstützung der Generierung von Excel-Dateien aus DBC-Daten.
- **Benutzerdefinierte Excel-Analyse und -Generierung**：Flexible, anpassbare Logik für die Analyse und Generierung von Excel-Dateien, die verschiedenen Anforderungen gerecht wird.

Diese Erweiterungen machen das Projekt noch flexibler und effizienter bei der Verarbeitung von DBC- und Excel-Dateien und verbessern die Verwaltung von Fahrzeugnetzwerken sowie die Signalverarbeitung.


---

## Hauptfunktionen  
- **DBC-Dateien analysieren**：Extrahieren von Signalen, Nachrichten und Knotendaten. 
- **Datenanzeige**：Interaktive Benutzeroberfläche zur übersichtlichen Darstellung der Analyseergebnisse.
- **Formatvalidierung**：Flexible Analyse alter und neuer Excel-Formate.
- **Erweiterbarkeit**：Anpassbare Logik für die Analyse und Erstellung von Excel-Dateien. 

---
## Plugin-Referenz

Für die Implementierung der Excel-Analyse- und -Erstellungsfunktionen wurde das Plugin [`NPOI`](https://github.com/dotnetcore/NPOI) verwendet.`NPOI` ist eine Open-Source-Bibliothek für .NET,die das Verarbeiten von Microsoft Office-Dateien (einschließlich Excel, Word und PowerPoint) ermöglicht. Das Plugin ist leistungsstark und benutzerfreundlich und eignet sich besonders für die Bearbeitung von Excel-Dateien.

In diesem Projekt wurde `NPOI` für folgende Funktionen verwendet:

- **Gruppierung**:Daten können nach bestimmten Regeln gruppiert werden, um eine bessere Darstellung und Verwaltung zu ermöglichen.
- **Dropdown-Menüs**:Implementierung von Dropdown-Menüs in Excel-Zellen, um die Benutzereingabe zu vereinfachen und Datenkonsistenz zu gewährleisten.
- **Validierung**: Datenvalidierung, um sicherzustellen, dass die Daten in Excel-Dateien bestimmten Regeln entsprechen.
- **Hintergrundfarbe setzen**: Anpassung der Hintergrundfarbe von Excel-Zellen zur Verbesserung der Lesbarkeit und optischen Gestaltung.
- **Zellenformatierung**:Anpassung von Zellenformaten wie Zahlen- und Datumsformaten, um eine korrekte Darstellung der Daten zu gewährleisten.

Mit `NPOI` kann dieses Projekt Excel-Dateien flexibler verarbeiten, komplexe Anforderungen erfüllen und die Datenverarbeitungskapazität steigern.

---

## Projektstruktur  
```plaintext
Projektwurzelverzeichnis/
├── EasyDbc/                # Quellcode-Verzeichnis
│   ├── Assets/             # Bilder und andere Ressourcen
│   ├── Contracts/          # Schnittstellendateien
│   ├── Helpers/            # Erweiterungen
│   ├── Models/             # Datenmodelle
│   ├── Observers/          # Fehlerklassen
│   ├── Parsers/            # Parser-Klassen
│   ├── Generators/         # Datei-Generierungsklassen
│   └── EasyDbc.csproj      # Projektdatei
├── DbcFiles/               # Ressourcen-Dateien
├── EasyDbc.Benchmark/      # Leistungs-Benchmarking-Dateien
├── EasyDbc.Test/           # Unit-Test-Verzeichnis
├── EasyDbc.Demo/           # Beispielprojekt-Verzeichnis
├── README.zh_CN.md         # Projektbeschreibung auf Chinesisch
├── README.md               # Projektbeschreibung
└── LICENSE                 # Lizenzdatei
```
---

# Schnellstart

## Programm-Demo

Hier sind einige Funktionen und Screenshots des Projekts dargestellt.

**Beispielbeschreibung Screenshot**:

![Benutzeranleitung](https://raw.githubusercontent.com/Vico-wu/EasyDbc/master/EasyDbc/Assets/DemoDescription.jpg)


---

## Funktion 1: **DBC-Dateien analysieren**

Analyse von DBC-Dateien durch Angabe eines Pfads und erfolgreiches Laden des Inhalts.

**Beispielbeschreibung Screenshot**:

![DBC-Dateien analysieren](https://raw.githubusercontent.com/Vico-wu/EasyDbc/master/EasyDbc/Assets/Overview.jpg)

**Erläuterung**:
- Eingabe: Auswahl einer DBC-Datei im korrekten Format.
- Ausgabe: Erfolgreiche Analyse der Datei und Anzeige der Signalliste und Nachrichteninformationen.

---

## Funktion 2: **Excel-Dateien analysieren und generieren**

Unterstützt die Analyse von Excel-Dateien zu DBC-Datenmodellen sowie die Erstellung von Excel-Dateien nach Benutzeranforderungen.

**Beispielbeschreibung Screenshot**:

![Excel-Dateien analysieren](https://raw.githubusercontent.com/Vico-wu/EasyDbc/master/EasyDbc/Assets/ExcelView.jpg)

**Erläuterung**:
- Hauptmerkmale:
  - Unterstützung benutzerdefinierter Analysemethoden.
  - Erstellung von Excel-Dateien mit Dropdown-Menüs, Datenvalidierung und benutzerdefinierten Stilen.

---

## Funktion 3: **DBC-Dateien zusammenführen**

Zusammenführung mehrerer DBC-Dateien zu einer einzigen Datei unter Berücksichtigung von Konflikten und doppelten Signalen.

**Beispielbeschreibung Screenshot**:

![DBC-Dateien zusammenführen](https://raw.githubusercontent.com/Vico-wu/EasyDbc/master/EasyDbc/Assets/DbcMergeView.jpg)

**Erläuterung**:
- Eingabe: Mehrere DBC-Dateien, die zusammengeführt werden sollen.
- Ausgabe: Eine vollständige zusammengeführte DBC-Datei mit anpassbaren Zusammenführungsregeln.

---


## Zusammenfassung

Die obigen Beispiele zeigen einige der Hauptfunktionen des Projekts. Weitere Details und Anwendungsbeispiele finden Sie in den nachfolgenden Kapiteln.

### Hinzufügen von Namespaces

```cs
// Für die Generierung von DBC- und Excel-Dateien
using EasyDbc.Generators;
// Für die spezielle Signalverarbeitung, z. B.: signal.Parent.CycleTime(out var cycleTime);
using EasyDbc.Helpers;
// Assoziierte Modelle
using EasyDbc.Models;
// Für das Parsen von DBC- und Excel-Dateien
using EasyDbc.Parsers;
```
---

### Parsen von Dateien in das Dbc-Format

```cs
private bool TryParsingToFile(string path, out Dbc dbc)
{
    string extension = Path.GetExtension(path)?.ToLower();
    // DBC-Datei anhand des Dateipfads parsen
    if (extension == ".dbc")
    {
        dbc = Parser.ParseFromPath(path);
        if (dbc != null)
        {
            return true;
        }
        return true;
    }
    else if (extension == ".xls" || extension == ".xlsx")
    {
        // EXCEL-Datei anhand des Dateipfads parsen
        ExcelParser excelParser = new ExcelParser();
        ExcelParserState result = excelParser.ParseFirstSheetFromPath(path, out Dbc dbcOutput);
        if (result == ExcelParserState.Success)
        {
            dbc = dbcOutput;
            return true;
        }
    }
    dbc = null;
    return false;
}

```
---

### Funktion zum Zusammenführen von DBC-Dateien
**⚠️ Wichtiger Hinweis** 

Bei der aktuellen Zusammenführungsfunktion für DBC-Dateien hat die zuerst hinzugefügte Nachricht Vorrang. 
Spätere werden ignoriert.

```cs
private bool ParsingAndMergeDbc()
{
    Nodes = string.Empty;
    Messages.Clear();
    _mergedDbc = null;
    List<Dbc> parsingResult = new List<Dbc>();
    if (!string.IsNullOrEmpty(FilePath1))
    {
        if (TryParsingToFile(FilePath1, out Dbc dbc))
        {
            parsingResult.Add(dbc);
        }
    }
    if (!string.IsNullOrEmpty(FilePath2))
    {
        if (TryParsingToFile(FilePath2, out Dbc dbc))
        {
            parsingResult.Add(dbc);
        }
    }
    if (!string.IsNullOrEmpty(FilePath3))
    {
        if (TryParsingToFile(FilePath3, out Dbc dbc))
        {
            parsingResult.Add(dbc);
        }
    }
    bool result = DbcGenerator.MergeDbc(parsingResult, out _mergedDbc);
    if (result)
    {
        foreach (Node node in _mergedDbc.Nodes)
        {
            Nodes = string.Join("; ", _mergedDbc.Nodes.Select(node => node.Name));
        }
    }
    return result;
}

```
---
### **Parsen von DBC-Dateien**
Nutzen Sie die statische Klasse Parser, um DBC-Dateien zu parsen. Wählen Sie eine der folgenden Methoden:
```cs
// Parsen über den Dateipfad
Dbc dbc = Parser.ParseFromPath("C:\\your_dbc_file.dbc");
// Parsen über einen Stream
Dbc dbc = Parser.ParseFromStream(File.OpenRead("C:\\your_dbc_file.dbc")); 
// Parsen über einen Textstring
Dbc dbc = Parser.Parse("a dbc as string");

```
---

### **Arbeiten mit dem DBC-Dateiobjekt**

Das `DBC`-Objekt enthält zwei Sammlungen: `Messages` und `Nodes`. Beide sind vom Typ `IEnumerable<T>`, wodurch Sie mit LINQ darauf zugreifen, iterieren und Abfragen durchführen können.

Beispiel: Abrufen aller Nachrichten mit einer ID > 100 und mehr als 2 Signalen:
```cs
var filteredSelection = dbc
			.Messages
			.Where(m => m.ID > 100 && m.Signals.Count > 2)
			.ToArray();
```
---

### **Verwaltung von Parse-Fehlern**

Während des Parsens auftretende Syntaxfehler können dem Benutzer gemeldet werden. 
Das Interface `IParseFailureObserver` stellt Methoden zur Behandlung von Syntaxfehlern bereit, z. B.:
- Allgemeine Syntaxfehler (z. B. fehlendes ;, ', ,)
- Doppelte Objektdefinitionen (z. B. Nachrichten mit derselben ID; Knoten, Signale oder benutzerdefinierte Attribute mit demselben Namen)
- Fehlende Objektdefinitionen (z. B. wird ein benutzerdefiniertes Attribut zugewiesen, bevor es deklariert wurde)
- Konsistenz der Werte (z. B. Attributwerte außerhalb der im Attribut definierten Wertebereiche)

Zwei Implementierungen stehen zur Verfügung:

1. SilentFailureObserver：Standardimplementierung, die Fehler während des Parsens unterdrückt.
2. SimpleFailureObserver：Ein einfacher Beobachter, der alle Fehler aufzeichnet. 
    
    Fehlerliste von SimpleFailureObserver:
    - Unknown syntax: Keine entsprechende TAG-Syntax
    - [TAG] Syntax error: Syntaxfehler für einen bestimmten TAG
    - Duplicated object: Parser hat ein doppeltes Objekt gefunden und ignoriert
    - Object Not found: Ein Objekt wurde deklariert oder referenziert, bevor es definiert wurde
    - Property value out of bound: Zugewiesener Wert liegt außerhalb der minimalen/maximalen Grenzen
    - Property value out of index: Unzulässiger Indexwert (bei attributen wie Enum-Werten)

Fehlerliste abrufen:
```cs
    // Diese beiden Zeilen kommentieren, um die Fehlerverwaltung zu deaktivieren (Fehler werden stillschweigend behandelt).
    // Sie können Ihre eigene Implementierung von IParseFailureObserver bereitstellen.
    var failureObserver = new SimpleFailureObserver();
    Parser.SetParsingFailuresObserver(failureObserver);

    var dbc = Parser.ParseFromPath(filePath);
    var errors = failureObserver.GetErrorList();

```
---

### Signalverpackung und -entpackung

### **Einfache Anwendungsszenarien**

Für das Verpacken und Entpacken von Signalen können Sie die statische Klasse `Packer` verwenden. 
Beispiel für das Verpacken/Entpacken eines Signals: 14 Bits, minimaler Wert: `-61.92`, maximaler Wert: `101.91`.

```cs
Signal sig = new Signal
{
  sig.Length = 14,
  sig.StartBit = 2,
  sig.IsSigned = 1,
  sig.ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
  sig.Factor = 0.01,
  sig.Offset = 20
};

// Verpacken des Signals für die Übertragung
ulong TxMsg = Packer.TxSignalPack(-34.3, sig);

// Entpacken eines empfangenen Signals und Berechnung des physischen Werts
double val = Packer.RxSignalUnpack(TxMsg, sig);

```

Mehrere Signale vor der Übertragung über CAN verpacken:
```cs
ulong TxMsg = 0;
TxMsg |= Packer.TxSignalPack(value1, sig1);
TxMsg |= Packer.TxSignalPack(value2, sig2);
TxMsg |= Packer.TxSignalPack(value3, sig3);
// ...
// Senden von TxMsg über den CAN-Bus.

```

Stellen Sie sicher, dass Sie durch die korrekte Angabe von `Length` und `StartBit` Überlappungen zwischen den Signalen vermeiden.

### **Beispielanwendung für Multiplexing-Signale**
Eine Nachricht kann multiplexierte Daten enthalten, bei denen sich das Layout je nach Multiplexerwert ändert. 
Die Klasse `Packer` unterstützt Multiplexing nicht direkt. Benutzer müssen prüfen, ob die Nachricht das jeweilige Signal enthält.

Beispiel DBC-Inhalt:
```
BO_ 568 UI_driverAssistRoadSign: 8 GTW
 SG_ UI_roadSign M : 0|8@1+ (1,0) [0|0] ""  DAS
 SG_ UI_dummyData m0 : 8|1@1+ (1,0) [0|0] "" Vector__XXX
 SG_ UI_stopSignStopLineDist m1 : 8|10@1+ (0.25,-8) [-8|247.5] "m" Vector__XXX
```

Das Signal `UI_dummyData` ist nur verfügbar, wenn `UI_roadSign` den Wert 0 hat, während `UI_stopSignStopLineDist` nur verfügbar ist, wenn `UI_roadSign` den Wert 1 hat. 
Zugriff auf Multiplexing-Informationen:
```cs
var multiplexingInfo = signal.MultiplexingInfo();
if(multiplexingInfo.Role == MultiplexingRole.Multiplexor)
{
	// Dies ist ein Multiplexer!
}
else if(multiplexingInfo.Role == MultiplexingRole.Multiplexed)
{
	Console.WriteLine($"Dieses Signal ist multiplexiert und verfügbar, wenn der Multiplexerwert {multiplexingInfo.Group} ist");
}
```
Prüfen, ob eine Nachricht multiplexiert ist:
```cs
if(message.IsMultiplexed())
{
	// ...
}
```
---
# Beitrag

Beiträge sind willkommen! Reichen Sie gerne Pull Requests ein, um diese Bibliothek zu verbessern.

---