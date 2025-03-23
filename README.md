# EnumerateFolders

I created this 'app' because I wanted a means to do the following:

1. Get a visual representation of how I am wasting my hard drive space.
2. Have the size of each of my drive folders always available.
3. Do the above using .Net EF Code First for the sake of learning and ease of development

Note: This is unfinished work. Most of the code is in place but improvements are needed here and there. 
Primarily in the startup code and the GUI.

App history is available in the ChangeList.txt file. 

Update:
I rewrote this app in C++ using a different approach - all operations were done against an in memory database which is periodically written to a physical database.
Performance was not very good. I'm not sure why. Most likely because storing millions of records eats up a lot of memory and searches are not efficient if the code has 
not been optimized.


November 3, 2024  
March 22, 2025 (latest update)
