---
trigger: always_on
---
aviso obligatorio de finalización y guía de prueba

Cada vez que completes un proceso, una funcionalidad, una corrección, una pantalla, un módulo o una integración, y el resultado ya esté listo para probarse, debes avisarlo de forma obligatoria al usuario.

No cierres una respuesta solo diciendo que ya terminaste. Debes indicar claramente que el proceso ya puede probarse y explicar exactamente qué debe hacer el usuario para validarlo.

Formato obligatorio al terminar cada proceso listo para prueba:

## Estado
Debes indicar una de estas opciones:
- Listo para probar
- Listo parcialmente para probar
- Requiere un paso previo antes de probar

## Qué se terminó
Explica en pocas líneas qué parte quedó implementada, corregida o conectada.

## Qué debe probar el usuario
Entrega una lista clara, ordenada y práctica de pasos exactos para la prueba.
Debes indicar:
- dónde entrar
- qué botón tocar
- qué dato ingresar
- qué acción ejecutar
- qué resultado debería ver
- qué comportamiento confirmaría que sí funciona

## Resultado esperado
Explica qué debe suceder si todo está correcto.

## Qué revisar si falla
Indica los puntos más probables a revisar si la prueba no sale bien.

## Siguiente paso recomendado
Después de la prueba, indica cuál sería el siguiente paso lógico del desarrollo.

Reglas obligatorias:
1. Siempre avisa cuando algo ya esté listo para probarse.
2. Nunca des por terminado un proceso sin indicar cómo probarlo.
3. Nunca asumas que el usuario sabrá qué validar por su cuenta.
4. Da pasos de prueba concretos, no explicaciones generales.
5. El lenguaje debe ser claro, práctico y directo.
6. Si algo no quedó completamente listo, dilo con honestidad y especifica qué sí puede probarse y qué no.
7. Si hay prerequisitos para probar, debes indicarlos antes de la guía de prueba.
8. Cuando aplique, incluye también el resultado visual esperado en pantalla.
9. Si el proceso tiene impacto en login, navegación, base de datos, Firebase, reportes, exportación, permisos o UI, menciona también qué parte exacta validar.
10. Siempre termina indicando si puedes continuar con el siguiente paso después de esa prueba.

Ejemplo de salida esperada del agente:

Estado:
Listo para probar.

Qué se terminó:
Se corrigió la pantalla de login y se conectó correctamente con Firebase Auth.

Qué debe probar el usuario:
1. Abre la aplicación.
2. Ingresa un correo válido registrado.
3. Ingresa la contraseña correcta.
4. Presiona el botón Iniciar sesión.
5. Verifica que el sistema te redirija al dashboard.
6. Cierra sesión y vuelve a entrar con una contraseña incorrecta para validar el mensaje de error.

Resultado esperado:
Con credenciales correctas debe abrir el dashboard. Con credenciales incorrectas debe mostrarse un mensaje de error sin cerrar la app ni romper la interfaz.

Qué revisar si falla:
- configuración de Firebase
- usuario no registrado
- error en la validación del formulario
- problema en la navegación al dashboard

Siguiente paso recomendado:
Después de validar el login, continuar con recuperación de contraseña o control de roles.