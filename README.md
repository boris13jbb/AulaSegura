# AulaSegura

AulaSegura es una aplicacion de escritorio para Windows orientada al bloqueo y control de acceso web en entornos familiares, escolares o administrativos. Combina una interfaz WPF para administracion con un Worker Service que aplica reglas sobre el archivo `hosts` y sirve una pagina local de bloqueo.

## Objetivo

El objetivo del sistema es permitir que un administrador gestione listas negras, listas blancas, categorias, palabras clave, horarios, reglas de bloqueo, reportes y configuracion basica desde una aplicacion local. La lista blanca tiene prioridad sobre la lista negra y los cambios se sincronizan con el archivo `hosts` cuando la aplicacion o el servicio tienen permisos de administrador.

## Tecnologias

- .NET 8
- WPF (`net8.0-windows`)
- Worker Service para Windows
- Entity Framework Core 8
- SQLite
- CommunityToolkit.Mvvm
- LiveChartsCore para graficos
- Serilog para logs del servicio
- BCrypt.Net para hashing de contrasenas

## Requisitos Previos

- Windows 10/11
- .NET 8 SDK para desarrollo
- .NET 8 Desktop Runtime para ejecutar la app WPF publicada
- PowerShell 5+ o PowerShell 7+
- Permisos de administrador para modificar `C:\Windows\System32\drivers\etc\hosts` e instalar el servicio

## Instalacion

1. Clonar el repositorio:

   ```powershell
   git clone https://github.com/boris13jbb/AulaSegura.git
   cd AulaSegura
   ```

2. Restaurar dependencias:

   ```powershell
   dotnet restore AulaSegura.sln
   ```

3. Compilar:

   ```powershell
   dotnet build AulaSegura.sln
   ```

4. Ejecutar la aplicacion WPF:

   ```powershell
   dotnet run --project src/AulaSegura.App/AulaSegura.App.csproj
   ```

5. Ejecutar el Worker Service en modo consola/desarrollo:

   ```powershell
   dotnet run --project src/AulaSegura.Service/AulaSegura.Service.csproj
   ```

## Configuracion

La configuracion base vive en:

- `src/AulaSegura.App/appsettings.json`
- `src/AulaSegura.Service/appsettings.json`
- `.env.example`

.NET lee variables de entorno del proceso. El archivo `.env.example` es una referencia para documentar valores, pero no se carga automaticamente sin una herramienta externa.

Variables principales:

| Variable | Uso |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | Ruta SQLite compartida por app y servicio. Por defecto usa `%PROGRAMDATA%\AulaSegura\aulasegura.db`. |
| `Database__RecreateWhenSchemaIsIncomplete` | Permite recrear la BD si el esquema esta incompleto. Mantener `false` salvo respaldo previo. |
| `SeedAdmin__Username` | Usuario inicial del administrador. |
| `SeedAdmin__Password` | Contrasena inicial. No se debe commitear un valor real. |
| `SeedAdmin__Email` | Email inicial del administrador. |
| `SeedAdmin__FullName` | Nombre visible del administrador. |
| `AppSettings__CheckBlockingRulesIntervalSeconds` | Frecuencia del Worker para reaplicar reglas. |
| `AppSettings__BlockingPagePort` | Puerto local de la pagina de bloqueo. |

Si `SeedAdmin__Password` no esta configurada y no existe ningun administrador, AulaSegura genera una contrasena temporal y crea `first-run-admin.txt` junto al ejecutable. Ese archivo esta ignorado por Git y debe eliminarse despues del primer cambio de contrasena.

## Comandos Utiles

```powershell
dotnet restore AulaSegura.sln
dotnet build AulaSegura.sln
dotnet run --project src/AulaSegura.App/AulaSegura.App.csproj
dotnet run --project src/AulaSegura.Service/AulaSegura.Service.csproj
dotnet publish src/AulaSegura.App/AulaSegura.App.csproj -c Release
dotnet publish src/AulaSegura.Service/AulaSegura.Service.csproj -c Release
```

No hay proyectos de pruebas ni scripts de lint configurados actualmente.

## Estructura

```text
AulaSegura/
├── src/
│   ├── AulaSegura.App/             # Aplicacion WPF y ViewModels
│   ├── AulaSegura.Core/            # Entidades, interfaces, constantes, validadores
│   ├── AulaSegura.Infrastructure/  # EF Core, SQLite, repositorios, servicios
│   ├── AulaSegura.Service/         # Worker Service y servidor de pagina de bloqueo
│   └── AulaSegura.Shared/          # Proyecto compartido reservado
├── docs/                           # Documentacion tecnica y manuales
├── scripts/                        # Scripts de instalacion y empaquetado
├── .env.example                    # Variables de entorno documentadas
├── .editorconfig                   # Estilo basico del repositorio
├── .gitignore
└── AulaSegura.sln
```

## Funcionalidades Actuales

- Login de administrador con BCrypt.
- Bloqueo temporal de cuenta por intentos fallidos.
- Gestion de sitios bloqueados.
- Gestion de sitios permitidos.
- Categorias, palabras clave, horarios, reglas y reportes.
- Dashboard con metricas y estado de permisos sobre `hosts`.
- Auditoria de acciones relevantes.
- Sincronizacion de reglas al archivo `hosts`.
- Pagina HTML local para indicar sitio bloqueado.
- Backups basicos de configuracion y del archivo `hosts`.

## Mejoras Aplicadas

- Limpieza de artefactos versionados: `.vs`, `bin`, `obj`, `dist`, paquetes generados y CSVs de reportes.
- Eliminacion de plantilla Worker duplicada en la raiz.
- Eliminacion de archivos placeholder `Class1.cs`.
- `.gitignore` ampliado para logs, backups, `.env`, bases SQLite locales y reportes generados.
- `.env.example` agregado sin secretos reales.
- Configuracion compartida de SQLite mediante `%PROGRAMDATA%\AulaSegura\aulasegura.db`.
- Seed de administrador sin contrasena fija en el codigo.
- Generacion de contrasena temporal local cuando no se configura `SeedAdmin__Password`.
- Validacion y normalizacion de dominios mas robusta.
- Reactivacion segura de dominios eliminados en lista blanca.
- Prevencion de duplicados al editar listas.
- Sincronizacion de hosts despues de cambios en whitelist.
- Cambio de contrasena usando el administrador autenticado.
- Validacion de contrasena consistente entre UI y backend.
- Manejo mas seguro de errores de login y arranque.
- Conversores WPF sin `NotImplementedException` en `ConvertBack`.
- Dashboard conectado al servicio real de configuracion.
- Proteccion contra inyeccion HTML en la pagina de bloqueo.
- Inicializacion de BD sin borrado automatico por defecto si el esquema esta incompleto.

## Notas Para Desarrolladores

- La modificacion del archivo `hosts` requiere ejecutar la app o el servicio como administrador.
- La base de datos local no debe commitearse.
- `Database__RecreateWhenSchemaIsIncomplete=true` puede borrar datos; usar solo con respaldo.
- Los documentos de `docs/` incluyen historico del desarrollo; el README es la referencia operativa principal.
- Antes de publicar una version, ejecutar `dotnet restore`, `dotnet build` y validar manualmente app + servicio en Windows con permisos de administrador.

## Posibles Mejoras Futuras

- Agregar proyectos de pruebas unitarias e integracion.
- Introducir migraciones EF Core en lugar de `EnsureCreated`.
- Implementar roles/permisos para multiples administradores.
- Agregar recuperacion segura de cuenta inicial.
- Mejorar instalador y empaquetado versionado.
- Agregar monitoreo de cambios externos en `hosts`.
- Agregar CI con build, tests y analisis estatico.
