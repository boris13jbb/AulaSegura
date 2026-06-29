# 📖 AulaSegura Control Web - Manual del Usuario

**Versión:** 1.0.0  
**Fecha:** 24 de Abril, 2026  
**Dirigido a:** Administradores del sistema (padres, educadores, TI)

---

## 📋 Tabla de Contenidos

1. [Introducción](#introducción)
2. [Primeros Pasos](#primeros-pasos)
3. [Gestión de Sitios Bloqueados](#gestión-de-sitios-bloqueados)
4. [Gestión de Sitios Permitidos](#gestión-de-sitios-permitidos)
5. [Categorías de Contenido](#categorías-de-contenido)
6. [Horarios de Bloqueo](#horarios-de-bloqueo)
7. [Registro de Actividad](#registro-de-actividad)
8. [Configuración del Sistema](#configuración-del-sistema)
9. [Copias de Seguridad](#copias-de-seguridad)
10. [Preguntas Frecuentes](#preguntas-frecuentes)

---

## 🎯 Introducción

### ¿Qué es AulaSegura Control Web?

AulaSegura Control Web es una aplicación de **control parental y filtrado web** diseñada para escuelas y hogares. Permite:

- ✅ **Bloquear contenido inapropiado** (sitios para adultos, juegos, redes sociales)
- ✅ **Permitir acceso selectivo** a sitios educativos o necesarios
- ✅ **Programar horarios** de bloqueo (ejemplo: bloquear durante clases)
- ✅ **Monitorear actividad** con registros detallados
- ✅ **Proteger múltiples dispositivos** desde una única consola

### ¿Cómo Funciona?

AulaSegura utiliza el **archivo hosts de Windows** para redirigir sitios bloqueados a `127.0.0.1` (localhost), efectivamente impidiendo el acceso. Este método es:

- 🔒 **Seguro:** No requiere software adicional en los navegadores
- ⚡ **Rápido:** Bloqueo a nivel de DNS, antes de que la página cargue
- 🛡️ **Difícil de evadir:** Requiere permisos de administrador para modificar

### Componentes del Sistema

1. **Servicio Windows (Background)**
   - Se ejecuta automáticamente al iniciar Windows
   - Monitorea cambios cada 60 segundos
   - Aplica reglas de bloqueo al archivo hosts
   - Crea copias de seguridad automáticas

2. **Aplicación WPF (Interfaz Gráfica)**
   - Permite configurar reglas de bloqueo
   - Visualizar registros de actividad
   - Gestionar categorías y horarios
   - Realizar copias de seguridad manuales

---

## 🚀 Primeros Pasos

### Iniciar Sesión

1. **Abrir la aplicación:**
   - Doble-click en `AulaSegura.App.exe`
   - O usar acceso directo en el escritorio

2. **Pantalla de Login:**
   ```
   ┌─────────────────────────────┐
   │          🔒                 │
   │      AulaSegura             │
   │      Control Web            │
   │                             │
   │  Usuario: [____________]    │
   │  Contraseña: [__________]   │
   │                             │
   │  [🔑 Iniciar Sesión]        │
   │  [Salir]                    │
   └─────────────────────────────┘
   ```

3. **Ingresar credenciales:**
   - **Usuario por defecto:** `admin`
   - **Contraseña por defecto:** `SeedAdmin__Password o first-run-admin.txt`

4. **Click en "Iniciar Sesión"**

5. **¡Bienvenido!** Verá el dashboard principal.

### ⚠️ IMPORTANTE: Cambiar Contraseña

Después del primer inicio de sesión, **cambie la contraseña por defecto**:

1. Ir a **Configuración** → **Perfil de Administrador**
2. Click en **"Cambiar Contraseña"**
3. Ingresar:
   - Contraseña actual: `SeedAdmin__Password o first-run-admin.txt`
   - Nueva contraseña: (mínimo 8 caracteres, mayúsculas, minúsculas, números)
   - Confirmar nueva contraseña
4. Click en **"Guardar"**

**Recomendación:** Use una contraseña fuerte como `MiEscuela@2026!`

---

## 🚫 Gestión de Sitios Bloqueados

### Agregar un Sitio Bloqueado

1. **Navegar a:** Dashboard → **Sitios Bloqueados** → **Agregar Nuevo**

2. **Completar formulario:**
   ```
   Dominio: facebook.com
   Categoría: Redes Sociales
   Bloquear subdominios: ☑ Sí
   Motivo: Distracción durante clases
   Activo: ☑ Sí
   ```

3. **Click en "Guardar"**

4. **Resultado:**
   - El sitio se agrega a la lista
   - El servicio aplicará el bloqueo en el próximo ciclo (máximo 60 segundos)
   - Los usuarios no podrán acceder a `facebook.com`, `www.facebook.com`, etc.

### Ejemplos Comunes de Sitios para Bloquear

| Sitio | Dominio | Categoría | Motivo |
|-------|---------|-----------|--------|
| Facebook | `facebook.com` | Redes Sociales | Distracción |
| YouTube | `youtube.com` | Streaming | Uso excesivo |
| TikTok | `tiktok.com` | Redes Sociales | Contenido inapropiado |
| Pornhub | `pornhub.com` | Adultos | Contenido explícito |
| Steam | `store.steampowered.com` | Juegos | Distracción |
| Twitter/X | `x.com` | Redes Sociales | Distracción |
| Instagram | `instagram.com` | Redes Sociales | Distracción |
| Netflix | `netflix.com` | Streaming | Uso no autorizado |

### Editar un Sitio Bloqueado

1. En la lista de sitios bloqueados, hacer **click derecho** en el sitio
2. Seleccionar **"Editar"**
3. Modificar los campos necesarios
4. Click en **"Guardar Cambios"**

### Eliminar un Sitio Bloqueado

1. En la lista, seleccionar el sitio
2. Click en botón **"Eliminar"** (o tecla Supr)
3. Confirmar eliminación
4. El sitio será removido del archivo hosts en el próximo ciclo

### Bloquear Subdominios

Cuando marca **"Bloquear subdominios"**, el sistema bloqueará automáticamente:

- `example.com` → Bloqueado
- `www.example.com` → Bloqueado
- `mail.example.com` → Bloqueado
- `api.example.com` → Bloqueado

**Recomendación:** Activar esta opción para bloqueos completos.

---

## ✅ Gestión de Sitios Permitidos (Lista Blanca)

### ¿Cuándo Usar la Lista Blanca?

La lista blanca (whitelist) permite **excepciones** a las reglas de bloqueo. Útil para:

- Permitir un sitio específico de una categoría bloqueada
- Acceso educativo a plataformas necesarias
- Excepciones temporales para proyectos especiales

### Agregar un Sitio Permitido

1. **Navegar a:** Dashboard → **Sitios Permitidos** → **Agregar Nuevo**

2. **Completar formulario:**
   ```
   Dominio: classroom.google.com
   Categoría: Educación
   Motivo: Plataforma educativa requerida
   Activo: ☑ Sí
   ```

3. **Click en "Guardar"**

4. **Resultado:**
   - Aunque "Google" esté bloqueado por otra regla, `classroom.google.com` seguirá accesible
   - La lista blanca tiene **prioridad** sobre la lista negra

### Ejemplos de Sitios para Lista Blanca

| Situación | Sitio Permitido | Razón |
|-----------|----------------|-------|
| YouTube bloqueado | `edu.youtube.com` | Canal educativo específico |
| Redes sociales bloqueadas | `linkedin.com` | Búsqueda de empleo (secundaria) |
| Juegos bloqueados | `code.org` | Aprendizaje de programación |
| Streaming bloqueado | `ted.com` | Charlas educativas |

### Prioridad de Reglas

El sistema aplica las reglas en este orden:

1. ✅ **Lista Blanca** (permitidos) - **MÁXIMA PRIORIDAD**
2. 🚫 **Lista Negra** (bloqueados)
3. 📂 **Categorías** (bloqueos por categoría)

**Ejemplo:**
- `youtube.com` está en Lista Negra → **BLOQUEADO**
- `edu.youtube.com` está en Lista Blanca → **PERMITIDO** (aunque sea subdominio)

---

## 📂 Categorías de Contenido

### Categorías Predefinidas

El sistema incluye 8 categorías predefinidas:

1. **🔞 Adultos** - Contenido explícito, pornografía
2. **💬 Redes Sociales** - Facebook, Instagram, TikTok, Twitter
3. **🎮 Juegos** - Steam, Epic Games, sitios de juegos online
4. **🎰 Apuestas** - Sitios de apuestas, casinos online
5. **🎬 Entretenimiento** - Sitios de entretenimiento general
6. **📺 Streaming** - Netflix, YouTube, Twitch
7. **💭 Chat** - WhatsApp Web, Telegram Web, Discord
8. **⚙️ Personalizadas** - Para categorías creadas por el usuario

### Bloquear por Categoría

En lugar de bloquear sitio por sitio, puede bloquear **categorías completas**:

1. **Navegar a:** Dashboard → **Categorías**
2. Seleccionar una categoría (ejemplo: "Redes Sociales")
3. Click en **"Activar Bloqueo"**
4. **Resultado:** Todos los sitios asociados a esa categoría se bloquearán automáticamente

### Asociar Sitios a Categorías

Al agregar un sitio bloqueado, seleccione la categoría correspondiente:

```
Dominio: instagram.com
Categoría: [Redes Sociales ▼]
```

Esto permite:
- Bloqueo/desbloqueo masivo por categoría
- Reportes organizados por tipo de contenido
- Configuración rápida de niveles de protección

### Crear Categoría Personalizada

1. **Navegar a:** Dashboard → **Categorías** → **Nueva Categoría**
2. **Completar:**
   ```
   Nombre: Sitios de Noticias
   Descripción: Periódicos y portales informativos
   Color: #FF9800 (naranja)
   ```
3. **Click en "Crear"**
4. Ahora puede asociar sitios a esta nueva categoría

---

## ⏰ Horarios de Bloqueo

### ¿Por Qué Usar Horarios?

Los horarios permiten **automatizar** el bloqueo según la hora del día o día de la semana:

- **Escuelas:** Bloquear redes sociales durante horas de clase
- **Hogares:** Bloquear juegos durante horario de estudio
- **Oficinas:** Bloquear streaming durante horario laboral

### Crear un Horario

1. **Navegar a:** Dashboard → **Horarios** → **Nuevo Horario**

2. **Configurar horario:**
   ```
   Nombre: Horas de Clase
   Días: Lunes, Martes, Miércoles, Jueves, Viernes
   Hora Inicio: 08:00
   Hora Fin: 15:00
   Categorías a Bloquear: Redes Sociales, Juegos, Streaming
   Activo: ☑ Sí
   ```

3. **Click en "Guardar"**

4. **Resultado:**
   - De lunes a viernes, de 8:00 AM a 3:00 PM
   - Las categorías seleccionadas estarán bloqueadas
   - Fuera de ese horario, el bloqueo se desactiva automáticamente

### Ejemplos de Horarios Comunes

#### Horario Escolar
```
Nombre: Jornada Escolar
Días: Lun-Vie
Horario: 07:30 - 15:30
Bloquear: Redes Sociales, Juegos, Streaming, Chat
```

#### Horario de Estudio en Casa
```
Nombre: Tiempo de Estudio
Días: Lun-Vie
Horario: 16:00 - 19:00
Bloquear: Juegos, Streaming, Redes Sociales
```

#### Fin de Semana Relajado
```
Nombre: Fin de Semana
Días: Sáb-Dom
Horario: Todo el día
Bloquear: Solo Adultos, Apuestas
Permitir: Redes Sociales, Juegos (con moderación)
```

### Múltiples Horarios

Puede crear **múltiples horarios** que se superponen:

- **Horario 1:** Bloqueo total durante clases (8:00-15:00)
- **Horario 2:** Bloqueo parcial en evenings (19:00-21:00)
- **Horario 3:** Sin restricciones fines de semana

El sistema combina todas las reglas activas.

### Desactivar Temporalmente un Horario

1. Ir a lista de horarios
2. Desmarcar checkbox **"Activo"**
3. Click en **"Guardar"**
4. El horario queda pausado hasta que lo reactive

---

## 📊 Registro de Actividad

### ¿Qué se Registra?

El sistema registra automáticamente:

- ✅ **Inicios de sesión** exitosos y fallidos
- ✅ **Cambios en configuración** (sitios agregados/eliminados)
- ✅ **Modificaciones al archivo hosts**
- ✅ **Copias de seguridad** realizadas
- ✅ **Errores del sistema**

### Ver el Registro de Actividad

1. **Navegar a:** Dashboard → **Registro de Actividad**

2. **Verá una tabla con:**
   ```
   Fecha/Hora          | Tipo          | Descripción                    | Usuario
   --------------------|---------------|--------------------------------|--------
   2026-04-24 14:30:15 | Login         | Inicio de sesión exitoso       | admin
   2026-04-24 14:32:10 | ConfigChange  | Sitio bloqueado agregado       | admin
   2026-04-24 14:32:11 | HostsUpdate   | Archivo hosts actualizado      | System
   2026-04-24 14:35:00 | Backup        | Backup de hosts creado         | System
   ```

### Filtrar el Registro

Use los filtros para encontrar eventos específicos:

- **Por fecha:** Seleccione rango de fechas
- **Por tipo:** Login, ConfigChange, HostsUpdate, Backup, Error
- **Por usuario:** admin, System, etc.
- **Por palabra clave:** Buscar en descripciones

### Exportar Registro

1. Click en botón **"Exportar"**
2. Seleccionar formato:
   - **CSV:** Para abrir en Excel
   - **TXT:** Archivo de texto plano
3. Elegir ubicación para guardar
4. Click en **"Descargar"**

### Importancia del Registro

El registro de actividad es crucial para:

- 🔍 **Auditoría:** Saber quién cambió qué y cuándo
- 🐛 **Troubleshooting:** Identificar causas de problemas
- 📈 **Reportes:** Generar informes de uso
- 🔒 **Seguridad:** Detectar intentos de acceso no autorizado

---

## ⚙️ Configuración del Sistema

### Configuración General

1. **Navegar a:** Dashboard → **Configuración**

2. **Opciones disponibles:**

#### Nombre de la Institución
```
Campo: Nombre de Institución
Valor: Colegio San Martín
```
- Aparece en reportes y encabezados
- Personaliza la experiencia

#### Modo de Operación
```
Opciones:
☉ Escuela  - Filtrado estricto, múltiples administradores
☉ Hogar    - Filtrado flexible, un administrador
```

#### Nivel de Protección
```
Opciones:
○ Bajo     - Solo adultos y apuestas
○ Medio    - + Redes sociales y juegos
○ Alto     - + Streaming y entretenimiento
○ Máximo   - Todo bloqueado excepto lista blanca
```

### Perfil de Administrador

1. **Navegar a:** Configuración → **Mi Perfil**

2. **Información editable:**
   - Nombre completo
   - Correo electrónico
   - Contraseña

3. **Cambiar contraseña:**
   ```
   Contraseña actual: [____________]
   Nueva contraseña:  [____________]
   Confirmar:         [____________]
   
   [Guardar Cambios]
   ```

**Requisitos de contraseña:**
- Mínimo 8 caracteres
- Al menos 1 mayúscula
- Al menos 1 minúscula
- Al menos 1 número
- Recomendado: 1 carácter especial (!@#$%^&*)

### Notificaciones (Futuro)

*Esta funcionalidad estará disponible en próximas versiones:*

- 📧 Email cuando se bloquee un sitio nuevo
- 📱 Notificación push para cambios críticos
- 📊 Reporte semanal de actividad

---

## 💾 Copias de Seguridad

### Copias de Seguridad Automáticas

El sistema crea backups automáticamente:

- **Antes de cada modificación** al archivo hosts
- **Ubicación:** `C:\Program Files\AulaSegura\Backups\hosts\`
- **Formato:** `hosts-backup-YYYYMMDD-HHMMSS.txt`
- **Retención:** Últimos 30 backups

### Crear Backup Manual

1. **Navegar a:** Dashboard → **Copias de Seguridad**
2. Click en **"Crear Backup Ahora"**
3. Esperar confirmación
4. Backup guardado en carpeta de backups

### Restaurar desde Backup

**⚠️ ADVERTENCIA:** Esto revertirá TODOS los cambios realizados al archivo hosts.

1. **Navegar a:** Dashboard → **Copias de Seguridad**
2. Seleccionar backup de la lista (por fecha/hora)
3. Click en **"Restaurar Selección"**
4. Confirmar acción
5. El sistema restaurará el archivo hosts al estado del backup
6. **Reiniciar el servicio** para aplicar cambios:
   ```powershell
   Restart-Service "AulaSeguraService"
   ```

### Ver Backups Existentes

1. Ir a sección de Copias de Seguridad
2. Verá lista con:
   - Fecha y hora del backup
   - Tamaño del archivo
   - Botones de acción (Restaurar, Descargar, Eliminar)

### Descargar Backup

Para respaldo externo:

1. Seleccionar backup
2. Click en **"Descargar"**
3. Guardar en USB, cloud storage, etc.

### Eliminar Backups Antiguos

1. Seleccionar backups antiguos
2. Click en **"Eliminar Seleccionados"**
3. Confirmar eliminación

**Recomendación:** Mantener al menos 5-10 backups recientes.

---

## ❓ Preguntas Frecuentes

### General

**P: ¿Puedo instalar AulaSegura en múltiples computadoras?**  
R: Sí, pero cada instalación es independiente. Necesita instalar el servicio y la aplicación en cada equipo.

**P: ¿Funciona con todos los navegadores?**  
R: Sí. Como el bloqueo es a nivel de DNS (archivo hosts), funciona con Chrome, Firefox, Edge, Safari, Opera, etc.

**P: ¿Los usuarios pueden evadir el bloqueo?**  
R: Es difícil. Requieren permisos de administrador para modificar el archivo hosts o cambiar la configuración DNS. En modo Escuela, esto está restringido.

---

### Bloqueo de Sitios

**P: Agregué un sitio a la lista de bloqueados, pero aún puedo acceder.**  
R: Espere hasta 60 segundos (ciclo de monitoreo). Si persiste:
1. Verifique que el sitio está marcado como "Activo"
2. Revise que no esté en la lista blanca
3. Reinicie el navegador
4. Verifique logs del servicio

**P: ¿Cómo bloqueo un sitio específico pero permito otro del mismo dominio?**  
R: Use la lista blanca. Ejemplo:
- Bloquear: `youtube.com`
- Permitir: `edu.youtube.com` (agregar a lista blanca)

**P: ¿El bloqueo funciona para aplicaciones, no solo navegadores?**  
R: Sí. Cualquier aplicación que use DNS para resolver dominios será afectada (apps de escritorio, juegos, etc.).

---

### Horarios

**P: Creé un horario pero no se aplica.**  
R: Verifique:
1. El horario está marcado como "Activo"
2. La hora actual está dentro del rango configurado
3. El día de la semana coincide
4. Las categorías están correctamente seleccionadas

**P: ¿Puedo tener horarios diferentes para días diferentes?**  
R: Sí. Cree múltiples horarios, uno para cada configuración.

---

### Seguridad

**P: Olvidé mi contraseña. ¿Cómo la recupero?**  
R: Contacte al administrador principal. Si usted es el único admin:
1. Detenga el servicio
2. Elimine el archivo `aulasegura.db`
3. Reinicie el servicio (recreará la base de datos con contraseña por defecto: `SeedAdmin__Password o first-run-admin.txt`)
4. **ADVERTENCIA:** Esto eliminará TODA la configuración

**P: ¿Es seguro cambiar manualmente el archivo hosts?**  
R: No lo recomendamos. Use siempre la aplicación AulaSegura para gestionar el archivo hosts. Los cambios manuales pueden ser sobrescritos por el servicio.

---

### Rendimiento

**P: ¿AulaSegura ralentiza mi computadora?**  
R: No. El servicio consume menos del 1% de CPU y 50 MB de RAM típicamente. El impacto es imperceptible.

**P: ¿El bloqueo afecta la velocidad de internet?**  
R: No. El bloqueo ocurre a nivel de DNS, antes de establecer conexión. Los sitios permitidos cargan a velocidad normal.

---

### Troubleshooting

**P: El servicio no inicia.**  
R: Verifique:
1. Tiene permisos de administrador
2. .NET 8 Runtime está instalado
3. Revise logs en `C:\Program Files\AulaSegura\Logs\`
4. Intente reiniciar el servicio: `Restart-Service "AulaSeguraService"`

**P: La aplicación no se conecta al servicio.**  
R: Ambos comparten la misma base de datos SQLite. Verifique:
1. El servicio está corriendo
2. El archivo `aulasegura.db` existe en `Data\`
3. No hay otra instancia de la aplicación abierta

**P: Recibo error de "permisos denegados" al modificar hosts.**  
R: El servicio debe ejecutarse como LocalSystem (configurado por defecto). Verifique:
```powershell
Get-Service "AulaSeguraService" | Select-Object *
```
Debe mostrar `LogOnAs = LocalSystem`

---

## 📞 Soporte

### Recursos de Ayuda

- **Documentación completa:** Carpeta `docs/` en la instalación
- **Logs del sistema:** `C:\Program Files\AulaSegura\Logs\`
- **Manual técnico:** `MANUAL_TECNICO.md` (para personal TI)
- **Guía de seguridad:** `SEGURIDAD.md`

### Contacto

Para soporte técnico avanzado, contacte al equipo de desarrollo con:
- Versión de AulaSegura instalada
- Versión de Windows
- Logs relevantes
- Descripción detallada del problema

---

## ✨ Consejos y Mejores Prácticas

### Para Escuelas

1. **Configure horarios escolares** para automatizar el bloqueo durante clases
2. **Cree categorías específicas** por edad/grado
3. **Mantenga una lista blanca** de sitios educativos aprobados
4. **Revise logs semanalmente** para identificar patrones de uso
5. **Capacite a profesores** sobre cómo solicitar excepciones

### Para Hogares

1. **Establezca reglas claras** con sus hijos sobre uso de internet
2. **Use horarios de estudio** para bloquear distracciones
3. **Permita acceso gradual** a medida que demuestran responsabilidad
4. **Revise reportes juntos** como oportunidad educativa
5. **Actualice contraseñas** regularmente

### Para Administradores TI

1. **Realice backups semanales** de la configuración
2. **Documente todos los cambios** en el registro de actividad
3. **Pruebe nuevas reglas** en un equipo antes de desplegar masivamente
4. **Mantenga el sistema actualizado** con las últimas versiones
5. **Monitoree logs** para detectar intentos de evasión

---

**Documento creado:** 24 de Abril, 2026  
**Versión:** 1.0.0  
**Próxima actualización:** Según feedback de usuarios
