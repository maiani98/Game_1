using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools; // Per LogAssert
using ChaosCosmos.Gameplay;

namespace ChaosCosmos.Tests.EditMode.Gameplay.Planet
{
    public class PlanetControllerTests
    {
        private GameObject _planetGO;
        private PlanetController _planetController;
        private Rigidbody2D _rb;
        private PlanetGrowthData _testGrowthData;

        [SetUp]
        public void SetUp()
        {
            _planetGO = new GameObject("Test_Planet");
            _rb = _planetGO.AddComponent<Rigidbody2D>();
            _rb.gravityScale = 0;

            _planetController = _planetGO.AddComponent<PlanetController>();

            _testGrowthData = ScriptableObject.CreateInstance<PlanetGrowthData>();
            _testGrowthData.massToRadiusCurve = new AnimationCurve(
                new Keyframe(0f, 0.5f),
                new Keyframe(1f, 1f),
                new Keyframe(4f, 2f),
                new Keyframe(9f, 3f),
                new Keyframe(40f, 6.3245f) // sqrt(40) circa
            );
            _planetController.growthData = _testGrowthData;

            _planetController.baseSpeed = 10f;

            // Chiamare Awake e Start manualmente per i test in Edit Mode
            // PlanetController.Awake() inizializza _rb e chiama UpdateScale()
            // PlanetController.Start() non è definito, quindi non serve chiamarlo.
            _planetController.SendMessage("Awake");
        }

        [TearDown]
        public void TearDown()
        {
            if (_planetGO != null) Object.DestroyImmediate(_planetGO);
            if (_testGrowthData != null) Object.DestroyImmediate(_testGrowthData);
        }

        [Test]
        public void Awake_InitializesRigidbodyAndScale()
        {
            Assert.IsNotNull(_planetController.GetComponent<Rigidbody2D>(), "Rigidbody2D non inizializzato da Awake.");
            // currentMass è 1f di default. Curva: massa 1f -> raggio 1f. Scale = (1,1,1)
            Assert.AreEqual(Vector3.one * 1f, _planetGO.transform.localScale, "Scala iniziale non corretta.");
        }

        [Test]
        public void Awake_WhenGrowthDataIsNull_LogsError()
        {
            // Ricrea un PlanetController senza growthData per questo test specifico
            GameObject tempPlanetGO = new GameObject("TempPlanetNoGrowth");
            PlanetController tempController = tempPlanetGO.AddComponent<PlanetController>();
            // Non assegnare tempController.growthData

            LogAssert.Expect(LogType.Error, "PlanetGrowthData non assegnato a PlanetController!");
            tempController.SendMessage("Awake");
            Object.DestroyImmediate(tempPlanetGO);
        }

        [Test]
        public void SetMovementInput_ZeroInput_SetsZeroVelocity()
        {
            _planetController.SetMovementInput(Vector2.zero);
            Assert.AreEqual(Vector2.zero, _rb.velocity, "La velocità non è zero con input zero.");
        }

        [Test]
        public void SetMovementInput_NonZeroInput_SetsVelocityCorrectly_NoBoost_Mass1()
        {
            float mass = 1f; // Massa iniziale
             // Per raggiungere questa massa, partendo da 1f, aggiungiamo mass - 1f
            if (mass != _planetController.CurrentMass) _planetController.AddMass(mass - _planetController.CurrentMass);
            Assert.AreEqual(mass, _planetController.CurrentMass, 0.001f);


            float expectedSpeedModifier = Mathf.Lerp(1.4f, 0.6f, Mathf.InverseLerp(1f, 40f, mass));
            float expectedSpeed = _planetController.baseSpeed * expectedSpeedModifier * 1f; // currentSpeedMultiplier è 1f

            _planetController.SetMovementInput(Vector2.right);
            Assert.AreEqual(expectedSpeed, _rb.velocity.magnitude, 0.01f, "Magnitudine velocità errata.");
            Assert.AreEqual(Vector2.right, _rb.velocity.normalized, "Direzione velocità errata.");
        }

        [Test]
        public void SetMovementInput_WithSpeedMultiplier_VelocityIsIncreased()
        {
            float mass = 1f;
            if (mass != _planetController.CurrentMass) _planetController.AddMass(mass - _planetController.CurrentMass);

            _planetController.ApplySpeedMultiplier(2.0f, true);
            float expectedSpeedModifier = Mathf.Lerp(1.4f, 0.6f, Mathf.InverseLerp(1f, 40f, mass));
            float expectedSpeed = _planetController.baseSpeed * expectedSpeedModifier * 2.0f;

            _planetController.SetMovementInput(Vector2.up);
            Assert.AreEqual(expectedSpeed, _rb.velocity.magnitude, 0.01f, "Velocità con moltiplicatore errata.");
        }

        [Test]
        public void AddMass_IncrementsMassAndUpdatesScale()
        {
            float initialMass = _planetController.CurrentMass;
            _planetController.AddMass(3f);
            Assert.AreEqual(initialMass + 3f, _planetController.CurrentMass, 0.01f, "Massa non incrementata correttamente.");
            // Curva: massa 4f -> raggio 2f. Scale = (2,2,2)
            Assert.AreEqual(Vector3.one * 2f, _planetGO.transform.localScale, "Scala non aggiornata dopo AddMass.");
        }

        [Test]
        public void AddMass_NegativeOrZeroAmount_DoesNotChangeMass()
        {
            float initialMass = _planetController.CurrentMass;
            _planetController.AddMass(0f);
            Assert.AreEqual(initialMass, _planetController.CurrentMass, "AddMass(0) ha cambiato la massa.");
            _planetController.AddMass(-5f);
            Assert.AreEqual(initialMass, _planetController.CurrentMass, "AddMass(-5) ha cambiato la massa.");
        }

        [Test]
        public void UpdateScale_SetsCorrectScaleBasedOnMassAndGrowthCurve()
        {
            // Massa 1 (default post Awake) -> Raggio 1
            Assert.AreEqual(Vector3.one * 1f, _planetGO.transform.localScale, "Scala iniziale errata.");

            _planetController.AddMass(3f); // Massa diventa 4
            Assert.AreEqual(Vector3.one * 2f, _planetGO.transform.localScale, "Scala per massa 4 errata.");

            _planetController.AddMass(5f); // Massa diventa 9
            Assert.AreEqual(Vector3.one * 3f, _planetGO.transform.localScale, "Scala per massa 9 errata.");
        }

        [Test]
        public void UpdateScale_WhenGrowthDataOrCurveIsNull_LogsWarningAndDoesNotChangeScale()
        {
            Vector3 initialScale = _planetGO.transform.localScale;

            _planetController.growthData = null;
            LogAssert.Expect(LogType.Warning, "PlanetGrowthData o la sua curva non sono configurati. La scala non verrà aggiornata.");
            _planetController.AddMass(1f); // Chiama UpdateScale internamente
            Assert.AreEqual(initialScale, _planetGO.transform.localScale, "Scala cambiata con growthData nullo.");

            // Ripristina growthData, ma con curva nulla
            _planetController.growthData = _testGrowthData;
            AnimationCurve originalCurve = _testGrowthData.massToRadiusCurve;
            _testGrowthData.massToRadiusCurve = null;
            LogAssert.Expect(LogType.Warning, "PlanetGrowthData o la sua curva non sono configurati. La scala non verrà aggiornata.");
            _planetController.AddMass(1f);
            Assert.AreEqual(initialScale, _planetGO.transform.localScale, "Scala cambiata con curva nulla.");
            _testGrowthData.massToRadiusCurve = originalCurve; // Ripristina per altri test
        }

        [Test]
        [TestCase(1f, 1.4f)]
        [TestCase(40f, 0.6f)]
        [TestCase(20.5f, 1.0f)]
        public void GetCurrentSpeedModifierValue_ReturnsCorrectModifierForMass(float mass, float expectedModifier)
        {
            // Resetta la massa a 1f prima di aggiungere la differenza
            _planetController.AddMass(1f - _planetController.CurrentMass);
            _planetController.AddMass(mass - 1f);
            Assert.AreEqual(mass, _planetController.CurrentMass, 0.01f, "Massa non impostata correttamente per il test.");
            Assert.AreEqual(expectedModifier, _planetController.GetCurrentSpeedModifierValue(), 0.01f, "SpeedModifier errato per la massa data.");
        }

        [Test]
        public void ApplySpeedMultiplier_AppliesAndRemovesMultiplierCorrectly()
        {
            _planetController.SetMovementInput(Vector2.right);
            float initialSpeed = _rb.velocity.magnitude; // Velocità con speedModifier base e currentSpeedMultiplier=1

            _planetController.ApplySpeedMultiplier(2.0f, true); // currentSpeedMultiplier ora è 2.0f
            _planetController.SetMovementInput(Vector2.right);
            Assert.AreEqual(initialSpeed * 2.0f, _rb.velocity.magnitude, 0.01f, "Moltiplicatore non applicato.");

            _planetController.ApplySpeedMultiplier(2.0f, false); // currentSpeedMultiplier torna a 1.0f
            _planetController.SetMovementInput(Vector2.right);
            Assert.AreEqual(initialSpeed, _rb.velocity.magnitude, 0.01f, "Moltiplicatore non rimosso.");
        }

        [Test]
        public void ApplySpeedMultiplier_ClampsAtMinimum()
        {
            _planetController.SetMovementInput(Vector2.right);
            float baseVelocityMagnitude = _rb.velocity.magnitude;

            _planetController.ApplySpeedMultiplier(0.01f, true);
            _planetController.SetMovementInput(Vector2.right);
            float expectedMinMultiplier = 0.1f;
            Assert.AreEqual(baseVelocityMagnitude * expectedMinMultiplier, _rb.velocity.magnitude, 0.01f, "Clamp minimo del moltiplicatore fallito.");
        }
    }
}
