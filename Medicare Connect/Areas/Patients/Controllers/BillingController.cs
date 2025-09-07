using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medicare_Connect.Data;
using Medicare_Connect.Data.Entities;
using System.Security.Claims;
using Medicare_Connect.Areas.Patients.Models;

namespace Medicare_Connect.Areas.Patients.Controllers;

[Area("Patients")]
[Authorize(Roles = "Patients")]
public class BillingController : Controller
{
    private readonly ApplicationDbContext _context;

    public BillingController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(patientId))
        {
            TempData["ErrorMessage"] = "Patient ID not found.";
            return RedirectToAction("Index", "Dashboard");
        }

        try
        {
            // Get all billing records for this patient
            var billingRecords = await _context.Billings
                .Where(b => b.PatientId == patientId)
                .OrderByDescending(b => b.ServiceDate)
                .ToListAsync();

            // Get payment records for this patient
            var paymentRecords = await _context.Payments
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Calculate summary statistics
            var totalBilled = billingRecords.Sum(b => b.Amount);
            var totalPaid = paymentRecords.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);
            var outstandingAmount = billingRecords
                .Where(b => b.PaymentStatus != "Paid")
                .Sum(b => b.PatientResponsibility);
            var overdueAmount = billingRecords
                .Where(b => b.IsOverdue)
                .Sum(b => b.PatientResponsibility);

            var viewModel = new PatientBillingViewModel
            {
                BillingRecords = billingRecords.Select(b => new PatientBillingItem
                {
                    Id = b.Id,
                    ServiceType = b.ServiceType,
                    ServiceDate = b.ServiceDate,
                    Amount = b.Amount,
                    InsuranceAmount = b.InsuranceAmount,
                    PatientResponsibility = b.PatientResponsibility,
                    PaymentStatus = b.PaymentStatus,
                    DueDate = b.DueDate,
                    IsOverdue = b.IsOverdue,
                    Notes = b.Notes,
                    CreatedAt = b.CreatedAt
                }).ToList(),

                PaymentRecords = paymentRecords.Select(p => new PatientPaymentItem
                {
                    Id = p.Id,
                    PaymentType = p.PaymentType.ToString(),
                    Description = p.Description,
                    Amount = p.Amount,
                    Status = p.Status.ToString(),
                    CreatedAt = p.CreatedAt,
                    CompletedAt = p.CompletedAt,
                    Notes = p.Notes
                }).ToList(),

                Summary = new BillingSummary
                {
                    TotalBilled = totalBilled,
                    TotalPaid = totalPaid,
                    OutstandingAmount = outstandingAmount,
                    OverdueAmount = overdueAmount,
                    BillingCount = billingRecords.Count,
                    PaymentCount = paymentRecords.Count
                }
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred while retrieving billing information: {ex.Message}";
            return RedirectToAction("Index", "Dashboard");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(patientId))
        {
            TempData["ErrorMessage"] = "Patient ID not found.";
            return RedirectToAction("Index");
        }

        try
        {
            var billing = await _context.Billings
                .FirstOrDefaultAsync(b => b.Id == id && b.PatientId == patientId);

            if (billing == null)
            {
                TempData["ErrorMessage"] = "Billing record not found.";
                return RedirectToAction("Index");
            }

            var viewModel = new PatientBillingDetailViewModel
            {
                Id = billing.Id,
                ServiceType = billing.ServiceType,
                ServiceDate = billing.ServiceDate,
                Amount = billing.Amount,
                InsuranceAmount = billing.InsuranceAmount,
                PatientResponsibility = billing.PatientResponsibility,
                PaymentStatus = billing.PaymentStatus,
                DueDate = billing.DueDate,
                IsOverdue = billing.IsOverdue,
                Notes = billing.Notes,
                CreatedAt = billing.CreatedAt,
                UpdatedAt = billing.UpdatedAt
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Outstanding()
    {
        var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(patientId))
        {
            TempData["ErrorMessage"] = "Patient ID not found.";
            return RedirectToAction("Index", "Dashboard");
        }

        try
        {
            var outstandingBills = await _context.Billings
                .Where(b => b.PatientId == patientId && b.PaymentStatus != "Paid")
                .OrderBy(b => b.DueDate)
                .ToListAsync();

            var viewModel = new OutstandingBillsViewModel
            {
                Bills = outstandingBills.Select(b => new OutstandingBillItem
                {
                    Id = b.Id,
                    ServiceType = b.ServiceType,
                    ServiceDate = b.ServiceDate,
                    Amount = b.Amount,
                    InsuranceAmount = b.InsuranceAmount,
                    PatientResponsibility = b.PatientResponsibility,
                    PaymentStatus = b.PaymentStatus,
                    DueDate = b.DueDate,
                    IsOverdue = b.IsOverdue,
                    DaysOverdue = b.IsOverdue ? (DateTime.Today - b.DueDate).Days : 0,
                    Notes = b.Notes
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    [HttpGet]
    public async Task<IActionResult> PaymentHistory()
    {
        var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(patientId))
        {
            TempData["ErrorMessage"] = "Patient ID not found.";
            return RedirectToAction("Index", "Dashboard");
        }

        try
        {
            var paymentHistory = await _context.Payments
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var viewModel = new PaymentHistoryViewModel
            {
                Payments = paymentHistory.Select(p => new PaymentHistoryItem
                {
                    Id = p.Id,
                    PaymentType = p.PaymentType.ToString(),
                    Description = p.Description,
                    Amount = p.Amount,
                    Status = p.Status.ToString(),
                    CreatedAt = p.CreatedAt,
                    CompletedAt = p.CompletedAt,
                    Notes = p.Notes,
                    ReferenceId = p.ReferenceId
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return RedirectToAction("Index");
        }
    }
} 