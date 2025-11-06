# Tunis Motors - Car E-commerce Platform

A comprehensive ASP.NET Core MVC application for buying and selling cars online in Tunisia. This platform provides a complete e-commerce solution with user management, product listings, shopping cart, wishlist functionality, and administrative controls, specifically tailored for the Tunisian automotive market.

## 🚗 Features

### User Features
- **User Registration & Authentication** - Secure login/signup with role-based access
- **Product Browsing** - Advanced filtering by category, brand, price range, and search
- **Shopping Cart** - Add/remove items, real-time cart count updates
- **Wishlist** - Save favorite cars for later
- **Order Management** - Place orders, view order history with detailed receipts
- **User Profile** - Manage personal information and view listings/orders

### Seller Features
- **Car Listings Management** - Create, edit, and delete car listings
- **Product Details** - Comprehensive car specifications and images
- **Approval System** - Listings require admin approval before publishing

### Admin Features
- **Dashboard Analytics** - Overview of platform statistics
- **User Management** - Manage user accounts and roles
- **Category Management** - Create and manage car categories with images
- **Approval System** - Review and approve/reject car listings
- **Role Management** - Assign and manage user roles (Admin, Manager, Seller)

### Technical Features
- **Responsive Design** - Mobile-friendly interface using Bootstrap 5
- **Real-time Updates** - AJAX-powered cart and wishlist updates
- **Advanced Filtering** - Price range slider, category/brand filters, sorting
- **File Upload** - Image upload for products and categories
- **Email Notifications** - Order confirmations and system notifications
- **Security** - CSRF protection, input validation, role-based authorization

## 🛠️ Technologies Used

### Backend
- **ASP.NET Core 8.0** - Web framework
- **Entity Framework Core** - ORM for database operations
- **SQL Server** - Database management
- **Identity Framework** - User authentication and authorization
- **C# 12** - Programming language

### Frontend
- **Razor Views** - Server-side rendering
- **Bootstrap 5** - CSS framework
- **Font Awesome 6** - Icons
- **jQuery** - JavaScript library
- **NoUISlider** - Price range slider
- **Slick Carousel** - Image carousels

### Development Tools
- **Visual Studio 2022** - IDE
- **Git** - Version control
- **Entity Framework Migrations** - Database schema management

## 📋 Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or full SQL Server)
- Visual Studio 2022 or VS Code
- Git

## 🚀 Installation

### 1. Clone the Repository
```bash
git clone <repository-url>
cd TP2
```

### 2. Database Setup
```bash
# Update connection string in appsettings.json
# Default connection uses LocalDB
```

### 3. Install Dependencies
```bash
dotnet restore
```

### 4. Database Migration
```bash
dotnet ef database update
```

### 5. Run the Application
```bash
dotnet run
```

### 6. Access the Application
- Open browser and navigate to `https://localhost:5001`
- Default accounts are created automatically on first run

## 📖 Usage

### User Registration
1. Navigate to the registration page
2. Fill in required information
3. Choose account type (Buyer/Seller)
4. Verify email if email confirmation is enabled

### Browsing Cars
1. Use the search bar for keyword search
2. Filter by category, brand, or price range
3. Sort results by newest, price, or other criteria
4. Switch between grid and list view

### Adding to Cart
1. Browse available cars
2. Click "Add to Cart" on desired items
3. View cart by clicking the cart icon
4. Adjust quantities or remove items

### Placing Orders
1. Review items in cart
2. Proceed to checkout
3. Enter delivery address and payment method
4. Confirm order placement

### Managing Listings (Sellers)
1. Login with seller account
2. Navigate to "My Cars" section
3. Create new listings with detailed specifications
4. Upload product images
5. Edit or delete existing listings

## 👥 User Roles

### Admin
- Full system access and control
- User account management (create, edit, delete users)
- Role assignment and management
- Category creation and management with images
- Approve/reject all car listings
- Comprehensive analytics dashboard
- System-wide settings and configuration

### Manager
- Approve/reject pending car listings
- View analytics dashboard with platform statistics
- Create and manage product categories
- Oversee seller activities and listings
- Access to approval workflow

### Seller
- Create and manage personal car listings
- Upload and manage product images
- View own sales performance and listings
- Access seller-specific dashboard
- Edit/delete own listings
- View customer inquiries for own products

### Buyer (Default)
- Browse and search car listings
- Advanced filtering (price, category, brand)
- Add items to cart and wishlist
- Complete purchase process with checkout
- View detailed order history
- Manage personal profile information

## 🗄️ Database Schema

### Main Tables
- **Users** - User accounts and profiles
- **CarListings** - Product catalog
- **Categories** - Product categories
- **Orders** - Customer orders
- **OrderItems** - Order line items
- **Wishlists** - User wishlists
- **Messages** - User communications

### Key Relationships
- Users → Orders (One-to-Many)
- Orders → OrderItems (One-to-Many)
- CarListings → Categories (Many-to-One)
- Users → Wishlists (One-to-Many)

## 🔧 Configuration

### Connection Strings
Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TP2;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### Email Settings
Configure SMTP settings in `appsettings.json` for email notifications.

### File Upload Settings
Configure upload paths and size limits in `appsettings.json`.

## 🧪 Testing

### Running Tests
```bash
dotnet test
```

### Manual Testing Checklist
- [ ] User registration and login
- [ ] Product browsing and filtering
- [ ] Cart functionality
- [ ] Checkout process
- [ ] Order placement and confirmation
- [ ] Wishlist management
- [ ] Admin dashboard access
- [ ] Category management
- [ ] File upload functionality

## 🚀 Deployment

### Production Deployment
1. Update connection strings for production database
2. Configure production email settings
3. Set up file storage (local or cloud)
4. Configure HTTPS certificates
5. Set environment variables for sensitive data

### Docker Deployment
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TP2.csproj", "."]
RUN dotnet restore "TP2.csproj"
COPY . .
RUN dotnet build "TP2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TP2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TP2.dll"]
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Development Guidelines
- Follow C# coding standards
- Use meaningful commit messages
- Write unit tests for new features
- Update documentation as needed
- Ensure responsive design works on all devices

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the documentation for common solutions

## 🔄 Version History

### v1.0.0
- Initial release
- Basic e-commerce functionality
- User management system
- Admin dashboard
- Responsive design implementation

---

**Built with ❤️ using ASP.NET Core - Tunis Motors 🇹🇳**