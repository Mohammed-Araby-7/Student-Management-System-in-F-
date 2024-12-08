open System
open System.Data.SQLite
open System.IO

// Define an immutable record type for the Student
type Student = {
    Id: int
    Name: string
    Grades: float list
}

// Function to create a database and a Students table
let createDatabase dbPath =
    if not (File.Exists dbPath) then
        SQLiteConnection.CreateFile(dbPath)
        use connection = new SQLiteConnection($"Data Source={dbPath};Version=3;")
        connection.Open()
        let command = connection.CreateCommand()
        command.CommandText <- 
            "CREATE TABLE Students (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Grades TEXT);"
        command.ExecuteNonQuery() |> ignore
