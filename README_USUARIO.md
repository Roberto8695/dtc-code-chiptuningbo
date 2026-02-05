# DTC Desk - Diccionario de Códigos DTC

![Logo DTC Desk](DtcDesk.WinForms/logo-apk.ico)

**Versión:** 1.0  
**Desarrollado para:** ECU Tuning Services Bolivia  
**Plataforma:** Windows 10/11 (64-bit)

---

## 📋 Descripción

DTC Desk es una aplicación de escritorio diseñada para talleres mecánicos e ingenieros que trabajan con ECUs y WinOLS. Permite gestionar un diccionario inteligente de códigos DTC (Diagnostic Trouble Codes) con funcionalidades de búsqueda rápida, importación/exportación y gestión completa de la base de datos.

---

## 🚀 Instalación

### Requisitos
- Windows 10 o Windows 11 (64-bit)
- **NO requiere** tener .NET instalado (aplicación auto-contenida)

### Pasos de instalación
1. Copia el archivo `DTCDesk.exe` a cualquier carpeta de tu PC
2. (Opcional) Copia también `logo.jpg` en la misma carpeta para ver el logo de la empresa
3. Ejecuta `DTCDesk.exe`

La aplicación creará automáticamente una carpeta `Data` con la base de datos SQLite en el mismo directorio del ejecutable.

---

## 📖 Guía de Uso

### 1️⃣ **Importar Códigos desde CSV**

#### Preparar el archivo CSV

Tu archivo CSV debe tener **exactamente** este formato con 5 columnas:

```csv
Code,Description,Category,Source,Notes
P0500,Vehicle Speed Sensor Malfunction,Powertrain,OBD-II Standard,
P0104,Mass or Volume Air Flow Circuit Intermittent,Powertrain,OBD-II Standard,Check MAF sensor
U0001,High Speed CAN Communication Bus,Network,Manufacturer Specific,
```

**Columnas requeridas:**
- `Code`: Código DTC (P####, U####)
- `Description`: Descripción del código
- `Category`: Categoría (Powertrain, Network, u otra)
- `Source`: Fuente del código (OBD-II Standard, Manufacturer, etc.)
- `Notes`: Notas adicionales (puede estar vacío)

#### Importar el archivo

1. **Menú → Archivo → Importar CSV...**
2. Selecciona tu archivo `.csv`
3. La aplicación mostrará:
   - Cantidad de códigos a importar
   - Duplicados detectados (se omitirán)
4. Clic en **"Importar"**
5. Espera la confirmación de importación exitosa

**Consejos:**
- El archivo debe estar codificado en UTF-8
- Los códigos duplicados NO se importarán (protección automática)
- Puedes importar miles de códigos a la vez

---

### 2️⃣ **Buscar y Analizar Códigos**

#### Pegar códigos para análisis

1. **Copia códigos** desde WinOLS, scanner o cualquier fuente. Formato soportado:
   ```
   2122 2123 0510 01A6 0101 0108
   ```
   _(Códigos hexadecimales de 4 caracteres)_

2. **Pégalos** en el área de texto izquierda "PEGAR CÓDIGOS DTC AQUÍ"

3. **Clic en "PROCESAR CÓDIGOS"**

4. La aplicación buscará automáticamente en las categorías **P** (Powertrain) y **U** (Network):
   - `P2122`, `U2122`
   - `P2123`, `U2123`
   - `P0510`, `U0510`
   - etc.

#### Filtrar resultados

Usa el selector **"Categoría:"** en la parte superior derecha:

- **Automático**: Muestra solo códigos encontrados en la BD
- **P - Powertrain**: Muestra solo códigos P#### (encontrados y no encontrados)
- **U - Network**: Muestra solo códigos U#### (encontrados y no encontrados)

#### Interpretar resultados

**Tabla de resultados:**
| Columna | Descripción |
|---------|-------------|
| CÓDIGO | Código DTC completo (ej: P0500) |
| DESCRIPCIÓN | Descripción del problema |
| CATEGORÍA | Powertrain o Network |
| ESTADO | ✓ Encontrado / ⚠ No encontrado |

**Códigos no encontrados:**
- Aparecen como "--- Sin descripción ---"
- Estado: "⚠ No encontrado" (en rojo)
- Puedes añadirlos manualmente con el botón **"Añadir"**

---

### 3️⃣ **Añadir o Editar Códigos Manualmente**

#### Añadir nuevo código

1. Selecciona un código **"No encontrado"** en la tabla
2. Clic en **"Añadir"**
3. Completa el formulario:
   - **Código**: Ya viene pre-llenado
   - **Descripción**: Descripción del problema
   - **Categoría**: Selecciona Powertrain o Network
   - **Fuente**: Origen del código (OBD-II, Fabricante, etc.)
   - **Notas**: Información adicional (opcional)
4. Clic en **"Guardar"**

#### Editar código existente

1. Selecciona un código **"Encontrado"** en la tabla
2. Clic en **"Editar"** (o doble clic en la fila)
3. Modifica los campos necesarios
4. Clic en **"Guardar"**

**Atajo:** Doble clic en cualquier fila abre automáticamente el formulario de edición/añadir.

---

### 4️⃣ **Exportar Resultados**

#### Exportar códigos analizados

1. Procesa códigos y obtén resultados
2. **Menú → Archivo → Exportar...**
3. Selecciona el formato:
   - **TXT**: Texto plano, fácil de leer
   - **CSV**: Para Excel/importar a otros programas

4. Opciones de exportación:
   - ☑ **Solo códigos encontrados**: Exporta únicamente los que tienen descripción
   - ☑ **Incluir no encontrados**: Exporta todos, incluso sin descripción

5. Selecciona ubicación y nombre del archivo
6. Clic en **"Exportar"**

#### Formatos de salida

**Formato TXT:**
```
CÓDIGOS DTC - ANÁLISIS
Generado: 05/02/2026 13:30

P0500 - Powertrain
  Descripción: Vehicle Speed Sensor Malfunction
  Fuente: OBD-II Standard
  Estado: Encontrado

U0001 - Network
  Descripción: High Speed CAN Communication Bus
  Fuente: Manufacturer Specific
  Estado: Encontrado
```

**Formato CSV:**
```csv
Code,Description,Category,Source,Notes,Found
P0500,Vehicle Speed Sensor Malfunction,Powertrain,OBD-II Standard,,True
```

---

### 5️⃣ **Gestión de la Base de Datos**

#### Ver estadísticas

**Menú → Herramientas → Ver Estadísticas DB**

Muestra:
- Total de códigos en la base de datos
- Desglose por categoría (Powertrain, Network)
- Otros códigos

#### Limpiar base de datos

**⚠️ CUIDADO: Esta acción es IRREVERSIBLE**

1. **Menú → Archivo → Limpiar Base de Datos...**
2. Confirma la eliminación
3. Todos los códigos se eliminarán permanentemente
4. Usa antes de importar un nuevo conjunto de códigos

**Recomendación:** Exporta tu base de datos a CSV antes de limpiar, como respaldo.

---

## 🎯 Flujo de Trabajo Típico

### Escenario 1: Primera vez usando la aplicación

```mermaid
1. Abrir DTCDesk.exe
   ↓
2. Menú → Archivo → Importar CSV
   ↓
3. Seleccionar archivo con códigos (ej: codigos_DTC.csv)
   ↓
4. Esperar confirmación de importación
   ↓
5. ¡Listo para usar!
```

### Escenario 2: Analizar códigos de un vehículo

```mermaid
1. Copiar códigos hex desde scanner/WinOLS (ej: 0500 0104 2122)
   ↓
2. Pegar en área de texto
   ↓
3. Clic "PROCESAR CÓDIGOS"
   ↓
4. Filtrar por categoría (P o U) si es necesario
   ↓
5. Revisar descripciones
   ↓
6. (Opcional) Exportar reporte
```

### Escenario 3: Añadir códigos nuevos

```mermaid
1. Procesar códigos
   ↓
2. Identificar códigos "No encontrados"
   ↓
3. Seleccionar código → Clic "Añadir"
   ↓
4. Completar información
   ↓
5. Guardar
```

---

## ⌨️ Atajos de Teclado

| Atajo | Acción |
|-------|--------|
| `Ctrl + C` | Copiar códigos seleccionados de la tabla |
| `Doble clic` | Abrir formulario de edición/añadir |
| `Enter` | Procesar códigos (cuando el área de texto está enfocada) |

---

## 📊 Formato de Códigos Soportados

### Códigos de entrada (para pegar)
- ✅ Hexadecimal de 4 caracteres: `2122`, `0510`, `01A6`
- ✅ Múltiples códigos separados por espacio: `2122 0510 01A6`
- ✅ Con saltos de línea (vertical u horizontal)

### Categorías procesadas
- **P - Powertrain**: Códigos del tren motriz (motor, transmisión)
- **U - Network**: Códigos de comunicación de red (CAN, LIN)

**Nota:** Las categorías B (Body) y C (Chassis) NO se procesan en esta versión.

---

## 🗂️ Ubicación de Archivos

### Base de datos
```
[Carpeta_del_ejecutable]\Data\dtc_codes.db
```

La base de datos SQLite se crea automáticamente la primera vez que ejecutas la aplicación.

### Logo
```
[Carpeta_del_ejecutable]\logo.jpg
```

Si existe, se muestra en la esquina superior derecha.

---

## ❓ Solución de Problemas

### No se importan los códigos

**Problema:** Al importar CSV, dice "0 códigos importados"

**Soluciones:**
1. Verifica que el CSV tenga el formato correcto (5 columnas)
2. Asegúrate de que el archivo esté en UTF-8
3. Revisa que los códigos no estén ya en la base de datos (duplicados)

### No aparecen resultados al procesar

**Problema:** Pego códigos y no aparecen resultados

**Soluciones:**
1. Verifica que los códigos sean hexadecimales de 4 caracteres (0-9, A-F)
2. Cambia el filtro de categoría a "Automático" o "P - Powertrain"
3. Revisa que hayas importado códigos a la base de datos

### Error al exportar

**Problema:** El botón de exportar no funciona

**Soluciones:**
1. Primero debes procesar códigos (tener resultados en la tabla)
2. Verifica que tengas permisos de escritura en la carpeta de destino

---

## 📞 Soporte

Para soporte técnico o reportar problemas, contacta a:

**ECU Tuning Services Bolivia**  
📧 Email: [tu-email@ecutuning.com]  
📱 Teléfono: [tu-teléfono]

---

## 📝 Notas de Versión

### Versión 1.0 (Febrero 2026)
- ✅ Importación masiva de códigos CSV
- ✅ Búsqueda multi-categoría (P y U)
- ✅ Filtrado por categoría
- ✅ Añadir/Editar códigos manualmente
- ✅ Exportación a TXT y CSV
- ✅ Gestión completa de base de datos
- ✅ Interfaz dark theme profesional
- ✅ Base de datos SQLite local

---

## 🔒 Privacidad y Datos

- Todos los datos se almacenan **localmente** en tu PC
- No se envía información a servidores externos
- La base de datos es portable (puedes copiarla a otro equipo)
- Puedes hacer respaldo de la carpeta `Data` para guardar tus códigos

---

**Desarrollado con ❤️ para ECU Tuning Services Bolivia**
