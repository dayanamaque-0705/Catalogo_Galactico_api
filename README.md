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


```markdown
# Catalogo_Galactico_api (Web API con EF Core & MySQL)

API Rest para la gestión de personajes, cartas y eventos de Star Wars desarrollada en **.NET 10**, utilizando **Entity Framework Core 9.0.0** y **MySQL Server 8.4**.

---

##  Pasos para ejecutar
### 1. Preparar la Base de Datos en MySQL
Abrir la terminal de MySQL (`mysql -u root -p`) y ejecutar los siguientes comandos para crear la base de datos y el usuario asignado:

```sql
CREATE DATABASE catalogo_galactico_db CHARACTER SET utf8mb4;
CREATE USER 'catalogo_user'@'localhost' IDENTIFIED BY '07112005';
GRANT ALL PRIVILEGES ON catalogo_galactico_db.* TO 'catalogo_user'@'localhost';
FLUSH PRIVILEGES;

```

---

### 2. Configurar Cadenas de Conexión en User Secrets

Guardar las credenciales en el almacén de secretos local para evitar exponer contraseñas en el repositorio:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=catalogo_galactico_db;User=catalogo_user;Password=07112005"

```

---

### 3. Restaurar Librerías

Si las librerías presentan advertencias de versión, asegurar el paquete EF Core y restaurar:

```bash
dotnet add package Pomelo.EntityFrameworkCore.MySql --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet restore
dotnet build

```

---

### 4. Aplicar las Migraciones a MySQL

Ejecutar la sincronización del esquema y sembrado de datos iniciales (*Seed Data*):

```bash
dotnet ef database update

```

---

### 5. Verificación en la Consola de MySQL

Para validar la creación de tablas y el registro de la migración:

```sql
USE catalogo_galactico_db;
SHOW TABLES;
SELECT * FROM __efmigrationshistory;
SELECT * FROM personajes;

```

---

### 6. Iniciar la Web API

Ejecutar el proyecto para abrir Swagger UI y probar los endpoints:

```bash
dotnet run

```

Acceder en el navegador a: `http://localhost:5000/swagger` (o la IP/puerto configurado).



