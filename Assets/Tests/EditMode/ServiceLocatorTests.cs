using NUnit.Framework;
using ChaosCosmos.Core.Services; // Assumendo che ServiceLocator e IService siano qui

// Interfaccia e classe fittizia per il test
public interface ITestService : IService { bool IsTestService { get; } }
public class TestService : ITestService { public bool IsTestService => true; }

public class ServiceLocatorTests
{
    [TearDown] // Eseguito dopo ogni test per pulire
    public void Teardown()
    {
        if (ServiceLocator.IsRegistered<ITestService>())
        {
            ServiceLocator.Unregister<ITestService>();
        }
         // ServiceLocator.UnregisterAll(); // Alternativa più generale
    }

    [Test]
    public void ServiceLocator_RegistersAndGetsService()
    {
        var service = new TestService();
        ServiceLocator.Register<ITestService>(service);
        Assert.IsTrue(ServiceLocator.IsRegistered<ITestService>());
        var retrievedService = ServiceLocator.Get<ITestService>();
        Assert.IsNotNull(retrievedService);
        Assert.AreSame(service, retrievedService);
        Assert.IsTrue(retrievedService.IsTestService);
    }

    [Test]
    public void ServiceLocator_ThrowsWhenGettingUnregisteredService()
    {
        Assert.Throws<System.InvalidOperationException>(() => {
            ServiceLocator.Get<ITestService>();
        });
    }
}
