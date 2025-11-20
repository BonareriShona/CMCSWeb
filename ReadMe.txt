CMCSWeb – Contract Monthly Claim System
Project Overview

CMCSWeb is a web-based Contract Monthly Claim System built using ASP.NET Core MVC. It allows lecturers to submit claims for hours worked, coordinators to verify them, and managers to approve verified claims. The system also supports file uploads for supporting documentation.

Part 2 Updates 
1. File Upload Validation

Previously, file uploads were optional and could be null, causing errors.

Changes implemented:

File upload is now required for all claim submissions.

Allowed file types updated to: .pdf, .docx, .xlsx, and .jpeg.

Added server-side validation to prevent unsupported file types.

User receives a clear error message if validation fails.

2. Claim Model Adjustments

[Required] attributes were causing issues when Notes or DocumentPath were null.

Changes implemented:

All required properties are now initialized in the form submission.

DocumentPath now has a dummy default during testing to pass unit tests.

Notes field is required with a 500-character limit.

3. Controller Enhancements

LecturerController:

Fixed Submit POST to handle required document uploads.

Added proper error handling and ModelState validation for file uploads.

Default claim status set to Pending.

Track method now properly returns claims in descending order of submission.

4. Views Updates

Submit.cshtml:

Form updated to enforce required file upload.

Validation scripts included for client-side error checking.

Field placeholders and labels improved for clarity.

Coordinator & Manager Views:

Placeholder Manage.cshtml files added to avoid runtime errors when buttons are clicked.

Navigation bar updated for easier access.

5. Unit Testing (XUnit)

Added unit tests to ensure:

Submit GET action returns the form view.

Valid claims are saved to the database.

Claims have default Pending status.

Track returns the list of claims.

Tests use In-Memory Database to avoid affecting production data.

Mocked IWebHostEnvironment to prevent null reference errors.

6. Configuration & Database

Updated appsettings.json for SQL Server connection.

File upload directory set to wwwroot/uploads.

Database migrations used for creating/updating tables.

Added support for seamless connectivity using Trusted_Connection=True.

7. Bug Fixes

Resolved ambiguous Claim reference by using CMCSWeb.Models.Claim.

Fixed null reference errors in unit tests by initializing required fields.

Corrected issues causing the Submit button to appear non-functional.