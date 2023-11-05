# Database schema management

## How to contribute to this folder.

> Before you make any changes you need to understand how DbUp works.

Here are a few things to keep in mind: 

#### 1. All scripts are numbered, they will be executed in the order they appear. 

All scripts are executed exactly ONCE in the database. 
Once a script is executed its name is added in a database table and DbUp checks that table to figure out what needs to be executed.

Therefore:
> Under no circumstance will anyone change the **NAME** of the script. That could cause catastrophic problems.

#### 2. No changing scripts, changes are only additive.

Once a script is merged into develop it should not be changed as the changes will not ever be executed (see above point) 
If the result is not what was needed then more scripts will be required.
