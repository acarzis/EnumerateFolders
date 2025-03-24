
Note: Below steps require admin access

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



Application Flow:

- Call Init()
- Get next queue item - i.e. folder location
- Get all the files within the folder and add the file to the repo, if not present
  Compute size of folder if added to repo
- Add all the sub-folders to the scan queue with priority 2 if not present
  if there are no sub-folders: 
     store the folder size as the sum of all the file sizes
     compute folder size of parent folder (and files) from the repo (NOT file system - for efficiency) and update repo entry
- Get the category name for the item and update the repo entry along with the lastchecked date
- From repo: Get the folder list where a category has been specified
  For each location:
     Get the folder details/last checked date/time from the repo if it exists.
     Add the folder to the scan queue if required, with priority 4.
- Call AddDrivesToScanQueue() if the scan queue is empty.
