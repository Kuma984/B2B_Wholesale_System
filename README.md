# B2B Wholesale Order Management System

**Course:** Software Engineering (FIT - TDTU)  
**Instructor:** Ky-Trung Pham (`tg_phamthaikytrung@tdtu.edu.vn`)  
**Assignment:** Group Assignment 2 (30% of QT2)  
**GitHub Repository:** [https://github.com/Kuma984/B2B_Wholesale_System.git](https://github.com/Kuma984/B2B_Wholesale_System.git)

---

## 🏢 System Overview

The **B2B Wholesale Order Management System** is a dual-platform software solution for a regional distributor:
1. **Back-Office Desktop Application (`B2B.WinForms`):** For warehouse staff and inventory administrators to manage products, adjust stock, manage registered distributor agents, process incoming orders, generate shipping manifest reports, and monitor sales analytics.
2. **Agent Web Portal (`B2B.Web`):** An ASP.NET Core MVC application where wholesale agents log in, browse the product catalog, place complex master-detail orders with atomic transaction safety, track order statuses, and print invoices.

---

## 👥 Division of Labor (3-Member Group)

| Member | Student ID | GitHub Account | Role | Assigned Modules & Tasks | Workload Share |
| :--- | :--- | :--- | :--- | :--- | :---: |
| **Trịnh Đức Long** | 524K0021 | [@Kuma984](https://github.com/Kuma984) | Core Architect, QA & DevOps | • Database Schema & 20+ Seed Rows per table (`Task 2a`)<br>• Solution Structure & Domain Entities<br>• Centralized Configuration (`Task 6a`)<br>• xUnit Business Logic Test Project (`Task 5.1`)<br>• Git Strategy, PR Reviews & Documentation (`Task 6b & 6c`) | **35%** |
| **Trần Hoàng Anh** | 524K0015 | [@SimonAnh](https://github.com/SimonAnh) | WinForms Desktop Specialist | • Admin Secure Login Form (`Task 1.1`)<br>• Items CRUD & Stock Restocking (`Task 1.2`)<br>• Agent Management & Order Status Processing (`Task 1.2`)<br>• Shipping Manifest Report (PDF/RDLC) (`Task 1.3`)<br>• Data Analytics & Filter Dashboard (`Task 1.4`)<br>• DB Integrity verification (`Task 2b`) | **30%** |
| **Võ Thị Mỹ Duyên** | 523V0016 | [@dien131](https://github.com/dien131) | Web Portal Specialist | • Agent Web Authentication (`Task 3.1`)<br>• Catalog View & Product Browsing (`Task 3.2`)<br>• Master-Detail Order Placement with DB Transaction (`Task 3.2`)<br>• "My Orders" History & Printable Invoice (`Task 3.3`)<br>• Responsive Bootstrap UI & DataTables (`Task 3.4`) | **35%** |

---

## 📁 Solution Architecture

```text
B2B_Wholesale_System/
├── Database/
│   └── Database_Setup.sql           # Complete SQL script (5 tables, constraints, 20+ seeds each)
├── src/
│   ├── B2B.Core/                    # Shared Domain Class Library (.NET 8)
│   │   ├── Data/
│   │   │   └── B2BDbContext.cs      # EF Core DbContext with fluent configurations
│   │   ├── DTOs/                    # OrderItemDto, OrderResult
│   │   ├── Entities/                # Item, Agent, Order, OrderDetail, User
│   │   └── Services/                # OrderService (Transactions), AuthService (Security)
│   ├── B2B.WinForms/                # Warehouse Administrator Client
│   │   └── App.config               # Centralized connection string (No hardcoding!)
│   └── B2B.Web/                     # Agent Ordering Portal (ASP.NET Core MVC)
│       ├── Program.cs               # DI setup, DbContext registration, Session
│       └── appsettings.json         # Centralized connection string (No hardcoding!)
└── tests/
    └── B2B.Tests/                   # xUnit Unit Test Suite (13 passing tests)
        ├── OrderServiceTests.cs     # Stock deduction & transaction tests
        └── AuthServiceTests.cs      # Authentication, role & lockout tests
```

---

## 🗄️ Database Setup Instructions

1. Open **SQL Server Management Studio (SSMS)** or **Azure Data Studio**.
2. Connect to your SQL Server instance (e.g. `(localdb)\mssqllocaldb` or `.\SQLEXPRESS`).
3. Open and execute [`Database/Database_Setup.sql`](Database/Database_Setup.sql).
4. The script creates:
   * Database `B2BWholesaleDB`
   * 5 Tables: `Users`, `Agent`, `Item`, `[Order]`, `OrderDetail`
   * Foreign keys with cascading update/delete rules
   * Integrity constraints: `CHECK (StockQuantity >= 0)`, `CHECK (Quantity > 0)`, `CHECK (Role IN ('Admin', 'Agent'))`
   * 20 realistic seed rows in each table.

### Default Login Accounts:
* **Admin Login (WinForms):**
  * Username: `admin_main` | Password: `Admin@123`
* **Agent Login (Web Portal):**
  * Username: `agent_hanoi01` | Password: `Agent@123`
  * Username: `agent_hcm01` | Password: `Agent@123`

---

## ⚙️ Secure Configuration (Zero Hardcoding)

* **WinForms:** Configured in `src/B2B.WinForms/App.config`:
  ```xml
  <connectionStrings>
    <add name="B2BDatabase" connectionString="Server=(localdb)\mssqllocaldb;Database=B2BWholesaleDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;" providerName="System.Data.SqlClient" />
  </connectionStrings>
  ```
* **Web Portal:** Configured in `src/B2B.Web/appsettings.json`:
  ```json
  "ConnectionStrings": {
    "B2BDatabase": "Server=(localdb)\\mssqllocaldb;Database=B2BWholesaleDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
  }
  ```

---

## 🌿 Git Branching Strategy & Workflow

Each member works on their dedicated feature branch:
1. **Desktop Specialist:** `feature/winforms-admin`
2. **Web Specialist:** `feature/web-agent-portal`
3. **Core / QA Lead:** `feature/testing-core`

### Workflow:
```bash
# 1. Clone repository
git clone https://github.com/Kuma984/B2B_Wholesale_System.git
cd B2B_Wholesale_System

# 2. Switch to your feature branch
git checkout -b feature/your-feature-name

# 3. Commit regular progress
git add .
git commit -m "feat(winforms): implement items CRUD screen"

# 4. Push to GitHub
git push -u origin feature/your-feature-name

# 5. Open a Pull Request on GitHub to merge into main.
```

---

## 🧪 Running the Unit Tests

Run the full xUnit test suite from the terminal:
```bash
dotnet test
```
All 13 unit tests verify:
* Atomic stock deduction upon order placement
* Order block & transaction rollback when stock is insufficient
* Zero stock handling when order quantity equals stock
* Validation when quantity $\le 0$
* Consolidating duplicate item quantities in a single order
* Order total calculations
* Admin & Agent role verification
* Account lockout logic after exceeding failed attempts
