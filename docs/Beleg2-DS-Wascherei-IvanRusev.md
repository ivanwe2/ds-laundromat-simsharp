# [Beleg 2] Discrete Event Simulation einer Wascherei

*Ersteller: Ivan Rusev 201222009*  
*Datum: 16.03.25*

## Gliederung
- [Aufgabenstellung](#aufgabe)
- [Discrete Event Simulation](#discrete-simulation)
- [Simulation](#simulation)
    - [Parameter](#parameter)
    - [Implementierung](#implementierung)
- [Ergebnisse](#ergebnisse)
    - [Diagramme](#ergebnisse-charts)
    - [Jahresübersicht](#ergebnisse-jahresübersicht)
- [Code](../src/DiscreteSim.Wascherei/Services/SimulationService.cs)
- [Fazit](#fazit)

## <a id="aufgabe">Aufgabenstellung</a>

Ziel dieser Simulation ist es, den Betrieb einer Wascherei im Studentenwohnheim mit Hilfe einer diskreten Ereignissimulation zu modellieren. Im Unterschied zur Monte Carlo Simulation wird hier der zeitliche Ablauf eines ganzen Jahres simuliert, wobei saisonale Schwankungen und unterschiedliche Kundenfrequenzen an verschiedenen Wochentagen berücksichtigt werden.

**Szenario**:

Ein Wäschereibetreiber in einem Studentenwohnheim möchte die Wirtschaftlichkeit seines Geschäfts über ein ganzes Jahr analysieren und optimieren. Er verfügt über 20 Waschmaschinen mit einer täglichen Betriebszeit von 24 Stunden. Im Durchschnitt nutzt ein Kunde eine Maschine für etwa 4 Stunden. Die durchschnittliche Zielkundenzahl liegt bei 100 pro Tag, schwankt jedoch je nach Monat und Wochentag.

Das Hauptproblem besteht darin, dass an Wochenenden zu viele Kunden kommen, wodurch nicht alle bedient werden können und potenzielle Einnahmen verloren gehen. Als Lösung wird ein Rabattsystem vorgeschlagen: Rabatte an Wochentagen, um Kunden vom Wochenende umzuverteilen und die Gesamtauslastung zu optimieren.

## <a id="discrete-simulation">Discrete Event Simulation</a>

Im Gegensatz zu kontinuierlichen Simulationen, bei denen Zustandsänderungen kontinuierlich über die Zeit verfolgt werden, konzentriert sich die diskrete Ereignissimulation (Discrete Event Simulation, DES) auf spezifische Ereignisse, die zu bestimmten Zeitpunkten auftreten. Diese Simulationsmethode ist besonders geeignet für Systeme, deren Zustand sich nur zu bestimmten Zeitpunkten ändert.

**Grundprinzipien der diskreten Ereignissimulation**:

1. **Ereignisbasierter Ansatz**: Das System verändert sich nur, wenn ein Ereignis eintritt. Die Zeit zwischen Ereignissen wird "übersprungen".

2. **Simulationsuhr**: Eine zentrale Uhr verfolgt die Simulationszeit und springt von Ereignis zu Ereignis.

3. **Ereignisliste**: Eine geordnete Liste aller geplanten Ereignisse, sortiert nach dem Zeitpunkt ihres Auftretens.

4. **Zustandsvariablen**: Variablen, die den Zustand des Systems zu einem bestimmten Zeitpunkt beschreiben.

5. **Statistische Zähler**: Sammeln Daten während der Simulation für die spätere Analyse.

In unserem Fall werden täglich neue Ereignisse generiert, die die Kundenankünfte, Maschinennutzung und finanziellen Transaktionen darstellen. Das SimSharp-Framework bietet eine effiziente Implementierung für diesen Ansatz, indem es Ereignisse plant und verarbeitet, während die Simulationszeit voranschreitet.

## <a id="simulation">Simulation</a>

Die diskrete Ereignissimulation modelliert den Betrieb der Wascherei über ein komplettes Jahr (2025) und vergleicht zwei Szenarien:
1. Standardpreise ohne Rabatte
2. Rabatte an Wochentagen zur besseren Kundenverteilung

### <a id="parameter">Parameter</a>

Die Simulation verwendet folgende Hauptparameter:
- **Anzahl der Maschinen**: 20
- **Betriebsstunden pro Tag**: 24
- **Durchschnittliche Nutzungszeit pro Kunde**: 4 Stunden
- **Basis-Kundenziel pro Tag**: 100
- **Durchschnittliche Ausgaben pro Student**: $8.00
- **Standardabweichung der Ausgaben**: $2.00
- **Tägliche Fixkosten**: $100.00
- **Variabler Kostenfaktor**: 0.3 (30% vom Umsatz)
- **Wöchentliche Wartungskosten**: $150.00 (jeden Montag)

**Saisonale Faktoren**:
- Unterschiedliche monatliche Faktoren, die die Kundenzahl beeinflussen (höher während des Semesters, niedriger in Ferienzeiten)
- Verstärkungsfaktoren für Wochenenden (1.4 ohne Rabatt)
- Verstärkungsfaktoren für Wochentage mit Rabatt (1.1)
- Verstärkungsfaktoren für Wochenenden mit Rabatt (1.2)

### <a id="implementierung">Implementierung</a>

Die Simulation wurde mit C# und dem SimSharp-Framework implementiert. SimSharp ist eine diskrete Ereignissimulationsbibliothek, die auf dem Python-Framework SimPy basiert.

**Kernkomponenten des Codes**:

1. **SimulationService**: Hauptklasse, die den Simulationsablauf steuert
   - Initialisiert die Simulation mit den definierten Parametern
   - Verwendet Wahrscheinlichkeitsverteilungen für realistische Schwankungen:
     - Normalverteilung für Kundenausgaben
     - Gleichverteilung für tägliche Variationen

2. **Simulationsablauf**:
   - Simuliert jeden Tag des Jahres sequentiell
   - Berechnet die verfügbare Maschinenkapazität pro Tag
   - Bestimmt die Anzahl der potenziellen Kunden basierend auf:
     - Monatsabhängigen Faktoren (Semesterzeiten vs. Ferien)
     - Wochentagsabhängigen Faktoren
     - Zufälligen Schwankungen

3. **Tagesablauf**:
   - Berechnet die tatsächliche Anzahl der bedienten Kunden basierend auf Maschinenkapazität
   - Erfasst verlorene Kunden und entgangenen Umsatz
   - Berechnet die Maschinenauslastung
   - Wendet Preismodifikatoren bei aktivierten Rabatten an
   - Berechnet Tagesumsatz, -kosten und -gewinn
   - Berücksichtigt wöchentliche Wartungskosten (montags)

4. **Statistikerfassung**:
   - Sammelt tägliche Daten
   - Aggregiert diese zu monatlichen und jährlichen Statistiken
   - Ermöglicht den Vergleich der beiden Strategien

## <a id="ergebnisse">Ergebnisse (Beispiel)</a>

Die Simulation vergleicht die beiden Strategien über ein ganzes Jahr und liefert umfassende Daten zur Entscheidungsfindung.

![Results](/assets/results-0.png)

### <a id="ergebnisse-charts">Diagramme</a>

Die Simulationsergebnisse werden in vier Hauptdiagrammen visualisiert:

1. **Monatlicher Umsatzvergleich**:  
   Vergleich des monatlichen Umsatzes zwischen der Standardstrategie und der Rabattstrategie an Wochentagen.

2. **Monatlicher Gewinnvergleich**:  
   Vergleich des monatlichen Gewinns zwischen beiden Strategien.

3. **Monatliche Kundenverteilung**:  
   Zeigt die Anzahl der bedienten und verlorenen Kunden pro Monat für beide Strategien.

4. **Monatliche Maschinenauslastung**:  
   Vergleicht die prozentuale Auslastung der Maschinen über das Jahr.

Die Diagramme zeigen deutliche saisonale Schwankungen, wobei in den Hauptsemestermonaten sowohl Umsatz als auch Maschinenauslastung am höchsten sind. Die Rabattstrategie führt zu einer gleichmäßigeren Verteilung der Kunden über die Woche.

### <a id="ergebnisse-jahresübersicht">Jahresübersicht</a>

Die Jahresübersicht fasst die wichtigsten Kennzahlen zusammen:

| Metrik | Standard-Preisstrategie | Wochentags-Rabattstrategie |
|--------|------------------------|----------------------------|
| Gesamtumsatz | $266,222.18 | $256,046.47 |
| Gesamtkosten | $124,766.65 | $120,813.88 |
| Gesamtgewinn | $141,455.53 | $135,232.59 |
| Gesamtkunden | 34672 | 34915 |
| Verlorene Kunden | 3262 | 2503 |
| Verlorener Umsatz | $24,903.10 | $21,364.72 |
| Durchschnittliche Maschinenauslastung | 77.05% | 79.71% |

**Analyse der Ergebnisse**:

Die Rabattstrategie führt zu:
1. Einem leicht geringeren Gesamtumsatz (-3.8%)
2. Geringeren Gesamtkosten (-3.2%)
3. Einem etwas niedrigeren Gesamtgewinn (-4.4%)
4. Mehr bedienten Kunden (+0.7%)
5. Deutlich weniger verlorenen Kunden (-23.3%)
6. Reduziertem verlorenen Umsatz (-14.2%)
7. Höherer durchschnittlicher Maschinenauslastung (+2.7%)

## <a id="fazit">Fazit</a>

Die diskrete Ereignissimulation ermöglicht eine detaillierte Analyse des Wäschereibetriebs über ein ganzes Jahr. Obwohl die Rabattstrategie zu einem etwas geringeren Gesamtgewinn führt, bietet sie langfristige Vorteile:

1. **Bessere Kundenverteilung**: Die Umverteilung von Kunden vom Wochenende auf Wochentage führt zu einer höheren Gesamtanzahl bediente Kunden.

2. **Effizienzsteigerung**: Die höhere Maschinenauslastung bedeutet eine bessere Nutzung der vorhandenen Ressourcen.

3. **Kundenzufriedenheit**: Weniger abgewiesene Kunden bedeuten potenziell höhere Kundenzufriedenheit und Kundenbindung.

4. **Risikominimierung**: Die gleichmäßigere Verteilung der Kunden über die Woche reduziert Überlastungsspitzen und damit verbundene operative Risiken.

Die Entscheidung zwischen den beiden Strategien hängt letztendlich von den Prioritäten des Wäschereibetreibers ab. Liegt der Fokus auf maximalen kurzfristigen Gewinnen, ist die Standardstrategie vorzuziehen. Für langfristige Stabilität, höhere Kundenzufriedenheit und optimale Ressourcennutzung bietet die Rabattstrategie trotz des etwas geringeren Gewinns erhebliche Vorteile.
