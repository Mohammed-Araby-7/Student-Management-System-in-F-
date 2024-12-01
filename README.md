# Student Management System

## Overview

The **Student Management System** is an F# application designed to facilitate the management of student records. The application utilizes an SQLite database to store essential student information, including ID, name, and grades. It features a graphical user interface (GUI) that allows users to easily interact with the system.

### Key Features

- **Database Management**: The system creates and manages a SQLite database (`students.db`) to store student records persistently.
- **Student Records**: Users can add, edit, and delete student records, ensuring that the information is up-to-date and accurate.
- **Grade Management**: The application calculates individual student averages and class-wide statistics, including:
  - Pass/fail rates
  - Highest and lowest grades in the class
- **User Roles**:
  - **Admin Role**: Provides full control over database operations, allowing for the addition and modification of student records.
  - **Viewer Role**: Grants read-only access to student records and statistical reports, ensuring that sensitive data is protected.

### How It Works

1. **Database Initialization**: When the application starts, it checks for the existence of the `students.db` file. If it doesn't exist, the application creates the database and initializes the `Students` table.
   
2. **Adding Students**: Admin users can enter the name and grades of students through the GUI. The system stores this information in the database.

3. **Viewing Records**: Users can view all student records, which are displayed in a user-friendly format. This includes the option to see detailed statistics about class performance.

4. **Grade Calculations**: The system automatically calculates averages, and identifies the highest and lowest grades, providing valuable insights into student performance.

This application serves as a practical tool for educational institutions or individual educators looking to manage student data effectively.
