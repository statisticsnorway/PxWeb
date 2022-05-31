# Filetypes In Filebased Px-Web 

# Files contain the data and metadata for a table
- The px-file itself. 
  https://www.scb.se/en/services/statistical-programs-for-px-files/px-file-format/

# Classifications
Files for classifications: .vs and .agg files  (valueset  and aggregation lists (groupings))
   If there are any valuesets with more than 1000 values: .VSC and .VSN files

For a given PX-file entries in the "-- select classification --" list come from 2 places.
- The "local" folder where the PX-file is. 
- The shared "Aggregations" folder (default path:  ../Resources/PC/Aggregations/ 

The use of classifiactions is triggered by the presence of a DOMAIN key word for the variable in the px-file:
DOMAIN("variable)=domainID

If no aggregations for the domain are found in the local folder then any sharded aggregations for the domain is used.
 
At application startup infomation from the shared folder is read and stored by domain.
Note the the name of the .vs file is not used, only the .vs -ending matters.

All .vs-files 

  
  If domain is present, the directory of the px-file (what about the common?) is searach for .vs files with a matching domain (seems the vs-file may have multiple domains.)
  Only the first encountered file with match is loaded. Any others will only get log line like: 
INFO  PCAxis.Paxiom.GroupRegistry - domain_region already loaded for
   
  (?)The valueset itself can not be selected by the user,(?) only the aggregations in the vs-file.  
  
  The key word DOMAIN combines the vs-file and agg-file with the pxfile.
  Aggregation path = Path to the folder where the aggregation files (.vs and .agg) are stored
  
   'Groupings from the local directory overrides groupings from the default directory
                        '==> Only add groupings from the default directory if no groupings are previously
                        '    added to the variable
  
  If you are using aggregation files they are stored in the aggregation
  path folder of your website. It is possible to create subdirectories of this
folder and arrange aggregation files in these subdirectories.

Pages 7 -10 in https://www.scb.se/globalassets/vara-tjanster/px-programmen/tutorial-pxwin_1.2_v1.pdf
  
  
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


LinkFileHandler
This handler handles .link files that is found by the spider. It parses the content of the link file to
extract the link and the link “description” and creates a LinkItem object setting the link url, text
and language. If the language is missing from the filename the language will be set to default
language of the PXWeb installation.
   
  
  
