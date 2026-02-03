Descripción general de la aplicación

Estás desarrollando una aplicación de escritorio para Windows orientada a talleres/ingenieros que trabajan con ECUs y WinOLS, cuya función principal es actuar como un diccionario inteligente de códigos DTC.

La app permite al usuario pegar una lista de códigos de error (copiados desde WinOLS o desde un TXT), y el sistema los ordena, limpia y traduce automáticamente mostrando la descripción de la falla al lado de cada código. Además, permite gestionar y ampliar la base de datos de códigos para que el diccionario crezca con el tiempo.

Funciones principales (MVP)

Pegado inteligente de códigos

El usuario pega códigos en formato horizontal o vertical.

La app detecta y extrae automáticamente los códigos válidos (ej. hex de 4 caracteres o P-codes).

Normaliza formato (mayúsculas, elimina basura/espacios).

Listado ordenado + visualización

Los códigos se muestran en una tabla (DataGridView) en forma vertical.

Se indica si el código existe en el diccionario o si no se encontró.

Búsqueda en diccionario (lookup)

Para cada código, la app consulta una base local SQLite.

Muestra:

Código

Descripción

(opcional) categoría / fuente / notas

Marcado / estado de códigos (opcional según lo que cierre el cliente)

El usuario puede seleccionar códigos y marcarlos con un “estado” (por ejemplo, 0/F o “filtrar/ignorar”).

Esto se usa para clasificar y exportar listas (sin tocar la ECU).

Exportación

Exporta resultados como:

TXT (solo códigos)

CSV/TXT (código + descripción)

CSV/TXT (código + estado)

Gestión de diccionario

Agregar/editar/eliminar códigos y sus descripciones.

Importar códigos masivamente desde CSV/Excel (para alimentar rápido la base).

Lógica interna (cómo trabaja el programa)

El flujo típico es:

Entrada

Usuario pega texto desde WinOLS / TXT.

Parser

Se separa el texto en tokens.

Se detectan códigos válidos (patrones).

Se normalizan (ej. c073 → C073).

Se elimina ruido (comas, tabs, espacios extra).

(opcional) se eliminan duplicados manteniendo el orden.

Lookup

Por cada código, se consulta el diccionario en SQLite.

Si existe → se devuelve descripción.

Si no existe → se marca “No encontrado”.

Presentación

Se carga la tabla con filas:

Código | Descripción | Estado | Fuente

Salida

Exportar / guardar selección / importar nuevas entradas.

Tecnologías usadas
Lenguaje y plataforma

C# (.NET 8)

Windows Forms (UI)

Base de datos

SQLite (local, offline, muy rápida)

Paquete: Microsoft.Data.Sqlite

Índices para búsqueda rápida por código

Importación / exportación

CsvHelper para CSV (manejo robusto)

(opcional) pandas no aplica aquí porque estás en C#

Distribución

dotnet publish para generar .exe

Opcional: publicación self-contained para que funcione sin instalar .NET

Estructura del proyecto (por capas)

DtcDesk.WinForms: interfaz (formularios, grillas, botones)

DtcDesk.Core: lógica (parser, modelos, servicios)

DtcDesk.Data: base de datos (repositorios SQLite, inicialización)

Esto mantiene el proyecto:

ordenado

escalable

fácil de mantener y mejorar

---

## 🚀 Guías de Uso

📖 **[UI_GUIDE.md](UI_GUIDE.md)** - Guía completa de uso de la interfaz

📋 **[CSV_IMPORT_GUIDE.md](CSV_IMPORT_GUIDE.md)** - Cómo crear el CSV para importar tus códigos

🔧 **[PARSER_EXAMPLES.md](PARSER_EXAMPLES.md)** - Ejemplos del parser de códigos

🎨 **[design.md](design.md)** - Paleta de colores de la aplicación

---

## 📁 Archivos Importantes

- **sample_dtc_codes.csv** - 14 códigos de ejemplo para probar importación
- **extended_dtc_examples.csv** - 100+ códigos DTC reales listos para importar
- **template_import.csv** - Plantilla vacía para crear tu propio CSV

---

## ⚡ Inicio Rápido

### 1. Ejecutar la aplicación

```bash
dotnet run --project DtcDesk.WinForms/DtcDesk.WinForms.csproj
```

### 2. Importar códigos de ejemplo

1. Abre la aplicación
2. Haz clic en "Importar"
3. Selecciona `extended_dtc_examples.csv`
4. Confirma → 100+ códigos cargados instantáneamente

### 3. Probar el parser

1. Pega códigos en el área izquierda: `P0420 P0300 C073`
2. Haz clic en "PROCESAR CÓDIGOS"
3. Ve los resultados con descripciones automáticas

### 4. Añadir códigos manualmente

1. Selecciona un código "No encontrado"
2. Haz clic en "Añadir"
3. Completa la descripción
4. Guarda → Ya está en tu diccionario

---

## 📊 Base de Datos

**Ubicación**: `%LocalAppData%\DtcDesk\dtc_codes.db`

**Estructura**:
```sql
DtcCodes (
    Id INTEGER PRIMARY KEY,
    Code TEXT UNIQUE,
    Description TEXT NOT NULL,
    Category TEXT,
    Source TEXT,
    Notes TEXT,
    CreatedAt TEXT,
    UpdatedAt TEXT,
    IsActive INTEGER
)
```

**Índices optimizados** para búsquedas rápidas con 10,000+ códigos