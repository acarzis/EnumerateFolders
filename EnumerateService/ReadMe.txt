
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
