using System.Net;
using System.Text;

namespace AulaSegura.Service;

/// <summary>
/// Servidor HTTP embebido que sirve la página de bloqueo
/// Escucha en http://127.0.0.1:8080/ y redirige todas las solicitudes a la página de bloqueo
/// </summary>
public class BlockingPageServer : IDisposable
{
    private HttpListener? _listener;
    private readonly string _blockingPagePath;
    private readonly ILogger<BlockingPageServer> _logger;
    private readonly int _port;
    private bool _isRunning;

    public BlockingPageServer(
        ILogger<BlockingPageServer> logger,
        string blockingPagePath,
        int port = 8080)
    {
        _logger = logger;
        _blockingPagePath = blockingPagePath;
        _port = port;
    }

    /// <summary>
    /// Inicia el servidor HTTP de bloqueo
    /// </summary>
    public async Task StartAsync()
    {
        if (_isRunning)
        {
            _logger.LogWarning("El servidor de bloqueo ya está en ejecución");
            return;
        }

        try
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://127.0.0.1:{_port}/");
            _listener.Start();
            _isRunning = true;

            _logger.LogInformation("Servidor de página de bloqueo iniciado en http://127.0.0.1:{Port}", _port);

            // Iniciar loop de aceptación de conexiones en background
            _ = Task.Run(() => AcceptConnectionsAsync());
        }
        catch (HttpListenerException ex) when (ex.ErrorCode == 5) // Access Denied
        {
            _logger.LogError(ex, 
                "Error de permisos al iniciar servidor de bloqueo. " +
                "El servicio debe ejecutarse con privilegios administrativos.");
            throw new UnauthorizedAccessException(
                "Se requieren privilegios de administrador para iniciar el servidor de bloqueo", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al iniciar servidor de bloqueo");
            throw;
        }
    }

    /// <summary>
    /// Detiene el servidor HTTP de bloqueo
    /// </summary>
    public void Stop()
    {
        if (!_isRunning)
            return;

        try
        {
            _listener?.Stop();
            _listener?.Close();
            _isRunning = false;
            _logger.LogInformation("Servidor de página de bloqueo detenido");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al detener servidor de bloqueo");
        }
    }

    /// <summary>
    /// Loop principal que acepta conexiones entrantes
    /// </summary>
    private async Task AcceptConnectionsAsync()
    {
        while (_isRunning && _listener != null)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequestAsync(context));
            }
            catch (ObjectDisposedException)
            {
                // El listener fue cerrado, salir del loop
                break;
            }
            catch (Exception ex)
            {
                if (_isRunning) // Solo loguear si no estamos apagando
                {
                    _logger.LogError(ex, "Error al aceptar conexión");
                }
            }
        }
    }

    /// <summary>
    /// Maneja una solicitud HTTP individual
    /// </summary>
    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;
            var response = context.Response;

            // Obtener el dominio solicitado desde el header Host o URL
            var requestedDomain = request.Headers["Host"] ?? "sitio-desconocido";
            
            // Leer la página HTML de bloqueo
            var htmlContent = await LoadBlockingPageAsync(requestedDomain);

            // Configurar respuesta HTTP
            response.StatusCode = 200;
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = Encoding.UTF8.GetByteCount(htmlContent);

            // Agregar headers de seguridad
            response.Headers.Add("X-Frame-Options", "DENY");
            response.Headers.Add("X-Content-Type-Options", "nosniff");
            response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");

            // Escribir respuesta
            using var output = response.OutputStream;
            var buffer = Encoding.UTF8.GetBytes(htmlContent);
            await output.WriteAsync(buffer, 0, buffer.Length);

            _logger.LogDebug("Página de bloqueo servida para: {Domain}", requestedDomain);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al manejar solicitud de bloqueo");
            
            // Enviar respuesta de error genérica
            try
            {
                var response = context.Response;
                response.StatusCode = 500;
                response.ContentType = "text/plain";
                
                using var output = response.OutputStream;
                var errorBytes = Encoding.UTF8.GetBytes("Error interno del servidor de bloqueo");
                await output.WriteAsync(errorBytes, 0, errorBytes.Length);
            }
            catch
            {
                // Ignorar errores al enviar respuesta de error
            }
        }
    }

    /// <summary>
    /// Carga la página HTML de bloqueo y reemplaza los placeholders
    /// </summary>
    private async Task<string> LoadBlockingPageAsync(string domain)
    {
        try
        {
            if (!File.Exists(_blockingPagePath))
            {
                _logger.LogWarning("Archivo de página de bloqueo no encontrado: {Path}", _blockingPagePath);
                return GenerateSimpleBlockingPage(domain);
            }

            var htmlTemplate = await File.ReadAllTextAsync(_blockingPagePath);
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Reemplazar placeholders
            var safeDomain = WebUtility.HtmlEncode(domain);

            var html = htmlTemplate
                .Replace("{{DOMAIN}}", safeDomain)
                .Replace("{{CATEGORY}}", "Categoría Restringida")
                .Replace("{{TIMESTAMP}}", timestamp)
                .Replace("{{RULE}}", "Lista Negra / Categoría Bloqueada")
                .Replace("{{ADMIN_CONTACT}}", "Contacte al administrador del sistema")
                .Replace("{{FULL_TIMESTAMP}}", timestamp);

            return html;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar página de bloqueo, usando fallback");
            return GenerateSimpleBlockingPage(domain);
        }
    }

    /// <summary>
    /// Genera una página de bloqueo simple como fallback si el template no está disponible
    /// </summary>
    private string GenerateSimpleBlockingPage(string domain)
    {
        var safeDomain = WebUtility.HtmlEncode(domain);

        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <title>Sitio Bloqueado</title>
    <style>
        body {{ font-family: Arial, sans-serif; text-align: center; padding: 50px; background: #f5f5f5; }}
        .container {{ background: white; padding: 40px; border-radius: 10px; max-width: 600px; margin: 0 auto; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        h1 {{ color: #d32f2f; }}
        p {{ color: #666; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <h1>🚫 Sitio Bloqueado</h1>
        <p>El acceso a <strong>{safeDomain}</strong> ha sido restringido.</p>
        <p>Este sitio está bloqueado según las políticas de seguridad configuradas.</p>
        <p style=""margin-top: 30px; color: #999; font-size: 12px;"">
            Protegido por AulaSegura Control Web<br>
            Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
        </p>
    </div>
</body>
</html>";
    }

    public void Dispose()
    {
        Stop();
        // HttpListener no implementa IDisposable en todas las versiones de .NET
        // Solo cerramos y liberamos recursos manualmente
        _listener = null;
    }
}
