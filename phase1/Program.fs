open System
open System.Data.SQLite
open System.IO
open System.Windows.Forms

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

// Function to add a student to the database
let addStudent (student: Student) (dbPath: string) =
    use connection = new SQLiteConnection($"Data Source={dbPath};Version=3;")
    connection.Open()
    let command = connection.CreateCommand()
    command.CommandText <- 
        "INSERT INTO Students (Name, Grades) VALUES (@name, @grades);"
    command.Parameters.AddWithValue("@name", student.Name)
    command.Parameters.AddWithValue("@grades", String.Join(",", student.Grades))
    command.ExecuteNonQuery() |> ignore

// Function to retrieve all students from the database
let getStudents (dbPath: string) : Student list =
    use connection = new SQLiteConnection($"Data Source={dbPath};Version=3;")
    connection.Open()
    let command = connection.CreateCommand()
    command.CommandText <- "SELECT * FROM Students;"
    use reader = command.ExecuteReader()
    let mutable students = []
    while reader.Read() do
        let id = reader.GetInt32(0)
        let name = reader.GetString(1)
        let grades = reader.GetString(2).Split(',') |> Array.map float |> Array.toList
        students <- { Id = id; Name = name; Grades = grades } :: students
    students

// Function to calculate average grade for a student
let calculateAverage (grades: float list) : float =
    if List.isEmpty grades then 0.0 else List.average grades

// Function to get class statistics
let classStatistics (studentList: Student list) =
    let totalStudents = List.length studentList
    let passedStudents = studentList |> List.filter (fun s -> calculateAverage s.Grades >= 50.0) |> List.length
    let highestGrade = studentList |> List.collect (fun s -> s.Grades) |> List.max
    let lowestGrade = studentList |> List.collect (fun s -> s.Grades) |> List.min
    (totalStudents, passedStudents, highestGrade, lowestGrade)

// Function to create the main form
let createForm(dbPath: string) =
    let form = new Form(Text = "Student Management System", Width = 800, Height = 600)

    let roleLabel = new Label(Text = "Select Role:", Top = 20, Left = 20)
    let adminRadioButton = new RadioButton(Text = "Admin", Top = 20, Left = 120, Checked = true)
    let viewerRadioButton = new RadioButton(Text = "Viewer", Top = 20, Left = 250)

    let nameInput = new TextBox(Top = 60, Left = 120, Width = 200)
    let gradesInput = new TextBox(Top = 100, Left = 120, Width = 200)
    
    let addButton = new Button(Text = "Add Student", Top = 140, Left = 120,Width = 150)
    let viewButton = new Button(Text = "View Students", Top = 180, Left = 120,Width = 150)
    let statsButton = new Button(Text = "View Statistics", Top = 220, Left = 120,Width = 150)

    // Enable/disable buttons based on role
    let updateButtonAccess () =
        let isAdmin = adminRadioButton.Checked
        addButton.Enabled <- isAdmin
        nameInput.Enabled <- isAdmin
        gradesInput.Enabled <- isAdmin

    adminRadioButton.CheckedChanged.Add(fun _ -> updateButtonAccess())
    viewerRadioButton.CheckedChanged.Add(fun _ -> updateButtonAccess())
    updateButtonAccess()

    // Event handler for adding a student
    addButton.Click.Add(fun _ ->
        let grades = gradesInput.Text.Split(',') |> Array.map float |> Array.toList
        let newStudent = {
            Id = 0 // ID will be auto-incremented
            Name = nameInput.Text
            Grades = grades
        }
        addStudent newStudent dbPath
        MessageBox.Show("Student Added!") |> ignore
    )

    // Event handler for viewing all students
    viewButton.Click.Add(fun _ ->
        let students = getStudents dbPath
        let studentInfo = 
            students |> List.map (fun s -> sprintf "Id: %d, Name: %s, Grades: %A" s.Id s.Name s.Grades)
            |> String.concat "\n"
        MessageBox.Show(studentInfo, "Student Records") |> ignore
    )

    // Event handler for viewing statistics
    statsButton.Click.Add(fun _ ->
        let students = getStudents dbPath
        if List.isEmpty students then
            MessageBox.Show("No students to display statistics.") |> ignore
        else
            let totalStudents, passedStudents, highestGrade, lowestGrade = classStatistics students
            let passRate = (float passedStudents / float totalStudents) * 100.0
            let statsMessage = 
                sprintf "Total Students: %d\nPassed Students: %d\nPass Rate: %.2f%%\nHighest Grade: %.2f\nLowest Grade: %.2f"
                        totalStudents passedStudents passRate highestGrade lowestGrade
            MessageBox.Show(statsMessage, "Class Statistics") |> ignore
    )

    // Add controls to the form
    form.Controls.AddRange([| roleLabel; adminRadioButton; viewerRadioButton; nameInput; gradesInput; addButton; viewButton; statsButton |])
    form

// Entry point for the application
[<EntryPoint>]
let main argv =
    let dbPath = "students.db"
    createDatabase dbPath

    Application.EnableVisualStyles()
    Application.Run(createForm dbPath)
    0