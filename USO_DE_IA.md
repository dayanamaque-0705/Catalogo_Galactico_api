# Declaración de Uso de Inteligencia Artificial
## 1. Resolución de Errores e Incidencias Técnicas(gemini)

Se utilizó la asistencia de IA para diagnosticar, depurar y resolver las siguientes incidencias técnicas durante el desarrollo:

- **Bloqueo en Ejecución de Migraciones (`dotnet ef database update`):** Solución a los fallos de compilación y carga de ensamblados durante las migraciones automáticas de Entity Framework Core mediante el script SQL directo sobre MySQL
- **Fallo de Compilación y Ejecución (`dotnet run` / Error `0x800711C7`):** Diagnóstico y solución del bloqueo de seguridad de Windows App Control 
`Catalogo_Galactico_api.dll` mediante la liberación de firmas con `Unblock-File` en PowerShell y la ejecución directa del binario con el runtime de .NET (`dotnet bin/...`).
- **Error 500 Internal Server Error (Enum Mapping Mismatch):** Identificación y corrección de la discrepancia de tipos entre los valores de texto introducidos en MySQL y el mapeo `enum Faccion` en C#.

---

## 2. Estructuración de Documentación

Se utilizó la asistencia de IA para la organización, redacción y estructuración técnica de la documentación del proyecto:

- Redacción estructurada del archivo `README.md`, incluyendo los requisitos previos, inicialización del servicio en Windows 

---

## 3. Protección del reseteo de experiencia

Se utilizó la asistencia de IA para implementar el filtro `AdminApiKeyFilter` y proteger `PUT /personajes/{id}/reseteo`. Esta operación puede reiniciar la experiencia o cambiar el rango de un personaje, por lo que no debía quedar disponible para cualquier consumidor de la API sin autorización.


Se aceptó esta solución porque centraliza la comprobación de autorización en un filtro y evita repetirla dentro de la lógica del controlador. La clave no se incluye en el repositorio ni se devuelve en las respuestas de la API.