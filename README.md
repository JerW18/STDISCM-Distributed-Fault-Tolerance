# Distributed Fault Tolerance Online Enrollment System
This project demonstrates a fault-tolerant online enrollment system built using a distributed, service-oriented architecture.

* **Architecture:** Follows MVC, with the View layer separated onto its own node. Backend logic is split into distinct services (Authentication, Courses, Grades), each designed to run on separate nodes.
* **Services:**
    * **Authentication:** Manages user login/logout (JWT) and sessions.
    * **Courses:** Handles course viewing and student enrollment.
    * **Grades:** Manages grade viewing (students) and uploading (faculty).
* **Fault Tolerance:** Designed so that the failure of one service node affects only its specific features, allowing the rest of the application to remain functional.

## How to Run
### Running in one machine
1. Double click on the provided `.sln` file and open the project in Visual Studio.
2. Build the project by clicking **Build → Build Solution**.
3. Configure the project to have multiple startup projects. See [link](https://learn.microsoft.com/en-us/visualstudio/ide/how-to-set-multiple-startup-projects?view=vs-2022) for guide.
4. Set Action to **Start** and Debug Target to `https`.
5. Run the project by clicking **Release → Start**.
6. To disable a node, go to the multiple startup configuration and change its Action to `none`.
