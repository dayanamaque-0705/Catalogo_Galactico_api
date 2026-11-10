Markdown
# Catalogo_Galactico_api

API REST desarrollada en **.NET 10 (Minimal APIs)** con **MySQL Server 8.4** y **Entity Framework Core 9.0**, orientada a la gestión del catálogo galáctico, simulación de batallas y un sistema dinámico de experiencia y rangos para personajes y cartas de Star Wars.

---

## Características Principales

- **Gestión de Entidades (CRUD):** Operaciones completas para administrar personajes, cartas y eventos galácticos.
- **Reglas de Negocio y Consistencia Temporal:**
  - Actualización automática del estado del personaje al registrar su deceso.
  - Validaciones estrictas que impiden la participación de personajes muertos en nuevos eventos.
- **Sistema Dinámico de XP y Ascenso de Rangos:**
  - Endpoint dedicado `POST /personajes/{id}/xp` para otorgar experiencia.
  - Ascenso automático de rango según los puntos de XP acumulados (*Recluta*, *Comandante*, *General*).
  - Incremento proporcional en el `NivelPeligrosidad` de la carta asociada al personaje al subir de nivel.
- **Estadísticas y Análisis:**
  - Filtros combinados por facción y sensibilidad a la fuerza (`enum`).
  - Ranking de personajes ordenados por nivel de poder y experiencia.
  - Cálculo del participante más valioso en batallas.
- **Simulación de Batallas:** Endpoint especializado que agrupa los niveles de poder por bando, aplica un factor aleatorio controlado (de -10 a +10) y determina al ganador.

---

## Tecnologías Utilizadas

- **Lenguaje & Framework:** C# (.NET 10 Minimal APIs)
- **Base de Datos & ORM:** MySQL Server 8.4 & Entity Framework Core 9.0.0 (Pomelo Provider)
- **Documentación & Pruebas:** Swagger UI / OpenAPI
- **Arquitectura:** Patrón por separación de responsabilidades (`Endpoints`, `Services`, `Models`, `Data`)

---

## Guía de Instalación y Configuración Paso a Paso

### 1. Clonar el Proyecto y Restaurar Paquetes

Abre tu terminal en la computadora y clona el repositorio desde GitHub:

```bash
git clone [https://github.com/dayanamaque-0705/Catalogo_Galactico_api.git](https://github.com/dayanamaque-0705/Catalogo_Galactico_api.git)
cd Catalogo_Galactico_api
Descargar e instalar librerías (dotnet restore):
Al clonar por primera vez o instalar el proyecto de cero, ejecuta:

Bash
dotnet restore
¿Para qué sirve dotnet restore? Lee el archivo .csproj y descarga automáticamente todas las librerías NuGet requeridas (como Pomelo.EntityFrameworkCore.MySql y Microsoft.EntityFrameworkCore.Design) necesarias para compilar el proyecto.

2. Instalación y Configuración de MySQL Server
Si aún no tienes el servidor de MySQL instalado, abre PowerShell como Administrador y ejecuta:

a) Instalación del motor MySQL con winget:
PowerShell
winget install --id Oracle.MySQL -e
b) Inicialización del servicio en Windows:
Si el servicio no se levanta automáticamente, inicialízalo y levántalo con:

DOS
"C:\Program Files\MySQL\MySQL Server 8.4\bin\mysqld.exe" --initialize-insecure
net start MySQL84
3. Crear la Base de Datos y Credenciales
Ingresa al cliente de MySQL como usuario root:

Desde PowerShell / CMD:

DOS
"C:\Program Files\MySQL\MySQL Server 8.4\bin\mysql.exe" -u root
Desde Git Bash:

Bash
"/c/Program Files/MySQL/MySQL Server 8.4/bin/mysql.exe" -u root
Ejecuta las siguientes sentencias SQL para crear la base de datos catalogo_galactico_db y el usuario del proyecto (catalogo_user / 07112005):

SQL
CREATE DATABASE IF NOT EXISTS catalogo_galactico_db CHARACTER SET utf8mb4;

CREATE USER IF NOT EXISTS 'catalogo_user'@'localhost' IDENTIFIED BY '07112005';

GRANT ALL PRIVILEGES ON catalogo_galactico_db.* TO 'catalogo_user'@'localhost';
FLUSH PRIVILEGES;
4. Configurar Cadena de Conexión (Secret Manager)
En la terminal dentro del proyecto, configura la cadena de conexión local:

Bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=catalogo_galactico_db;User=catalogo_user;Password=07112005;"
dotnet user-secrets list
5. Configurar la Base de Datos (Elegir Opción A u Opción B)
Puedes crear y estructurar las tablas en MySQL utilizando cualquiera de estas dos opciones:

🔹 OPCIÓN A: Utilizando Entity Framework Core (Automático)
Instala la herramienta global de EF (si no la tienes):

Bash
dotnet tool install --global dotnet-ef --version 9.0.0
Aplica las migraciones automáticamente:

Bash
dotnet ef database update
(Si Windows arroja un error de bloqueo de archivo 0x800711C7, ejecuta dotnet ef database update --no-build).

🔹 OPCIÓN B: Mediante Script SQL Directo en MySQL (Manual / Respaldo)
Si prefieres ejecutar el esquema directamente o tuviste problemas de compilación en Windows, ingresa a MySQL con el usuario del proyecto:

Bash
"/c/Program Files/MySQL/MySQL Server 8.4/bin/mysql.exe" -u catalogo_user -p catalogo_galactico_db
(Ingresa la contraseña 07112005)

Y ejecuta las sentencias para crear las tablas y registrar la migración:

SQL
USE catalogo_galactico_db;

-- 1. Crear tabla personajes
CREATE TABLE IF NOT EXISTS personajes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Especie VARCHAR(80) NOT NULL,
    Faccion INT NOT NULL,
    Afiliacion VARCHAR(100) NOT NULL,
    Estado INT NOT NULL,
    FuerzaSensitivo TINYINT(1) NOT NULL,
    XP INT NOT NULL DEFAULT 0,
    Rango VARCHAR(50) NOT NULL DEFAULT 'Recluta'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. Crear tabla cartas
CREATE TABLE IF NOT EXISTS cartas (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    NombreCarta VARCHAR(100) NOT NULL,
    NivelPeligrosidad INT NOT NULL,
    PersonajeId INT NOT NULL,
    CONSTRAINT FK_cartas_personajes_PersonajeId 
        FOREIGN KEY (PersonajeId) REFERENCES personajes (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3. Historial de migraciones de EF Core
CREATE TABLE IF NOT EXISTS `__efmigrationshistory` (
    `MigrationId` VARCHAR(150) NOT NULL,
    `ProductVersion` VARCHAR(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO `__efmigrationshistory` (`MigrationId`, `ProductVersion`) 
VALUES ('20261109000000_SistemaXPAndRango', '9.0.0')
ON DUPLICATE KEY UPDATE `ProductVersion` = '9.0.0';
6. Verificación de la Estructura en MySQL
En la consola de MySQL (mysql>), confirma que las tablas fueron creadas correctamente:

SQL
SHOW TABLES;
DESCRIBE personajes;
DESCRIBE cartas;
7. Ejecución de la API y Pruebas en Swagger UI
a) Iniciar la API:
Bash
dotnet run
b) Abrir Swagger UI:
Ingresa en tu navegador a: http://localhost:5131/swagger

c) Paso a Paso para probar el Sistema de XP y Rangos:
Crear un Personaje (POST /personajes):
Envía el siguiente JSON en el cuerpo de la petición (recordando usar enteros para los enums como faccion: 0 para Jedi):

JSON
{
  "nombre": "Luke Skywalker",
  "especie": "Humano",
  "faccion": 0,
  "afiliacion": "Alianza Rebelde",
  "estado": 0,
  "fuerzaSensitivo": true
}
Asociar una Carta en MySQL:
Para asegurar que el nivel de peligrosidad aumente sin errores 404, inserta la carta asociada al PersonajeId = 1:

SQL
INSERT INTO cartas (NombreCarta, NivelPeligrosidad, PersonajeId) 
VALUES ('Sable de Luz', 1, 1);
Otorgar Puntos de XP (POST /personajes/{id}/xp):

Selecciona el endpoint POST /personajes/1/xp.

Haz clic en Try it out.

En el cuerpo de la petición (Request Body), ingresa únicamente el número entero plano (ejemplo: 100), sin llaves ni comillas.

Haz clic en Execute.