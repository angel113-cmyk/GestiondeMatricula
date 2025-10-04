# 🎓 Portal Académico - Gestión de Cursos y Matrículas

Sistema web para gestión de cursos, estudiantes y matrículas universitarias.

## 🚀 Características

- **Autenticación de usuarios** con Identity
- **Catálogo de cursos** con filtros avanzados
- **Sistema de matrículas** con validaciones
- **Panel de coordinador** con CRUD completo
- **Cache con Redis** para mejor rendimiento
- **Sesiones de usuario** para experiencia personalizada

## 🛠️ Tecnologías

- ASP.NET Core 8.0 MVC
- Entity Framework Core + SQLite
- Identity para autenticación
- Redis para cache y sesiones
- Bootstrap 5 para interfaz
- Docker para contenerización
- Render.com para hosting

## 📦 Instalación Local

### Prerrequisitos
- .NET 8.0 SDK
- SQLite
- Redis (opcional)

### Pasos
```bash
# 1. Clonar repositorio
git clone https://github.com/tu-usuario/GestiondeMatricula.git

# 2. Entrar al directorio
cd GestiondeMatricula

# 3. Restaurar paquetes
dotnet restore

# 4. Ejecutar migraciones
dotnet ef database update

# 5. Ejecutar aplicación
dotnet run

Credenciales de Prueba
Coordinador: coordinador@universidad.edu / Coordinador123!

Usuarios regulares: Pueden registrarse desde la aplicación
