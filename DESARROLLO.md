# Desarrollo - Corrección de Botones Editar/Eliminar en Vistas CRUD

## Objetivo General
Corregir los botones Editar y Eliminar no funcionales en las vistas de gestión KeywordsView y BlockingRulesView del proyecto AulaSegura WPF.

## Estado Actual del Sistema
- Los botones Editar y Eliminar en KeywordsView y BlockingRulesView no responden al hacer clic
- Los bindings XAML parecen correctos usando RelativeSource
- Los comandos están inicializados en los ViewModels
- El problema está en la lógica CanExecute de los comandos Update

## Archivos Revisados
- src/AulaSegura.App/ViewModels/KeywordsViewModel.cs
- src/AulaSegura.App/ViewModels/BlockingRulesViewModel.cs
- src/AulaSegura.App/Views/KeywordsView.xaml
- src/AulaSegura.App/Views/BlockingRulesView.xaml
- src/AulaSegura.App/ViewModels/CategoriesViewModel.cs (referencia)
- src/AulaSegura.App/ViewModels/SchedulesViewModel.cs (referencia)
- src/AulaSegura.App/ViewModels/AllowedSitesViewModel.cs (referencia)
- src/AulaSegura.App/ViewModels/BlockedSitesViewModel.cs (referencia)

## Problema Identificado

### KeywordsViewModel
El comando `UpdateKeywordCommand` tiene una condición `CanExecute`:
```csharp
UpdateKeywordCommand = new AsyncRelayCommand(UpdateKeywordAsync, () => SelectedKeyword != null);
```

Pero esta condición NO se notifica cuando cambia `SelectedKeyword`, por lo tanto el botón permanece deshabilitado incluso después de seleccionar una palabra clave para editar.

### BlockingRulesViewModel  
El mismo problema ocurre con `UpdateRuleCommand`:
```csharp
UpdateRuleCommand = new AsyncRelayCommand(UpdateRuleAsync, () => SelectedRule != null);
```

## Solución Propuesta

Agregar llamadas a `NotifyCanExecuteChanged()` en los partial methods que se ejecutan cuando cambian las propiedades `SelectedKeyword` y `SelectedRule`.

Esto asegurará que los comandos se reevalúen automáticamente cuando el usuario haga clic en el botón Editar.

## Fases de Desarrollo

### Fase 1: Análisis ✅
- Revisión completa de todos los ViewModels CRUD
- Identificación del patrón problemático
- Comparación con otros ViewModels que funcionan correctamente

### Fase 2: Implementación
- Agregar partial methods OnSelectedKeywordChanged en KeywordsViewModel
- Agregar partial methods OnSelectedRuleChanged en BlockingRulesViewModel
- Llamar NotifyCanExecuteChanged() en ambos casos

### Fase 3: Validación
- Verificar compilación
- Probar funcionalidad Edit/Delete en runtime
- Confirmar actualización de UI después de operaciones

## Avances Realizados
- [x] Análisis completado
- [ ] Implementación pendiente
- [ ] Pruebas pendientes

## Decisiones Técnicas
1. Se mantendrá el patrón inline de edición (sin modales) para Keywords y BlockingRules
2. Se usará el mecanismo NotifyCanExecuteChanged() de CommunityToolkit.Mvvm
3. No se modificarán las otras vistas que ya funcionan correctamente con modales

## Pendientes
- Implementar correcciones en KeywordsViewModel
- Implementar correcciones en BlockingRulesViewModel
- Probar en runtime
- Documentar guía de prueba
