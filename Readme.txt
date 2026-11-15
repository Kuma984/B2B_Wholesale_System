Software Engineering Lab Assignment 2 - B2B Wholesale Management System
Course: Sem 1_2627 - Faculty of Information Technology, TDTU
Instructor: Mr. Ky-Trung Pham (tg_phamthaikytrung@tdtu.edu.vn)

1. GitHub Repository
Repository URL: https://github.com/Kuma984/B2B_Wholesale_System.git
Web Application Option: Option C - ASP.NET Core MVC (Code First)

2. Team Members and Division of Labor
- Trịnh Đức Long (524K0021) - Project Lead / Core Architect & QA Lead: 35%
  Tasks: Database Schema & Seeds (Task 2a), Centralized Configuration (Task 6a), xUnit Unit Testing Suite (Task 5.1), Git Collaboration & PR Review (Task 6b), Documentation & Packaging (Task 6c).
- Trần Hoàng Anh (524K0015) - WinForms Desktop Specialist: 30%
  Tasks: Secure Admin Login Form (Task 1.1), Items CRUD & Stock Restocking (Task 1.2), Agent Management & Order Processing (Task 1.2), Shipping Manifest Report (Task 1.3), Sales Analytics Dashboard (Task 1.4).
- Võ Thị Mỹ Duyên (523V0016) - Web Portal Specialist: 35%
  Tasks: Agent Web Authentication (Task 3.1), Product Catalog & Live Stock Status (Task 3.2), Master-Detail Shopping Cart & Atomic Order Placement (Task 3.2), My Orders History & Printable Invoice (Task 3.3), Responsive Bootstrap 5 UI (Task 3.4).

Total Contribution: 100%

3. Database Setup Instructions
1. Open SQL Server Management Studio (SSMS) or Azure Data Studio and connect to (localdb)\mssqllocaldb (or .\SQLEXPRESS).
2. Open and run Database/Database_Setup.sql.
   This script creates the B2BWholesaleDB database, sets up all 5 tables (Users, Agent, Item, [Order], OrderDetail), adds foreign keys and CHECK constraints, and seeds 20 realistic records per table.
3. Check the connection strings:
   - WinForms: src/B2B.WinForms/App.config -> key "B2BDatabase"
   - Web: src/B2B.Web/appsettings.json -> ConnectionStrings:"B2BDatabase"
   Both use: Server=(localdb)\mssqllocaldb;Database=B2BWholesaleDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;

4. Demo Accounts
- Back-Office Administrator (WinForms):
  Username: admin_main
  Password: Admin@123
- Wholesale Agent (Web Portal):
  Username: agent_hanoi01
  Password: Agent@123

5. Running Unit Tests
In the solution directory, run:
  dotnet test
All 13 unit tests in tests/B2B.Tests will execute and pass (100% pass rate).
