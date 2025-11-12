#  Proyecto: Juego de Meteoros — Clasificación en Tiempo Real

**Versión de Unity:** 6000.0.36f1  
**Plataforma principal:** Android  
**Integración:** Firebase Firestore  
**Lenguaje:** C#

---

##  Descripción General

Este proyecto es un juego desarrollado en **Unity** donde el jugador esquiva meteoros, acumula puntuación y registra su desempeño en un sistema de clasificación conectado en tiempo real mediante **Firebase Firestore**.

El objetivo es mantener la mayor puntuación posible antes de colisionar.  
Al perder, los datos del jugador (nombre, puntuación, tiempo de sesión, meteoros esquivados y número de intentos) se envían automáticamente a la base de datos y se muestran en la escena de clasificación.

---

##  Flujo General del Proyecto

El flujo de ejecución del proyecto sigue una estructura modular y asincrónica que garantiza estabilidad y sincronización entre las escenas y los servicios en la nube.

### 1. Inicio del Juego
- Al iniciar, se cargan los servicios principales mediante el objeto `GameServices`.
- `FirebaseInitializer` verifica dependencias y establece la conexión con Firebase.
- `FirestoreService` se encarga de la conexión con la base de datos y la escucha de cambios en tiempo real.

### 2. Perfil del Jugador
- `PlayerProfile` mantiene el nombre del jugador y sus datos persistentes entre escenas.
- Utiliza `DontDestroyOnLoad` para asegurar que la información del usuario no se pierda durante los cambios de escena.

### 3. Sesión de Juego
- `GameSession` registra los datos de la partida:
  - Tiempo total jugado.
  - Número de meteoros esquivados.
  - Intentos del jugador.

Estos datos se acumulan en memoria y se reinician al iniciar una nueva sesión.

### 4. Mecánicas del Juego
- `MeteorSpawner` controla la generación y movimiento de meteoros.
- `ScoreManager` lleva el conteo de la puntuación actual del jugador.
- Si el jugador colisiona, `GameManager` detiene la generación de meteoros y marca el final de la partida.

### 5. Registro de Resultados
- `GameManager` recopila todos los datos de la sesión y los envía a `FirestoreService`.
- El método `AddHighscoreAsync()` guarda la información en la colección de Firestore.
- En caso de falla de conexión, los datos se almacenan localmente mediante `LeaderboardManager`.

### 6. Escena de Clasificación
- Al perder, se carga la escena **"Clasificación"**.
- `LeaderboardSimpleUI` muestra los mejores puntajes en tiempo real.
- `FirestoreService.StartListeningTop(10)` mantiene una escucha activa de los 10 mejores resultados registrados en Firestore.

---

##  Instalación y Configuración

### 1. Requisitos Previos
- Unity **6000.0.36f1** o superior.  
- SDK de Android configurado.  
- Cuenta de **Firebase** con un proyecto creado.  
- Archivo **google-services.json** descargado desde la consola de Firebase y ubicado en:

### 2. Configuración en Unity
1. Abre el proyecto desde Unity Hub.  
2. Ve a **Edit > Project Settings > Player > Publishing Settings** y activa las opciones necesarias para Android.  
3. En el menú **Window > Package Manager**, verifica que tengas instalados:
 - **Firebase SDK for Unity (Firestore)**  
 - **TextMeshPro**   
4. Asegúrate de que la escena principal esté incluida en **File > Build Settings > Scenes In Build**.

---

## Funcionamiento de la Conexión con Firebase

1. **Inicialización**  
 - `FirebaseInitializer` ejecuta la verificación de dependencias (`FirebaseApp.CheckAndFixDependenciesAsync()`).
 - Si todo está correcto, inicializa la instancia de Firebase y la mantiene viva entre escenas.

2. **Conexión con Firestore**  
 - `FirestoreService` maneja la conexión y los intentos de reconexión en caso de error.
 - Los datos se guardan con:
   ```csharp
   await firestore.Collection("highscores").AddAsync(entry);
   ```
 - Escucha de puntuaciones en tiempo real mediante `ListenAsync()`:
   ```csharp
   firestore.Collection("highscores")
            .OrderByDescending("score")
            .Limit(10)
            .Listen(snapshot => { ... });
   ```

3. **Gestión Offline**  
 - Si la conexión falla, los resultados se almacenan localmente y se envían más tarde.
 - El sistema maneja hasta 5 intentos automáticos de reconexión.

---

## Flujo de Escenas

1. **Menu Inicio**  
 - Permite ingresar tu nombre y tambien iniciar a jugar.

2. **Juego**  
 - Inicia el juego, genera meteoros y gestiona la partida.

3. **Clasificación**  
 - Se carga automáticamente al perder.
 - Muestra el top de puntuaciones.
 - Escucha actualizaciones en tiempo real desde Firestore.


---

##  Flujo de Información

1. **Inicio del juego:**  
 `GameServices` → inicializa Firebase → FirestoreService listo.

2. **Durante la partida:**  
 `GameSession` → acumula datos de tiempo y meteoros.  
 `ScoreManager` → actualiza puntuación visible.

3. **Colisión:**  
 `GameManager.HandlePlayerHit()` →  
 Recoge datos del jugador → los envía a Firestore → carga la escena "Clasificación".

4. **Escena de Clasificación:**  
 `FirestoreService.StartListeningTop(10)` → recibe datos en tiempo real →  
 `LeaderboardSimpleUI` → actualiza UI dinámicamente.

---

## Compilación en Android

1. Abre **File > Build Settings** y selecciona **Android**.  
2. Asegúrate de que la escena principal esté incluida.  
3. Haz clic en **Switch Platform**.  
4. Configura el paquete:
 - Company Name: `com.tuempresa.juegodemeteoros`
 - Package Name: `com.tuempresa.juegodemeteoros`
5. Haz clic en **Build** o **Build and Run**.

---

## Créditos

**Desarrollado por:**  
Diego Vásquez — Ingeniería Multimedia  

**Frameworks y Servicios:**  
- Unity Engine  
- Firebase Firestore SDK  
- TextMeshPro  

---

## Licencia

Este proyecto se distribuye con fines educativos y de demostración.  
Puede ser modificado o extendido libremente citando al autor original.

---

