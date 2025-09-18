-- User Accounts (Login/Register with Identity)
The application includes 3 predefined users for testing.
You can log in with any of them:

Username: giannis — Password: Giannis1! — Email: giannis@test.com

Username: alex — Password: Alex1! — Email: alex@test.com

Username: jane — Password: Jane1! — Email: jane@test.com

With these accounts you can:

View all books

Leave reviews on books

Like/dislike a review

Register a new user through the Register page



-- Admin Panel
There is a dedicated administrator account with full permissions.

Email/Username: admin@demo.local

Password: Admin#12345!

Only this account has access to the Admin Panel, where the admin can:

Create new books

Update existing books



-- Books
Books can only be created/modified by the Admin.

Regular users do not have access to book creation or editing.



-- Reviews
Only logged-in users can add reviews.

Each review includes:

Rating (1–5)

Comment (optional text)

Username of the reviewer

Guests (not logged in) can view reviews but cannot submit new ones.



-- REST API (Swagger)
Besides the Razor UI, the application also exposes REST APIs:

BookApiController → CRUD for books

ReviewApiController → CRUD for reviews

When you run the application and navigate to:

https://localhost:7259/swagger/index.html

you can access the Swagger UI and test all endpoints.



** The application uses SQLite as a database, and all data (users, books, reviews) is stored locally.