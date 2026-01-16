# Real Estate Platform - Complete Implementation

## Overview
This is a complete real estate management platform built with ASP.NET Core 9.0, MySQL (Pomelo), and Identity Framework.

## Features Implemented

### 1. Authentication & Authorization
- **AccountController** - Complete login/logout/register functionality
- **Role-based access control**: Admin, Client, Agent
- Identity configuration with relaxed password requirements for development

### 2. Property Management (PropertiesController)
- ✅ **List all properties** - Public view with images
- ✅ **Property details** - Detailed view with images and owner info
- ✅ **Create property** - Authenticated users can create properties with image upload
- ✅ **Edit property** - Property owners and admins can edit
- ✅ **Delete property** - Property owners and admins can delete
- ✅ **Image upload** - Multiple images per property
- ✅ **Delete images** - Remove individual property images
- ✅ **Authorization checks** - Users can only edit/delete their own properties (or admin)

### 3. Request Management (RequestsController)
- ✅ **Create request** - Clients can request property viewings/information
- ✅ **View requests** - Users see their own requests, admins see all
- ✅ **My property requests** - Property owners see requests for their properties
- ✅ **Update request status** - Property owners can approve/reject requests
- ✅ **Request details** - View full request information
- ✅ **Delete request** - Request owners can delete

### 4. Messaging System (MessagesController)
- ✅ **Send message** - Send messages to other users by email
- ✅ **Inbox** - View all received messages
- ✅ **Sent messages** - View sent messages
- ✅ **Message details** - View message content
- ✅ **Reply** - Reply to received messages
- ✅ **Delete message** - Remove messages

### 5. Admin Dashboard (AdminController)
- ✅ **Dashboard** - Statistics overview (users, properties, requests, messages)
- ✅ **User management** - View all users, roles, and activities
- ✅ **User details** - View user's properties and requests
- ✅ **Toggle user roles** - Add/remove roles from users
- ✅ **Delete users** - Remove users from system
- ✅ **View all properties** - Admin property listing
- ✅ **View all requests** - Admin request monitoring
- ✅ **View all messages** - Admin message oversight
- ✅ **Statistics page** - Detailed stats by status and role
- ✅ **Seed roles** - Initialize system roles

### 6. Home & Public Pages (HomeController)
- ✅ **Homepage** - Featured/latest available properties
- ✅ **Privacy page**
- ✅ **Error handling**

## Database Entities

### ApplicationUser (extends IdentityUser)
- FirstName, LastName
- OwnedProperties (navigation)
- SentMessages, ReceivedMessages (navigations)
- Requests (navigation)

### Property
- Id, Title, Description, Address
- Price, Bedrooms, Bathrooms, Area
- Status (Available, Sold, Rented, etc.)
- OwnerId (foreign key to ApplicationUser)
- Images (navigation to PropertyImage collection)

### PropertyImage
- Id, Url
- PropertyId (foreign key)

### Request
- Id, PropertyId, UserId
- Notes, Status (Pending, Approved, Rejected)
- CreatedAt

### Message
- Id, FromUserId, ToUserId
- Subject, Body, SentAt

## View Models Created

1. **LoginViewModel** - Email, Password, RememberMe
2. **RegisterViewModel** - FirstName, LastName, Email, Password, ConfirmPassword, Role
3. **PropertyCreateViewModel** - Property fields + image upload support
4. **RequestCreateViewModel** - PropertyId, Notes
5. **MessageCreateViewModel** - ToUserEmail, Subject, Body

## Database Configuration

### Connection String (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=realestate_db;User=root;Password=ChangeMe123!;"
  }
}
```

**⚠️ IMPORTANT**: Update the MySQL password in `appsettings.json` to match your MySQL server.

### Default Seeded Users

The system automatically seeds three test users on first run:

1. **Admin User**
   - Email: `admin@realestate.com`
   - Password: `Admin@123`
   - Role: Admin

2. **Client User**
   - Email: `client@realestate.com`
   - Password: `Client@123`
   - Role: Client

3. **Agent User**
   - Email: `agent@realestate.com`
   - Password: `Agent@123`
   - Role: Agent

## How to Run

### Prerequisites
- .NET 9.0 SDK installed
- MySQL Server running on localhost:3306
- Update MySQL password in `appsettings.json`

### Steps

1. **Update database connection**:
   - Open `appsettings.json`
   - Update the password in the connection string to match your MySQL root password

2. **Run the application**:
   ```cmd
   cd C:\Users\yby39\RiderProjects\WebApplication4\WebApplication4
   dotnet run
   ```

3. **The application will**:
   - Automatically create the database `realestate_db`
   - Apply all migrations
   - Seed roles (Admin, Client, Agent)
   - Create default users
   - Start the web server

4. **Access the application**:
   - Open browser to `http://localhost:5119` or `https://localhost:7115`
   - Login with one of the seeded accounts

## Project Structure

```
Controllers/
  ├── AccountController.cs       - Login, Register, Logout
  ├── PropertiesController.cs    - Property CRUD + Images
  ├── RequestsController.cs      - Request management
  ├── MessagesController.cs      - Messaging system
  ├── AdminController.cs         - Admin dashboard & management
  └── HomeController.cs          - Public pages

Models/
  ├── ApplicationUser.cs         - User entity
  ├── Property.cs                - Property entity
  ├── PropertyImage.cs           - Property image entity
  ├── Request.cs                 - Request entity
  └── Message.cs                 - Message entity

ViewModels/
  ├── LoginViewModel.cs
  ├── RegisterViewModel.cs
  ├── PropertyCreateViewModel.cs
  ├── RequestCreateViewModel.cs
  └── MessageCreateViewModel.cs

Data/
  ├── ApplicationDbContext.cs    - EF Core DbContext
  ├── DbInitializer.cs           - Database seeding
  └── DesignTimeDbContextFactory.cs - EF migrations support

Views/
  ├── Account/                   - Login, Register views
  ├── Properties/                - Property views
  ├── Requests/                  - Request views
  ├── Messages/                  - Message views
  ├── Admin/                     - Admin dashboard views
  └── Home/                      - Homepage, Privacy
```

## Key Features

### Authorization
- Public users can view properties
- Authenticated users can create properties and requests
- Property owners can edit/delete their properties
- Property owners can manage requests for their properties
- Admins have full access to everything

### Image Upload
- Properties support multiple image uploads
- Images stored in `wwwroot/uploads/properties/`
- Each image gets a unique GUID filename
- Images can be deleted individually

### Database Migrations
- Initial migration includes all entities
- Identity tables (AspNetUsers, AspNetRoles, etc.)
- Custom entities (Properties, Requests, Messages, PropertyImages)
- Proper foreign key relationships and indexes

## Next Steps

1. **Update MySQL password** in appsettings.json
2. **Run the application**: `dotnet run`
3. **Login** with admin@realestate.com / Admin@123
4. **Create properties**, manage users, handle requests

## Notes

- The project uses MySQL via Pomelo.EntityFrameworkCore.MySql 9.0.0
- Entity Framework Core 9.0.0
- ASP.NET Core Identity for authentication
- Bootstrap 5 for UI (already included in layout)
- The database will be automatically created and seeded on first run

