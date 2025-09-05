namespace Pitstop.NotificationService;

public class NotificationWorker : IHostedService, IMessageHandlerCallback
{
    IMessageHandler _messageHandler;
    INotificationRepository _repo;
    IEmailNotifier _emailNotifier;

    public NotificationWorker(IMessageHandler messageHandler, INotificationRepository repo, IEmailNotifier emailNotifier)
    {
        _messageHandler = messageHandler;
        _repo = repo;
        _emailNotifier = emailNotifier;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _messageHandler.Start(this);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _messageHandler.Stop();
        return Task.CompletedTask;
    }

    public async Task<bool> HandleMessageAsync(string messageType, string message)
    {
        try
        {
            JObject messageObject = MessageSerializer.Deserialize(message);
            switch (messageType)
            {
                case "CustomerRegistered":
                    await HandleAsync(messageObject.ToObject<CustomerRegistered>());
                    break;
                case "MaintenanceJobPlanned":
                    await HandleAsync(messageObject.ToObject<MaintenanceJobPlanned>());
                    break;
                case "MaintenanceJobFinished":
                    await HandleAsync(messageObject.ToObject<MaintenanceJobFinished>());
                    break;
                case "DayHasPassed":
                    await HandleAsync(messageObject.ToObject<DayHasPassed>());
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error while handling {messageType} event.");
        }

        return true;
    }

    private async Task HandleAsync(CustomerRegistered cr)
    {
        Customer customer = new Customer
        {
            CustomerId = cr.CustomerId,
            Name = cr.Name,
            TelephoneNumber = cr.TelephoneNumber,
            EmailAddress = cr.EmailAddress
        };

        Log.Information("Register customer: {Id}, {Name}, {TelephoneNumber}, {Email}",
            customer.CustomerId, customer.Name, customer.TelephoneNumber, customer.EmailAddress);

        await _repo.RegisterCustomerAsync(customer);

        // Send welcome email
        StringBuilder body = new StringBuilder();
        body.AppendLine($"Dear {customer.Name},\n");
        body.AppendLine($"Welcome to PitStop! We're excited to have you as our customer.\n");
        body.AppendLine($"You can now schedule maintenance for your vehicles through our system.\n");
        body.AppendLine($"Best regards,\n");
        body.AppendLine($"The PitStop crew");

        Log.Information("Sending welcome email to: {CustomerName}", customer.Name);

        await _emailNotifier.SendEmailAsync(
            customer.EmailAddress, "noreply@pitstop.nl", "Welcome to PitStop!", body.ToString());
    }

    private async Task HandleAsync(MaintenanceJobPlanned mjp)
    {
        MaintenanceJob job = new MaintenanceJob
        {
            JobId = mjp.JobId.ToString(),
            CustomerId = mjp.CustomerInfo.Id,
            LicenseNumber = mjp.VehicleInfo.LicenseNumber,
            StartTime = mjp.StartTime,
            Description = mjp.Description
        };

        Log.Information("Register Maintenance Job: {Id}, {CustomerId}, {VehicleLicenseNumber}, {StartTime}, {Description}",
            job.JobId, job.CustomerId, job.LicenseNumber, job.StartTime, job.Description);

        await _repo.RegisterMaintenanceJobAsync(job);

        // Send confirmation email to customer
        Customer customer = await _repo.GetCustomerAsync(job.CustomerId);
        StringBuilder body = new StringBuilder();
        body.AppendLine($"Dear {customer.Name},\n");
        body.AppendLine($"Your maintenance appointment has been scheduled successfully.\n");
        body.AppendLine($"Appointment details:");
        body.AppendLine($"- Date: {job.StartTime.ToString("dd-MM-yyyy")}");
        body.AppendLine($"- Time: {job.StartTime.ToString("HH:mm")}");
        body.AppendLine($"- Vehicle: {job.LicenseNumber}");
        body.AppendLine($"- Service: {job.Description}\n");
        body.AppendLine($"Please arrive 10 minutes before your scheduled time and check in at our front desk.\n");
        body.AppendLine($"If you need to reschedule or cancel, please contact us as soon as possible.\n");
        body.AppendLine($"Best regards,\n");
        body.AppendLine($"The PitStop crew");

        Log.Information("Sending maintenance scheduling confirmation email to: {CustomerName}", customer.Name);

        await _emailNotifier.SendEmailAsync(
            customer.EmailAddress, 
            "noreply@pitstop.nl", 
            "Your PitStop Maintenance Appointment Confirmation", 
            body.ToString());
    }

    private async Task HandleAsync(MaintenanceJobFinished mjf)
    {
        Log.Information("Remove finished Maintenance Job: {Id}", mjf.JobId);

        // Get the job details before removing it
        var jobs = await _repo.GetMaintenanceJobsAsync(new[] { mjf.JobId.ToString() });
        var job = jobs.FirstOrDefault();
        
        if (job != null)
        {
            // Get customer details
            Customer customer = await _repo.GetCustomerAsync(job.CustomerId);

            // Send completion email
            StringBuilder body = new StringBuilder();
            body.AppendLine($"Dear {customer.Name},\n");
            body.AppendLine($"The maintenance service for your vehicle has been completed successfully.\n");
            body.AppendLine($"Service details:");
            body.AppendLine($"- Vehicle: {job.LicenseNumber}");
            body.AppendLine($"- Service completed: {job.Description}");
            body.AppendLine($"- Completion date: {DateTime.Now.ToString("dd-MM-yyyy HH:mm")}\n");
            body.AppendLine($"Thank you for choosing PitStop for your vehicle maintenance needs.\n");
            body.AppendLine($"If you have any questions about the service performed, please don't hesitate to contact us.\n");
            body.AppendLine($"Best regards,\n");
            body.AppendLine($"The PitStop crew");

            Log.Information("Sending maintenance completion email to: {CustomerName}", customer.Name);

            await _emailNotifier.SendEmailAsync(
                customer.EmailAddress, 
                "noreply@pitstop.nl", 
                "Your PitStop Maintenance Service is Complete", 
                body.ToString());
        }

        await _repo.RemoveMaintenanceJobsAsync(new string[] { mjf.JobId.ToString() });
    }

    private async Task HandleAsync(DayHasPassed dhp)
    {
        DateTime today = DateTime.Now;

        IEnumerable<MaintenanceJob> jobsToNotify = await _repo.GetMaintenanceJobsForTodayAsync(today);
        foreach (var jobsPerCustomer in jobsToNotify.GroupBy(job => job.CustomerId))
        {
            // build notification body
            string customerId = jobsPerCustomer.Key;
            Customer customer = await _repo.GetCustomerAsync(customerId);
            StringBuilder body = new StringBuilder();
            body.AppendLine($"Dear {customer.Name},\n");
            body.AppendLine($"We would like to remind you that you have an appointment with us for maintenance on your vehicle(s):\n");
            foreach (MaintenanceJob job in jobsPerCustomer)
            {
                body.AppendLine($"- {job.StartTime.ToString("dd-MM-yyyy")} at {job.StartTime.ToString("HH:mm")} : " +
                    $"{job.Description} on vehicle with license-number {job.LicenseNumber}");
            }

            body.AppendLine($"\nPlease make sure you're present at least 10 minutes before the (first) job is planned.");
            body.AppendLine($"Once arrived, you can notify your arrival at our front-desk.\n");
            body.AppendLine($"Greetings,\n");
            body.AppendLine($"The PitStop crew");

            Log.Information("Sent notification to: {CustomerName}", customer.Name);

            // send notification
            await _emailNotifier.SendEmailAsync(
                customer.EmailAddress, "noreply@pitstop.nl", "Vehicle maintenance reminder", body.ToString());

            // remove jobs for which a notification was sent
            await _repo.RemoveMaintenanceJobsAsync(jobsPerCustomer.Select(job => job.JobId));
        }
    }
}