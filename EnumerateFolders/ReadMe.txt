I created this 'app' because I wanted a means to do the following:

1. Get a visual representation of how I am wasting my hard drive space.
2. Have the size of each of my drive folders always available.
3. Do the above using .Net EF Code First for the sake of learning and ease of development 
   (Unfortunately, all the db relations are 1:1. So this is not that good of a training exercise)

Database requirement: SQL Server
EnumerateApp project: Primarily created for testing my code.


Sure, others have implemented similar applications. I decided to do it on my own because it's great creating your own work, 
and the fact that this is/was something relatively simple to do.


Please note that I created this solution quickly and there are items probably should have done better 
(i.e. there is quite a bit of sloppy work). 

For instance:
My preference is to not to use methods that return success/fail, rather, use exceptions. 



Some items regarding EF Migrations:

A migration is, more or less, a db object create/update script.

In the package manager console, type the following:
Add-Migration InitialMigration					- this will create a Migrations folder
Update-Database 								- this will apply the migrations to our database

We can do the same thing above in code:
Database.Migrate();	- this will execute migrations


Angelo Carzis
November 2, 2024
Montreal, Canada
