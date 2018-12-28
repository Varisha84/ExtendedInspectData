# ExtendedInspectData
newest version: 1.2.1.2

Features

Single selection of zones to store things:
Adds a scrollable list of entries to the inspect window if a zone is selected. The list contains an icon, the label and the total number of things in the zone. 

Single/Multiple selection of items to store things:
Adds a scrollable list of entries to the inspect window if one or more items are selected. The list contains an icon, the label and the total number of things stored in the selected item(s). 
Due to dynamic height calculation there might be empty space in inspection window if less than 4 differen types of items stored.

Single selection of farm zones:
Adds a graph to the inspect window showing the distribution of growth values of the plants in the zone.
The graph contains these information:
 - x axis = growth value in percent (a plant has growth value of 0-100%)
 - y axis = percentage of total planted plants in the zone that reached growth value x (let's say i have like 8 percent of plants that reached growth value ~52% and 13% are around 100% growth value)
 - The white vertical line marks the growth value that is needed to harvest a plant.

Below the graph you find the following information from left to right
 - Total of cells with no plant in it (this includes cells that are marked as growing zone, but have a lamp on it or so)
 - Total cells with plants in it (any growth value)
 - Total number of plants that cannot be harvested yet (left of white line)
 - Total number of plants that can be harvested (right of white line)
 - Total number of fully grown plants (growth value = 100%)

Multiple selection of farm zones:
 - List can be sorted by fully grown plants or harvestable plants (descending only)
 - Single entry in list can be selected which results in single selection view of this zone.
