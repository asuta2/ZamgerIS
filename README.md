# Zamger Information System


This is a web application built with ASP.NET Core and Entity Framework Core. It's designed to manage students progress on individual courses to help faculty member organize their data better.


## Live Demo

You can view a live demo of the application at [https://zamgeris.azurewebsites.net](https://zamgeris.azurewebsites.net).

## Test Cases:

Below are provided some test users which will show the basic functionalities of this application for the following roles:

- **Student**: test@gmai.com password

- **Professor**: testprof@test.com password

- **Student Services**: admin@etf.unsa.ba password

## Features

- **User Authentication**: The application uses ASP.NET Core Identity for user management. It supports roles such as "Student" and "Teacher".

- **Exam Management**: Teachers can create exams for their courses. Students can register for these exams.

- **Homework Management**: Teachers can assign homework to their courses. Students can view and submit homework.

- **Google Sheets Integration**: The application integrates with Google Sheets to manage exam data.

- **Notifications**: The application sends notifications to students when new exams are created.

### Installing

1. Clone the repository
2. Open the solution in your IDE
3. Update the database connection string in `appsettings.json`
4. Run the application

## Authors

- Nedim Džajić
- Nihad Baberović
- Edwin Graca
- Ajdin Šuta
- Ada Džanko
