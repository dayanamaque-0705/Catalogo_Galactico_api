# Catalogo_Galactico_api
## Caracteristicas Principales
- Gestion de entidades con operaciones CRUD completas para personajes, cartas y eventos.
- Relaciones estructuradas mediante identificadores 
- Reglas de negocio y consistencia temporal:
  - Actualizacion automatica del estado de un personaje a muerto cuando se regsitra
  - Validacion estricta del registrar de participantes de personajes muertos en nuevos eventos.
- Estadisticas y analisis:
  - Filtros combinados por faccion y sensibilidad a la fuerza(enum).
  - Ranking de personajes ordenado por poder.
  - Calculo del participante mas valioso .
- Simulacion de batallas: endpoint especializado que agrupa poderes por bando, aplica un factor aleatorio controlado de menos diez a diez y determina al ganador.

## Tecnologias Utilizadas
- Lenguaje: C# (.NET Core / Minimal APIs)
- Documentacion: Swagger / OpenAPI
- Arquitectura: Patron basado en separacion de responsabilidades (Endpoints, Services, Models, Data)
## Como clonar y ejecutar el proyecto en la universidad
## Como Clonar el proyecto
1. Abre tu terminal o la consola de comandos en la computadora.
2. Clona el repositorio desde GitHub ejecutando el siguiente comando:
   ```bash
   git clone [https://github.com/dayanamaque-0705/Catalogo_Galactico_api.git](https://github.com/dayanamaque-0705/Catalogo_Galactico_api.git)

## Como ejecutar el proyecto
1. Asegurate de tener instalado el SDK de .NET.
2. Abre la carpeta del proyecto en tu terminal.
  ```bash
   cd Catalogo_Galactico_api

   
3. Ejecuta el siguiente comando para iniciar la aplicacion:
   ```bash
   dotnet run
4. http://localhost:5131/swagger


