# 🛡️ API de Autenticación ASP.NET Core con JWT y SendGrid

Esta API proporciona un sistema completo de autenticación con:

- Inicio de sesión con JWT
- Confirmación de correo electrónico
- Cambio y recuperación de contraseña
- Envío de correos con SendGrid
- Preparada para Docker

---

## ⚙️ Configuración de entorno (`appsettings.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",                  // Nivel de log por defecto
      "Microsoft.AspNetCore": "Warning"          // Nivel de log para ASP.NET Core
    }
  },
  "AllowedHosts": "*",                           // Hosts permitidos (usar * para todos)

  "DbSettings": {
    "Host": "postgres",                          // Dirección del servidor de base de datos (p.ej. localhost o postgres en Docker)
    "Port": 5432,                                // Puerto de PostgreSQL
    "Username": "postgres",                      // Usuario de la base de datos
    "Password": "postgres",                      // Contraseña de la base de datos
    "Database": "postgres"                       // Nombre de la base de datos
  },

  "JwtSettings": {
    "SecretKey": "llavesecreta"                  // Clave secreta para firmar los tokens JWT
  },

  "EmailSettings": {
    "UserEmail": "emailcorreo",                  // Dirección de correo desde la cual se enviarán los emails (verificada en SendGrid)
    "UserName": "name",                          // Nombre que aparecerá como remitente en los correos
    "UserApiKey": "key",                         // Clave API de SendGrid para enviar correos
    "Host": "https://tusitio.com"                // URL del frontend donde el usuario será redirigido para confirmar correo o cambiar contraseña
                                                // Ej: https://miweb.com → enlaces enviados al usuario serán como:
                                                // https://miweb.com/confirm-email?token=... o /reset-password?token=...
  }
}
