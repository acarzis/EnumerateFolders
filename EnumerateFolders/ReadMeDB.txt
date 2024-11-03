Goals:

1. We want to associate a folder or file to a category
2. We want to know the size of any folder at any time

This is what we need in terms of database objects:

1. Something to store the size of each folder, it's last modified time, and the last time the folder was checked for its size.
   We can store the full path as a hash to save space.

2. A db table to store our category types and their associated file extensions

3. A db table to store a folder (hash) with a (optional) category

4. A db table to store a file (fullpath) hash with a categery


Summary:
We have Categories
We have Folders
Folders belong to (1) Category					- By folder, we mean the full Folder Path
Files belong to (1) Category , (1) Folder	 


Once we know what we want (above), we start working on what operations we need in our DB 'API'.
This is what defines IFolderInfoRepository and its implementation class, FolderInfoRepository
