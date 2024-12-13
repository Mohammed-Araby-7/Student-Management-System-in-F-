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


// GUI 

open System.Windows.Forms
open System.Drawing

let createForm(dbPath: string) =
    let form = new Form(Text = "Student Management System", Width = 1000, Height = 700)
    form.BackColor <- Color.LightBlue

    // Role selection
    let roleLabel = new Label(Text = "Select Role:", Top = 20, Left = 20, Width = 200, Font = new Font("Arial", 12.0F, FontStyle.Bold))
    let adminRadioButton = new RadioButton(Text = "Admin", Top = 20, Left = 240, Font = new Font("Arial", 12.0F))
    let viewerRadioButton = new RadioButton(Text = "Viewer", Top = 20, Left = 360, Font = new Font("Arial", 12.0F))

    // Student name input
    let nameLabel = new Label(Text = "Student Name:", Top = 60, Left = 20, Width = 120, Font = new Font("Arial", 12.0F))
    let nameInput = new TextBox(Top = 60, Left = 180, Width = 300)

    // Define subjects and create TextBoxes for grades
    let subjects = ["Math"; "Arabic"; "Chemistry"; "Physics"; "English"; "History"]
    let gradeInputs = 
        subjects |> List.mapi (fun i subject ->
            let label = new Label(Text = subject, Top = 100 + i * 50, Left = 20, Width = 100, Font = new Font("Arial", 12.0F))
            let textBox = new TextBox(Top = 100 + i * 50, Left = 140, Width = 300)
            form.Controls.Add(label) 
            form.Controls.Add(textBox) 
            textBox 
        )

    // ID input for deletion
    let deleteIdLabel = new Label(Text = "Delete Student ID:", Top = 400, Left = 20, Width = 120, Font = new Font("Arial", 12.0F))
    let deleteIdInput = new TextBox(Top = 400, Left = 160, Width = 100)

    // Action buttons
    let buttonStyle = new Font("Arial", 12.0F, FontStyle.Bold)
    let addButton = new Button(Text = "Add Student", Top = 440, Left = 140, Width = 150, Font = buttonStyle, BackColor = Color.LightGreen)
    let viewButton = new Button(Text = "View Students", Top = 440, Left = 320, Width = 150, Font = buttonStyle, BackColor = Color.LightYellow)
    let statsButton = new Button(Text = "View Statistics", Top = 440, Left = 500, Width = 150, Font = buttonStyle, BackColor = Color.LightCoral)
    let deleteButton = new Button(Text = "Delete Student", Top = 440, Left = 680, Width = 150, Font = buttonStyle, BackColor = Color.LightCoral)

    // Enable/disable buttons based on role
    let updateButtonAccess () =
        let isAdmin = adminRadioButton.Checked
        addButton.Enabled <- isAdmin
        deleteIdInput.Enabled <- isAdmin
        deleteButton.Enabled <- isAdmin
        nameInput.Enabled <- isAdmin
        gradeInputs |> List.iter (fun tb -> tb.Enabled <- isAdmin)

    adminRadioButton.CheckedChanged.Add(fun _ -> updateButtonAccess())
    viewerRadioButton.CheckedChanged.Add(fun _ -> updateButtonAccess())
    updateButtonAccess()

    // Event handler for adding a student
    addButton.Click.Add(fun _ ->
        let grades =
            try 
                gradeInputs |> List.map (fun tb -> 
                    let text = tb.Text.Trim()
                    if String.IsNullOrWhiteSpace(text) then
                        raise (System.FormatException())
                    float text)
            with
            | :? System.FormatException -> 
                MessageBox.Show("Please enter valid numeric grades for all subjects.") |> ignore
                []

        if List.isEmpty grades then
            MessageBox.Show("Grades cannot be empty.") |> ignore
        else
            let newStudent = {
                Id = 0 
                Name = nameInput.Text 
                Grades = grades
            }
            addStudent newStudent dbPath // Call to the function you defined earlier
            MessageBox.Show("Student Added!") |> ignore
            nameInput.Clear()
            gradeInputs |> List.iter (fun tb -> tb.Clear())
    )

    // Event handler for viewing all students
    viewButton.Click.Add(fun _ ->
        let students = getStudents dbPath // Call to the function you defined earlier
        let studentInfo = 
            students |> List.map (fun s -> 
                let passStatus = if List.exists (fun g -> g < 50.0) s.Grades then "Failed" else "Passed"
                sprintf "Id: %d\nName: %s\nGrades: [%s]\nStatus: %s\n" 
                        s.Id 
                        s.Name 
                        (String.Join(", ", List.zip subjects s.Grades |> List.map (fun (subject, grade) -> sprintf "%s: %.2f" subject grade))) 
                        passStatus)
            |> String.concat "\n\n"

        let studentForm = new Form(Text = "Student Records", Width = 1200, Height = 800)
        let textBox = new TextBox(Multiline = true, ScrollBars = ScrollBars.Vertical, ReadOnly = true, Dock = DockStyle.Fill, Text = studentInfo)
        studentForm.Controls.Add(textBox)
        studentForm.ShowDialog() |> ignore
    )

    // Event handler for viewing statistics
    statsButton.Click.Add(fun _ ->
        let students = getStudents dbPath // Call to the function you defined earlier
        if List.isEmpty students then
            MessageBox.Show("No students to display statistics.") |> ignore
        else
            let totalStudents, passedStudents, failedStudents, highestGrade, lowestGrade = classStatistics students
            let passRate = (float passedStudents / float totalStudents) * 100.0
            let statsMessage = 
                sprintf "Total Students: %d\nPassed Students: %d\nFailed Students: %d\nPass Rate: %.2f%%\nHighest Grade: %.2f\nLowest Grade: %.2f"
                        totalStudents passedStudents failedStudents passRate highestGrade lowestGrade
            
            let statsForm = new Form(Text = "Class Statistics", Width = 1200, Height = 800)
            let textBox = new TextBox(Multiline = true, ScrollBars = ScrollBars.Vertical, ReadOnly = true, Dock = DockStyle.Fill, Text = statsMessage)
            statsForm.Controls.Add(textBox)
            statsForm.ShowDialog() |> ignore
    )

    // Event handler for deleting a student
    deleteButton.Click.Add(fun _ ->
        try
            let id = int deleteIdInput.Text
            deleteStudentById id dbPath // Call to the function you defined earlier
            MessageBox.Show("Student Deleted!") |> ignore
            deleteIdInput.Clear()
        with
        | :? System.FormatException -> 
            MessageBox.Show("Please enter a valid numeric ID.") |> ignore
        | ex -> 
            MessageBox.Show($"Error: {ex.Message}") |> ignore
    )

    // Add controls to the form
    form.Controls.AddRange([| roleLabel; adminRadioButton; viewerRadioButton; nameLabel; nameInput; deleteIdLabel; deleteIdInput; addButton; viewButton; statsButton; deleteButton |])
    form

// Entry point for the application
[<EntryPoint>]
let main argv =
    let dbPath = "students.db"
    createDatabase dbPath // Call to the function you defined earlier

    Application.EnableVisualStyles()
    Application.Run(createForm dbPath)


// Function to calculate average grade for a student
let calculateAverage (grades: float list) : float =
    if List.isEmpty grades then 0.0 else List.average grades

// Function to get class statistics
let classStatistics (studentList: Student list) =
    if List.isEmpty studentList then
        (0, 0, 0, 0.0, 0.0) // Avoid errors if no students
    else
        let totalStudents = List.length studentList
        let passedStudents = 
            studentList 
            |> List.filter (fun s -> List.forall (fun g -> g >= 50.0) s.Grades) // Passed if all grades >= 50
            |> List.length
        let failedStudents = totalStudents - passedStudents // Calculate failed students
        let highestGrade = studentList |> List.collect (fun s -> s.Grades) |> List.max
        let lowestGrade = studentList |> List.collect (fun s -> s.Grades) |> List.min
        (totalStudents, passedStudents, failedStudents, highestGrade, lowestGrade)

