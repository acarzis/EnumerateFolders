
Note: Below, requires admin access

installing the service :
   installutil.exe EnumerateService.exe  

removing the service
   installutil.exe -uninstall EnumerateService.exe  


Alternatively:

SC CREATE "EnumerateService" binpath= "fullpath"
SC DELETE "EnumerateService" 

SC CREATE "EnumerateService" binpath= "D:\DEVELOPMENT\PROJECTs\Visual Studio 2022\EnumerateFolders\EnumerateService\bin\Debug\EnumerateService.exe"


DB Permissions:
NT AUTHORITY\SYSTEM must be a user to the database FolderEF 


What do we want this service to do ?

1. Get the category list
2. Get the drive list
3. Scan each folder for files that match something in the Category List.
   Add/Update a file in the repo with category, 	
   Record/update the folder size, last checked date, last modified date  
4. Do not scan any folder which has been assigned a Category.
5. Do not scan any folder who's last checked date > last modified date.
6. Root folder:  Always record/update the folder size, last checked date, last modified date   

Future additions:
1. Perhaps record the folder names and their size of all sub-root folders.


Indexing Algorithm:

- Get next queue item - i.e. folder location
- Get all the files within the folder and add the file to the repo, if not present
- Add all the sub-folders to the scan queue with priority 2 if not present
  if there are no sub-folders: 
     store the folder size as the sum of all the file sizes
     compute folder size of parent folder (and files) from the repo (NOT file system - for efficiency) and update repo entry
- Get the category name for the item and update the repo entry along with the lastchecked date
- From repo: Get the folder list where a category has been specified
  For each location:
     Get the folder details/last checked date/time from the repo if it exists.
     Add the folder to the scan queue if required, with priority 4.
- Call Init() if the scan queue is empty.


Consider:
Step 3: Don't add the sub-folder to scan queue if folder is in repo. 
ToScanQueue table: add a creation Timestamp field


