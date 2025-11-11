using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Extensions;

public class FirebaseInitializer : MonoBehaviour
{
    public static FirebaseInitializer Instance { get; private set; }
    public static bool IsInitialized { get; private set; }
    public static FirebaseApp AppInstance { get; private set; }

    private void Awake()
    {
        // Implementación del patrón Singleton
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Se encontró una instancia existente de FirebaseInitializer, destruyendo la nueva...");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("Iniciando Firebase...");
        InitializeFirebase();
    }

    private async void InitializeFirebase()
    {
        if (IsInitialized)
        {
            Debug.Log("Firebase ya está inicializado.");
            return;
        }

        try
        {
            Debug.Log("Verificando dependencias de Firebase...");
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            
            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("Dependencias de Firebase verificadas correctamente.");
                
                try
                {
                    // Intentar obtener la instancia existente primero
                    AppInstance = FirebaseApp.DefaultInstance;
                    
                    if (AppInstance != null)
                    {
                        Debug.Log("Se encontró una instancia existente de Firebase.");
                        IsInitialized = true;
                        return;
                    }

                    // Si no hay instancia, crear una nueva
                    var options = new AppOptions();
                    AppInstance = FirebaseApp.Create(options);
                    
                    if (AppInstance != null)
                    {
                        Debug.Log("Nueva instancia de Firebase creada correctamente.");
                        IsInitialized = true;
                    }
                    else
                    {
                        throw new Exception("No se pudo crear la instancia de Firebase");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error al inicializar Firebase: {ex.Message}");
                    IsInitialized = false;
                    AppInstance = null;
                }
            }
            else
            {
                Debug.LogError($"No se pudieron resolver las dependencias de Firebase: {dependencyStatus}");
                IsInitialized = false;
                AppInstance = null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Firebase initialization error: {ex.Message}");
        }
    }
}