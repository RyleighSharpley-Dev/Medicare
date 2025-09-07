# BillingViewModel Class Structure

## Class Diagram

```mermaid
classDiagram
    class BillingViewModel {
        +string PatientId
        +string ServiceType
        +DateTime ServiceDate
        +decimal Amount
        +bool InsuranceCoverage
        +decimal? InsuranceAmount
        +decimal PatientResponsibility
        +string PaymentStatus
        +DateTime DueDate
        +string? Notes
        
        +ValidationAttributes()
        +BusinessLogic()
    }
    
    class BillingListItem {
        +string Id
        +string PatientName
        +string ServiceType
        +DateTime ServiceDate
        +decimal Amount
        +decimal? InsuranceAmount
        +decimal PatientResponsibility
        +string PaymentStatus
        +DateTime DueDate
        +bool IsOverdue
    }
    
    BillingViewModel --> BillingListItem : converts to
```

## Property Details

```mermaid
graph TD
    A[BillingViewModel] --> B[PatientId]
    A --> C[ServiceType]
    A --> D[ServiceDate]
    A --> E[Amount]
    A --> F[InsuranceCoverage]
    A --> G[InsuranceAmount]
    A --> H[PatientResponsibility]
    A --> I[PaymentStatus]
    A --> J[DueDate]
    A --> K[Notes]
    
    B --> B1[Required]
    B --> B2[string]
    
    C --> C1[Required]
    C --> C2[string]
    
    D --> D1[Required]
    D --> D2[DataType: Date]
    D --> D3[Default: Today]
    
    E --> E1[Required]
    E --> E2[Range: 0.01-10000.00]
    E --> E3[decimal]
    
    F --> F1[bool]
    F --> F2[Default: false]
    
    G --> G1[Range: 0.00-10000.00]
    G --> G2[decimal?]
    G --> G3[Nullable]
    
    H --> H1[Range: 0.00-10000.00]
    H --> H2[decimal]
    
    I --> I1[string]
    I --> I2[Default: "Pending"]
    
    J --> J1[DataType: Date]
    J --> J2[Default: Today + 30 days]
    
    K --> K1[StringLength: 500]
    K --> K2[string?]
    K --> K3[Nullable]
```

## Data Flow

```mermaid
flowchart LR
    A[Admin Input] --> B[BillingViewModel]
    B --> C[Validation]
    C --> D[Business Logic]
    D --> E[Database Save]
    E --> F[BillingEntity]
    
    G[Database Query] --> H[BillingListItem]
    H --> I[Admin View Display]
    
    B --> J[Form Binding]
    J --> K[ModelState Validation]
    K --> L[Success/Error Response]
```

## Validation Rules

```mermaid
graph LR
    A[Validation Rules] --> B[Required Fields]
    A --> C[Range Constraints]
    A --> D[Data Types]
    A --> E[String Lengths]
    
    B --> B1[PatientId]
    B --> B2[ServiceType]
    B --> B3[ServiceDate]
    B --> B4[Amount]
    B --> B5[PaymentStatus]
    B --> B6[DueDate]
    
    C --> C1[Amount: 0.01-10000.00]
    C --> C2[InsuranceAmount: 0.00-10000.00]
    C --> C3[PatientResponsibility: 0.00-10000.00]
    
    D --> D1[ServiceDate: Date]
    D --> D2[DueDate: Date]
    D --> D3[Amount: decimal]
    
    E --> E1[Notes: Max 500 chars]
```

## Business Logic Flow

```mermaid
sequenceDiagram
    participant Admin
    participant Controller
    participant ViewModel
    participant Database
    participant View
    
    Admin->>Controller: Submit Billing Form
    Controller->>ViewModel: Bind Form Data
    ViewModel->>Controller: Validate Model
    
    alt Validation Success
        Controller->>ViewModel: Calculate PatientResponsibility
        Controller->>Database: Save BillingEntity
        Database-->>Controller: Success
        Controller->>View: Redirect with Success Message
    else Validation Failed
        Controller->>View: Return View with Errors
    end
    
    View->>Admin: Display Result
```

## Entity Relationships

```mermaid
erDiagram
    BillingViewModel {
        string PatientId FK
        string ServiceType
        date ServiceDate
        decimal Amount
        bool InsuranceCoverage
        decimal InsuranceAmount
        decimal PatientResponsibility
        string PaymentStatus
        date DueDate
        string Notes
    }
    
    BillingEntity {
        string Id PK
        string PatientId FK
        string ServiceType
        date ServiceDate
        decimal Amount
        decimal InsuranceAmount
        decimal PatientResponsibility
        string PaymentStatus
        date DueDate
        string Notes
        date CreatedAt
        date UpdatedAt
    }
    
    IdentityUser {
        string Id PK
        string UserName
        string Email
    }
    
    BillingViewModel ||--|| BillingEntity : converts to
    BillingEntity ||--|| IdentityUser : belongs to
    BillingViewModel ||--|| IdentityUser : references
``` 