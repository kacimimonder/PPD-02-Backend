# 🎓 Mini Coursera - Backend

This is the backend for **Mini Coursera**, a lightweight online learning platform inspired by Coursera. It provides RESTful APIs to manage users, courses, enrollments, and authentication.

Built using **.NET 8** and **ASP.NET Core Web APIs**.

---

## 🛠 Tech Stack

- **.NET 8**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **SQL Server**
- **JWT Authentication**
- **Role-based Authorization**
- **RESTful API Architecture**

---

## 🔐 Features

- ✅ User registration and login
- ✅ JWT-based authentication (Access + Refresh tokens)
- ✅ Role-based authorization (Student / Instructor)
- ✅ Course creation and management
- ✅ Student enrollment system
- ✅ Protected API endpoints
- ✅ Token refresh flow using HttpOnly cookies
- ✅ AI integration gateway endpoints (`/api/ai/*`) for chat, summary, and quiz (Stage 1)

## 🤖 AI Integration Docs

- See `docs/AI_INTEGRATION_GUIDE.md` for architecture, setup, contracts, tests, and staged roadmap.

---

---

## 🚀 Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/your-username/mini-coursera-backend.git
cd mini-coursera-backend


2. Configure the database
Update your appsettings.json:

json
Copy
Edit
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MiniCourseraDb;User Id=your_user;Password=your_password;"
  },
  ...
}

3. Run EF Core migrations
bash
Copy
Edit
dotnet ef database update
4. Run the application
bash
Copy
Edit
dotnet run
The API will be available at:
https://localhost:5001 or http://localhost:5000

👤 Author
Adnane Mezrag
Software Developer

📄 License
This project is licensed under the MIT License.


---

```
