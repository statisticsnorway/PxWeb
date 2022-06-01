# Filetypes In Filebased Px-Web 

# Files contain the data and metadata for a table
- The px-file itself. 
  https://www.scb.se/en/services/statistical-programs-for-px-files/px-file-format/
  
  PX-edit & job has a .pxk file, which is a px-file but without data and a .pxc for classifications.
  https://www.stat.fi/tup/tilastotietokannat/ohjeet_en.html
  

# Classifications
Files for classifications: .vs and .agg files  (valueset  and aggregation lists (groupings))
   If there are any valuesets with more than 1000 values: .VSC and .VSN files

For a given PX-file entries in the "-- select classification --" list come from 2 places.
- The "local" folder where the PX-file is. 
- The shared "Aggregations" folder (default path:  ../Resources/PC/Aggregations/ ) This may have subfolders

The use of classifiactions is triggered by the presence of a DOMAIN key word for the variable in the px-file:



DOMAIN("variable)=domainID

The valueset-file has  these sections:

```
[Descr]
Name=Name_inValueSetFile_Region1999/2003
Type=V
[Aggreg]
1=regional.agg
[Domain]
1=domain_region
[Valuecode]
1=0114
2=0115
[Valuetext]
1=Upplands Väsby
2=Vallentuna
```


The Desc/name is used in the .agg-files. The items in Aggreg are filenames of .agg-files. The Domain items provides connections to the px-files. Whats does Valuecode/Valuetext do?


So a valueSet belongs to one or more domains and has aggregations. If no valueSet for the domain are found in the local folder then any found when searching the sharded folder is used. Px-files has only one valueset for each variable. If more that one .vs-file with a matching domain is found in a folder, only the first is used. The others will only get log line like: 

... INFO  PCAxis.Paxiom.GroupRegistry - domain_region already loaded for ...

The name of the .vs file is not used, only the .vs -ending matters.

The valueset itself can not be selected by the user([current bug](https://github.com/statisticssweden/PxWeb/issues/209)), only the aggregations in the vs-file.  

The agg file looks like this:
```
[Aggreg]
Name=Regional
Valueset=Name_inValueSetFile_Region1999/2003
1=-14
2=15-19
[Aggtext]
1=-14
2=15-19
[-14]
1=-14
[15-19]
1=15
2=16
3=17
4=18
5=19
```
The aggreg section consists of Name which is displayed in the --Select classification -- dropdown, the valueset must match the valueset that links to it and a list of mothercodes. The Aggtext section holds the texts for the mothercodes.  Then each mother has a section listing its children.
 

PxWin has a editor for creation of classifications https://www.scb.se/globalassets/vara-tjanster/px-programmen/tutorial-pxwin_1.2_v1.pdf
  
  
# Menu related files. 
There is one Menu.xml file for each database, which is how PxWeb see the menu at runtime. 
These are generated (With a button click or http-call) and files like Alias_en.txt and Menu.sort influence the generation of the Menu.xml files. See  https://www.scb.se/globalassets/vara-tjanster/px-programmen/pxweb-configuration.pdf
- Alias_<LANG>.txt
By adding a file called Alias.txt (Alias_<LANG>.txt) to a folder it is
possible to change the name of the menu branch in the user interface.
If a folder contains a file called Alias.text when the Menu.xml file is
generated, the generator will use the text within the Alias.txt file as the
branch name instead of the name of the file system folder. 
(or if the language is missing from the filename the language will be set to default language of the PXWeb
installation. )

- Menu.<LANG>.sort
It is possible to make a custom sort order for the folders in a PX-file
database by using Menu.sort files. A folder will be sorted according to
the text in the Menu.sort file located within the folder.


- anyString<_lang>.Link
  These files has one line: "<display text>","<url>"
  PX\Databases\Example\alias uses this. (It is currently aliased to Population, there are 2 Population, this is the 2. :-))
  If the language is missing from the filename the language will be set to default
language of the PXWeb installation.
   
  
  
