# Real Estate Platform - Complete Implementation

## Overview
This is a complete real estate management platform, "Landmark", built with ASP.NET Core 9.0, MySQL (Pomelo), and Identity Framework. The application serves buyers, sellers, and property owners with a comprehensive suite of tools for property listing, searching, and management.

## Features Implemented

### 1. Authentication & Authorization (AccountController)
- **Complete Authentication**: Login, logout, and registration functionality.
- **User Dashboard**: Personalized dashboard showing owned properties, requests, and messages.
- **Profile Management**: Users can edit their profile (First Name, Last Name) and change their password.
- **Account Deletion**: Users have the ability to permanently delete their account.
- **Role-based Access Control**: Distinct roles for Admin, Client, and Agent.
- **Identity Configuration**: Configured with relaxed password requirements for development ease.

### 2. Property Management (PropertiesController)
- **Property Listing**: Public view of all available properties with images.
- **Property Details**: Detailed view including description, specs, price, and owner information.
- **Create Property**: Authenticated users can list new properties with image uploads.
- **Edit Property**: Property owners and admins can update property details.
- **Delete Property**: Property owners and admins can remove listings.
- **Image Management**: Support for multiple images per property with individual deletion capabilities.
- **Authorization**: Strict checks ensures users can only manage their own properties (unless Admin).

### 3. Request Management (RequestsController)
- **Create Request**: Clients can submit inquiries or viewing requests for specific properties.
- **View Requests**: Users track their sent requests; Admins view all system requests.
- **Property Inquiries**: Owners can view and manage requests received for their properties.
- **Request Status**: Owners can approve or reject requests.
- **Request Details**: Comprehensive view of request information and status.
- **Delete Request**: Request owners can cancel/remove their requests.

### 4. Messaging System (MessagesController)
- **Internal Messaging**: Secure messaging system between users (e.g., Buyer to Agent).
- **Inbox & Sent**: Separate views for received and sent messages.
- **Reply Functionality**: Direct reply capability for ease of communication.
- **Message Management**: Users can delete messages from their inbox.

### 5. Admin Dashboard (AdminController)
- **Comprehensive Statistics**: Visual breakdown of system data:
    - Property Status Distribution (Available vs Sold).
    - Monthly Request Trends (Last 6 months).
    - User Distribution by Role.
    - Top 5 Most Requested Properties.
    - Monthly Message Trends.
- **User Management**: List, search, and view details of all users.
- **Role Management**: Dynamically add or remove roles (Admin, Agent, Client) for any user.
- **User Deletion**: Ability to remove users from the system.
- **System Oversight**: Full access to view all properties, requests, and messages.
- **Seeding**: Utilities to initialize system roles.

### 6. Public Pages & User Experience (HomeController)
- **Hero Section**: Dynamic landing page with call-to-action buttons for registration/login.
- **Featured Properties**: Showcase of latest available listings.
- **Value Propositions**: Targeted information sections for both Buyers/Sellers and Property Owners.
- **Why Choose Us**: Highlighted platform benefits and statistics.
- **Contact Info**: Integrated contact section with location and communication details.
- **Privacy & Error Handling**: Standard support pages.

## Database Entities

### ApplicationUser (extends IdentityUser)
- Attributes: FirstName, LastName
- Navigations: OwnedProperties, SentMessages, ReceivedMessages, Requests

### Property
- Attributes: Id, Title, Description, Address, Price, Bedrooms, Bathrooms, Area, Status
- Relations: OwnerId (User), Images (PropertyImage collection)

### PropertyImage
- Attributes: Id, Url, PropertyId

### Request
- Attributes: Id, PropertyId, UserId, Notes, Status, CreatedAt

### Message
- Attributes: Id, FromUserId, ToUserId, Subject, Body, SentAt

## View Models

1. **LoginViewModel**: Email, Password, RememberMe
2. **RegisterViewModel**: Personal info, Role selection
3. **EditProfileViewModel**: First Name, Last Name updates
4. **ChangePasswordViewModel**: Password security updates
5. **PropertyCreateViewModel**: Property details + Multi-image upload
6. **RequestCreateViewModel**: Inquiry details
7. **MessageCreateViewModel**: Messaging structure
8. **DashboardStatisticsViewModel**: Data structure for Admin charts

## Database Configuration

### Connection String (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=realestate;User=root;Password=password"
  }
}
```
**Important**: Update the MySQL password in `appsettings.json` to match your local MySQL server configuration.

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

1. **Update Database Connection**:
   - Open `appsettings.json`.
   - Update the connection string password to match your MySQL root password.

2. **Run the Application**:
   - Use the helper script:
     ```powershell
     .\run-website.ps1
     ```
   - Or manually via dotnet CLI:
     ```powershell
     dotnet run --project "WebApplication4\WebApplication4.csproj" --launch-profile "http"
     ```

3. **Initialization**:
   - The application will automatically create the database `realestate`.
   - Apply pending migrations.
   - Seed default roles and users.
   - Start the web server.

4. **Access**:
   - Open browser to `http://localhost:5119` or the port indicated in the console.

## Project Structure

```
Controllers/
  ├── AccountController.cs       - Authentication & Profile Management
  ├── PropertiesController.cs    - Property Listings & CRUD
  ├── RequestsController.cs      - Inquiries & Status Workflow
  ├── MessagesController.cs      - User Messaging
  ├── AdminController.cs         - System Administration & Stats
  └── HomeController.cs          - Landing Page & Public Content

Models/
  ├── ApplicationUser.cs
  ├── Property.cs
  ├── PropertyImage.cs
  ├── Request.cs
  └── Message.cs

ViewModels/
  ├── LoginViewModel.cs
  ├── RegisterViewModel.cs
  ├── EditProfileViewModel.cs
  ├── ChangePasswordViewModel.cs
  ├── PropertyCreateViewModel.cs
  ├── RequestCreateViewModel.cs
  ├── MessageCreateViewModel.cs
  └── DashboardStatisticsViewModel.cs

Data/
  ├── ApplicationDbContext.cs
  ├── DbInitializer.cs
  └── DesignTimeDbContextFactory.cs

Views/
  ├── Account/
  ├── Properties/
  ├── Requests/
  ├── Messages/
  ├── Admin/
  └── Home/
```
