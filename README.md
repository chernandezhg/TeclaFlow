# TeclaFlow

TeclaFlow es una aplicación de escritorio para Windows que introduce texto carácter por carácter en la ventana activa. Está pensada como herramienta local de accesibilidad y productividad para entornos donde la automatización esté permitida.

## Uso

1. Abre TeclaFlow y completa el tutorial inicial.
2. Coloca el contenido en **Texto a escribir**.
3. Selecciona un intervalo fijo y una cuenta regresiva.
4. Pulsa **Comenzar escritura**.
5. Mientras TeclaFlow está minimizado, una banda flotante mostrará la cuenta regresiva. Haz clic en el punto exacto de la aplicación destino donde debe aparecer el primer carácter.

Atajos disponibles durante la ejecución:

- **F7:** pausar o continuar.
- **F8:** detener y volver a TeclaFlow.

## Ejecutar desde el código

```powershell
dotnet run --project .\TeclaFlow\TeclaFlow.csproj
```

## Compilar

```powershell
dotnet build .\TeclaFlow.slnx --configuration Release
```

El proyecto utiliza WPF y .NET 9 para Windows, sin paquetes externos. El contenido no se envía a servidores y el portapapeles no se utiliza durante la escritura.

## Instalador para Windows

La forma recomendada de instalar TeclaFlow es descargar `TeclaFlow-Setup-1.0.0.exe` desde la sección **Releases** de GitHub. El asistente instala la aplicación para el usuario actual, permite crear un acceso directo y añade un desinstalador estándar de Windows.

El código del instalador y sus recursos visuales están en la carpeta `installer`.

## Consideraciones

- La aplicación destino debe admitir entrada de teclado Unicode.
- Una aplicación abierta como administrador puede rechazar entradas de una aplicación sin elevar.
- No utilices TeclaFlow para incumplir políticas de exámenes, instituciones, empresas o servicios.
