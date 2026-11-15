========================================================================
SOFTWARE ENGINEERING LAB ASSIGNMENT 2: B2B WHOLESALE MANAGEMENT SYSTEM
========================================================================
Instructor: Ky-Trung Pham (tg_phamthaikytrung@tdtu.edu.vn) | FIT - TDTU
Academic Year: Sem 1_2627

1. GITHUB REPOSITORY
------------------------------------------------------------------------
Repository URL: https://github.com/Kuma984/B2B_Wholesale_System.git
Web Application Option Chosen: Option C - ASP.NET Core MVC (Code First)

2. TEAM MEMBERS & PERCENTAGE CONTRIBUTION
------------------------------------------------------------------------
Member 1 (Project Lead / Core Architect & QA Lead):
- Full Name: Trịnh Đức Long
- Student ID: 524K0021
- GitHub: https://github.com/Kuma984
- Contribution: 35%
- Features Built: Database Schema & Seeds (Task 2a), Centralized Configuration (Task 6a), xUnit Unit Testing Suite (Task 5.1), Git Collaboration & PR Review (Task 6b), Documentation & Packaging (Task 6c).

Member 2 (WinForms Desktop Specialist):
- Full Name: Trần Hoàng Anh
- Student ID: 524K0015
- GitHub: https://github.com/SimonAnh
- Contribution: 30%
- Features Built: Secure Admin Login Form (Task 1.1), Items CRUD & Stock Restocking (Task 1.2), Agent Management & Order Status Processing (Task 1.2), Shipping Manifest Report (Task 1.3), Sales Analytics & Low-Stock Dashboard (Task 1.4), Database Integrity Verification (Task 2b).

Member 3 (Web Portal Specialist):
- Full Name: Võ Thị Mỹ Duyên
- Student ID: 523V0016
- GitHub: https://github.com/dien131
- Contribution: 35%
- Features Built: Agent Web Authentication (Task 3.1), Product Catalog & Live Stock Status (Task 3.2), Master-Detail Shopping Cart & Atomic Database Transaction Order Placement (Task 3.2), My Orders History & Dedicated Printable Invoice (Task 3.3), Responsive Bootstrap 5 UI & DataTables (Task 3.4).

TOTAL CONTRIBUTION: 100%

3. DATABASE CONFIGURATION INSTRUCTIONS
------------------------------------------------------------------------
Step 1: Open SQL Server Management Studio (SSMS) or Azure Data Studio.
Step 2: Connect to your Microsoft SQL Server instance (e.g. (localdb)\mssqllocaldb, .\SQLEXPRESS, or localhost).
Step 3: Open and execute the script:
        Database/Database_Setup.sql

Step 4: Verify Connection Strings:
- For WinForms: 'src/B2B.WinForms/App.config'
- For Web App: 'src/B2B.Web/appsettings.json'

4. DEMO CREDENTIALS
------------------------------------------------------------------------
- Warehouse Admin Account (WinForms):
  Username: admin_main
  Password: Admin@123

- Distributor Agent Account (Web Portal):
  Username: agent_hanoi01 (or agent_hcm01)
  Password: Agent@123

5. HOW TO RUN UNIT TESTS
------------------------------------------------------------------------
Open a terminal in the root solution folder and run:
  dotnet test
All 13 unit tests will execute and pass (100% pass rate).
========================================================================
