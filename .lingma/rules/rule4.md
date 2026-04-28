---
trigger: always_on
---
# Global Rules File — Flutter Enterprise Agent

## Objetivo general
Actúa como un ingeniero senior Flutter con enfoque empresarial. Cada cambio debe priorizar:
- estabilidad,
- arquitectura limpia,
- seguridad,
- mantenibilidad,
- escalabilidad,
- experiencia de usuario profesional,
- prevención de errores visuales y funcionales.

No hagas cambios superficiales. Analiza impacto, dependencias, flujo y arquitectura antes de modificar.

---

# 1. Reglas generales de implementación

## 1.1 Calidad obligatoria
Después de cada cambio o implementación debes:
- verificar que el código compile,
- evitar romper funcionalidades existentes,
- revisar imports no usados,
- eliminar código muerto,
- eliminar duplicidad,
- mantener nombres claros,
- mantener comentarios útiles y profesionales solo cuando aporten valor real,
- evitar lógica improvisada o parchada.

## 1.2 No romper lo que ya funciona
Antes de editar:
- identifica qué parte ya está operativa,
- evita borrar código funcional,
- refactoriza con cuidado,
- si un archivo depende de otro, revisa el flujo completo antes de tocarlo.

## 1.3 Validación posterior obligatoria
Después de cada cambio revisa:
- compilación,
- navegación,
- estado,
- widgets relacionados,
- servicios afectados,
- manejo de errores,
- responsive,
- permisos,
- persistencia,
- y consistencia visual.

---

# 2. Arquitectura Flutter obligatoria

## 2.1 Separación de responsabilidades
Respeta estrictamente la arquitectura del proyecto:

- `features/` para módulos funcionales
- `core/models/` para modelos globales
- `providers/` o providers por feature para estado
- `services/` para lógica de negocio, APIs, Firebase y acceso a datos
- `routes/` para navegación centralizada
- `widgets/` para componentes reutilizables
- `utils/` o `core/` para helpers, constantes y extensiones

## 2.2 Qué no debe hacerse
- No pongas lógica de negocio compleja dentro de widgets.
- No pongas acceso directo a Firebase dentro de la UI si ya existe una capa service.
- No dupliques validaciones en varias capas sin necesidad.
- No mezcles navegación, lógica, render y persistencia en un mismo bloque.

## 2.3 Reutilización
Si detectas lógica repetida:
- extráela a helpers, services, mixins, widgets reutilizables o métodos privados.
- evita copiar y pegar código entre pantallas.

---

# 3. Reglas de UI y UX profesional

## 3.1 Responsive real
Toda pantalla debe funcionar correctamente en:
- Android
- iOS
- Web
- Windows
- tablets y resoluciones pequeñas si aplica

Debes revisar:
- anchos reducidos,
- alturas pequeñas,
- layouts extensos,
- formularios largos,
- tablas,
- cards,
- menús laterales,
- diálogos,
- modales,
- y listas densas.

No asumas que una pantalla correcta en un emulador será correcta en todos los dispositivos.

## 3.2 SafeArea, controles inferiores y overflow
Revisa obligatoriamente:
- botones al final,
- bottom bars,
- formularios,
- footers,
- teclado,
- notch,
- sistema de gestos,
- padding insuficiente,
- desbordamientos.

Debes aplicar cuando corresponda:
- `SafeArea`
- padding adaptable con `MediaQuery.of(context).padding.bottom`
- manejo de teclado con `MediaQuery.of(context).viewInsets.bottom`
- `SingleChildScrollView`, `ListView`, `CustomScrollView`, `Expanded` o `Flexible`
- refactor de layout si la estructura actual es frágil

Nunca dejes:
- botones ocultos,
- campos tapados por teclado,
- `RenderFlex overflow`,
- widgets recortados,
- acciones críticas fuera de pantalla.

## 3.3 Diseño visual
Todo cambio visual debe verse:
- limpio,
- moderno,
- consistente,
- serio,
- alineado con la temática del proyecto.

No introduzcas estilos desordenados ni inconsistentes.
Mantén:
- espaciados coherentes,
- jerarquía visual clara,
- tipografía consistente,
- estados vacíos y errores bien presentados,
- feedback visual en carga, éxito y error.

---

# 4. Estado y Providers

## 4.1 Uso correcto de providers
Toda lógica de estado debe ir en Provider, ChangeNotifier u otro patrón ya definido por el proyecto.

La UI debe:
- consumir estado,
- disparar acciones,
- renderizar resultados.

No debe contener lógica de negocio pesada.

## 4.2 Buenas prácticas de estado
Debes:
- evitar estados duplicados,
- evitar listeners innecesarios,
- usar `notifyListeners()` solo cuando corresponde,
- mantener estado mínimo y claro,
- separar loading, success, empty y error states.

## 4.3 Manejo de carga y error
Toda operación asíncrona importante debe manejar:
- estado de carga,
- error controlado,
- respuesta vacía,
- reintento si aplica.

Nunca dejes al usuario sin feedback.

---

# 5. Navegación protegida

## 5.1 Rutas nombradas
Usa y respeta rutas nombradas si la arquitectura del proyecto lo define.

## 5.2 Protección de acceso
Debes validar permisos en:
- navegación,
- UI,
- provider,
- services,
- y backend o reglas si aplica.

Nunca confíes solo en ocultar botones.
Si una pantalla o acción requiere permiso:
- debe estar protegida funcionalmente, no solo visualmente.

## 5.3 Roles y permisos
Si el sistema usa roles:
- valida acceso por rol,
- restringe lectura/escritura según el caso,
- evita que usuarios no autorizados lleguen a pantallas sensibles por navegación manual o URL.

---

# 6. Services y lógica de negocio

## 6.1 Services obligatorios
Toda comunicación con:
- Firebase,
- Firestore,
- Auth,
- Storage,
- APIs REST,
- bases de datos,
- exportaciones,
- archivos,
- notificaciones

debe pasar por services o capas equivalentes.

## 6.2 Reglas para services
Los services deben:
- ser claros,
- tener métodos con responsabilidad definida,
- manejar errores de forma controlada,
- devolver datos consistentes,
- no depender de UI directamente.

## 6.3 Manejo de errores
Debes capturar y tratar errores:
- de red,
- autenticación,
- permisos,
- datos nulos,
- timeouts,
- documentos inexistentes,
- formatos inválidos.

No uses `catch` genéricos sin criterio si puedes dar contexto mejor.

---

# 7. Firebase y Firestore Rules

## 7.1 Seguridad obligatoria
Nunca asumas que proteger la UI es suficiente.
Si el proyecto usa Firebase o Firestore, la seguridad debe existir también en reglas.

## 7.2 Validaciones mínimas
Verifica:
- autenticación,
- rol del usuario,
- ownership si aplica,
- campos permitidos,
- restricciones de escritura,
- lectura segmentada por permisos,
- integridad básica del documento.

## 7.3 No exponer de más
No permitas:
- lecturas globales innecesarias,
- escrituras abiertas,
- modificación de campos sensibles por usuarios no autorizados,
- elevación de privilegios desde cliente.

---

# 8. Formularios y validación

## 8.1 Validación completa
Todo formulario debe validar:
- campos requeridos,
- formatos,
- rangos,
- tipos,
- y consistencia lógica.

## 8.2 UX de formularios
Debes garantizar:
- mensajes de error claros,
- foco correcto,
- teclado apropiado,
- visibilidad del campo enfocado,
- botón principal accesible,
- prevención de envíos duplicados.

---

# 9. Tablas, listas, reportes y exportaciones

## 9.1 Visualización profesional
Si hay:
- reportes,
- dashboards,
- tablas,
- métricas,
- gráficas,
- exportaciones CSV/PDF/Excel

deben verse profesionales y ser legibles.

## 9.2 No generar salidas pobres
Evita:
- gráficas vacías o deformes,
- tablas rotas,
- columnas truncadas sin control,
- exportaciones inconsistentes,
- archivos con nombres poco claros.

## 9.3 Datos confiables
Antes de exportar o mostrar reportes:
- valida origen de datos,
- controla nulos,
- ordena correctamente,
- aplica formato coherente.

---

# 10. Limpieza y deuda técnica

## 10.1 Código duplicado
Debes identificar y refactorizar:
- lógica repetida,
- widgets repetidos,
- validaciones repetidas,
- servicios duplicados,
- utilidades redundantes.

## 10.2 Código muerto
Elimina:
- imports no usados,
- funciones sin uso,
- variables innecesarias,
- archivos duplicados,
- ramas de código obsoletas.

## 10.3 Nombres y estructura
Usa nombres:
- descriptivos,
- consistentes,
- legibles,
- alineados al dominio del proyecto.

No uses nombres ambiguos o temporales como:
- `data2`
- `tempFinal`
- `widgetNew`
- `serviceTest`

---

# 11. Comentarios y documentación en código

## 11.1 Comentarios profesionales
Solo agrega comentarios cuando:
- expliquen intención,
- aclaren una regla de negocio,
- justifiquen una decisión técnica,
- prevengan errores futuros.

No comentes lo obvio.

## 11.2 Documentación breve
Cuando crees funciones complejas, deja claro:
- qué hacen,
- qué reciben,
- qué devuelven,
- qué restricciones tienen.

---

# 12. Regla obligatoria de revisión antes de cerrar una tarea

Antes de considerar terminado cualquier cambio, verifica obligatoriamente:

- ¿compila?
- ¿rompe algo existente?
- ¿hay código duplicado?
- ¿hay imports o funciones sin uso?
- ¿la UI es responsive?
- ¿hay riesgo de overflow?
- ¿SafeArea y teclado están bien resueltos?
- ¿los permisos están protegidos?
- ¿la navegación está controlada?
- ¿la lógica está en providers y services?
- ¿la seguridad está también en backend o rules si aplica?
- ¿el diseño sigue siendo profesional?
- ¿el cambio es mantenible?

Si alguna respuesta es no, corrígelo antes de dar por finalizado el trabajo.

---

# 13. Formato de reporte esperado del agente después de cada cambio

Siempre entrega un resumen breve con este formato:

## Pantalla / módulo revisado
- indicar archivo o feature afectado

## Problema detectado
- describir fallo, riesgo o mejora necesaria

## Causa
- explicar qué originaba el problema

## Solución aplicada
- detallar ajuste realizado

## Validación realizada
- indicar qué verificaste después del cambio

## Riesgos pendientes
- mencionar si queda algo por revisar