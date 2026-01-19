# 🏋️‍♂️ Gym Management Web Application

## 📌 Project Introduction

This application is a web system that digitizes membership, appointment, and trainer management for gyms. Developed with ASP.NET Core MVC, this system allows gym managers, members, and trainers to easily track all operations.

Within the scope of the project, users can:
- Register to the system as a member,
- View trainers and services,
- Make appointments with available trainers at suitable times,
- Get AI-powered exercise and diet recommendations.

---

## 🔧 Technologies Used

- **Backend:** ASP.NET Core MVC (C#), Entity Framework Core, LINQ
- **Frontend:** HTML5, CSS3, Bootstrap 5, jQuery
- **Database:** PostgreSQL (Code-First Migrations)
- **Authentication:** ASP.NET Identity (Admin and Member roles)
- **AI Integration:** Google GenAI (Gemini API) – Recommendation System
- **Version Control:** Git & GitHub

---

## 👥 Roles and Authorization

| Role     | Permissions |
|---------|-------------|
| **Admin** | Adding trainers/gyms/services, approving appointments, full system control |
| **Member** | Registering, viewing trainers/services, making appointments, viewing AI recommendations |

---

## 🤖 Artificial Intelligence Module

- The user connects to the Google Gemini API by entering data such as height, weight, and body type.

- On the AI recommendation page, personalized fitness + nutrition plans and transformation images are displayed.

- Model used: gemini-2.5-flash-image
