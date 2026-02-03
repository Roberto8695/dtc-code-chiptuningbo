# 🚗 DtcDesk - Guía de Uso de la Interfaz

## 🎨 Diseño de la Aplicación

La interfaz está dividida en tres áreas principales:

### 📋 Panel Izquierdo - Entrada de Códigos
- **Área de texto grande**: Pega aquí los códigos DTC copiados de WinOLS o TXT
- **Botón "PROCESAR CÓDIGOS"**: Extrae y busca los códigos en la base de datos
- **Botón "Limpiar"**: Borra el texto y los resultados

### 📊 Panel Derecho - Resultados
- **DataGridView**: Muestra los códigos procesados con:
  - ✅ **Código**: El código DTC normalizado
  - 📝 **Descripción**: Descripción del error (si existe en BD)
  - 🏷️ **Categoría**: P (Powertrain), C (Chassis), B (Body), U (Network), Hex
  - ⚠️ **Estado**: "✓ Encontrado" o "⚠ No encontrado"

### 🎛️ Barra de Botones
- **Añadir**: Agrega un nuevo código a la base de datos
- **Editar**: Modifica un código existente (solo si está en BD)
- **Eliminar**: Borra un código de la base de datos
- **Exportar**: Guarda los resultados en TXT o CSV
- **Importar**: Carga códigos masivamente desde CSV

---

## 📖 Cómo Usar la Aplicación

### 1️⃣ Pegar y Procesar Códigos

1. Copia códigos desde WinOLS o cualquier texto
   ```
   Ejemplo:
   P0420 P0300 C073 B1234 U0100
   ```

2. Pega en el área de texto izquierda

3. Haz clic en **"PROCESAR CÓDIGOS"**

4. Los resultados aparecen en la tabla:
   - Códigos encontrados en BD → Verde/amarillo con descripción
   - Códigos no encontrados → Rojo con "⚠ No encontrado"

### 2️⃣ Añadir Código Nuevo

Si un código **no se encuentra en la BD**:

1. Selecciona el código en la tabla (o déjalo sin seleccionar)
2. Haz clic en **"Añadir"**
3. Completa el formulario:
   - **Código DTC** *(obligatorio)*: P0420
   - **Descripción** *(obligatorio)*: Eficiencia del catalizador...
   - **Categoría**: Powertrain / Chassis / Body / Network / Hex
   - **Fuente** *(opcional)*: OBD-II Standard, VAG, BMW, etc.
   - **Notas** *(opcional)*: Información adicional
4. Clic en **"GUARDAR"**

💡 **Atajo rápido**: Haz **doble clic** en un código no encontrado para abrirlo directamente en el formulario de añadir.

### 3️⃣ Editar Código Existente

1. Selecciona un código **encontrado** en la tabla
2. Haz clic en **"Editar"** (o doble clic en el código)
3. Modifica la descripción, categoría, fuente o notas
4. Guarda los cambios

### 4️⃣ Importar Códigos Masivamente

Para cargar tus **2000+ códigos DTC** desde CSV:

1. Prepara un archivo CSV con estas columnas:
   ```csv
   Code,Description,Category,Source,Notes
   P0420,Eficiencia del catalizador...,Powertrain,OBD-II Standard,Verificar catalizador
   P0300,Fallo de encendido aleatorio,Powertrain,OBD-II Standard,
   ```

2. Haz clic en **"Importar"**
3. Selecciona tu archivo CSV
4. Confirma la importación
5. ✅ Todos los códigos se cargan en segundos

📁 **Archivo de ejemplo**: `sample_dtc_codes.csv` (incluido en el proyecto)

### 5️⃣ Exportar Resultados

Guarda los códigos procesados:

1. Haz clic en **"Exportar"**
2. Elige opciones:
   - ☑️ Incluir descripción
   - ☑️ Incluir categoría y fuente
   - ☐ Solo códigos no encontrados
3. Selecciona formato:
   - **📄 TXT**: Texto legible
   - **📊 CSV**: Para Excel/importación

---

## 🎯 Características Especiales

### ✨ Parser Inteligente
- Extrae códigos de **cualquier formato** (horizontal, vertical, con ruido)
- Normaliza automáticamente (p0420 → P0420)
- Elimina duplicados manteniendo el orden
- Detecta códigos P/C/B/U y hexadecimales (FFFF)

### 🎨 Tema Oscuro Moderno
- Paleta de colores diseñada para reducir fatiga visual
- Amarillo de acento para elementos importantes
- Filas alternadas en la tabla para mejor lectura

### 💾 Base de Datos Local SQLite
- Ultra-rápida incluso con 10,000+ códigos
- Índices optimizados para búsqueda instantánea
- Ubicación: `%LocalAppData%\DtcDesk\dtc_codes.db`

---

## 🔑 Atajos de Teclado

| Acción | Atajo |
|--------|-------|
| Doble clic en código encontrado | Abre **Editar** |
| Doble clic en código no encontrado | Abre **Añadir** |
| Seleccionar + Enter | Abre detalles |

---

## 📝 Formato de Códigos DTC Soportados

| Formato | Ejemplo | Descripción |
|---------|---------|-------------|
| P-codes | P0420, P1234 | Powertrain (motor/transmisión) |
| C-codes | C0073, C1234 | Chassis (frenos/suspensión) |
| B-codes | B0001, B1234 | Body (carrocería/eléctricos) |
| U-codes | U0100, U1234 | Network (CAN/comunicación) |
| Hex 4 chars | FFFF, 1A2B, 00C8 | Códigos hexadecimales genéricos |

---

## 🚀 Flujo de Trabajo Recomendado

1. **Importa** tu diccionario completo de 2000+ códigos desde CSV
2. **Pega** códigos desde WinOLS cuando trabajes en una ECU
3. **Procesa** para ver descripciones automáticamente
4. **Añade** nuevos códigos que no existan en tu diccionario
5. **Exporta** el resultado final para documentación

---

## 💡 Consejos

- Los códigos **se guardan automáticamente** en la BD local
- Usa **Importar** para alimentar rápidamente la base de datos
- Los códigos se **normalizan automáticamente** a mayúsculas
- La **búsqueda es case-insensitive** (P0420 = p0420)
- Usa el **archivo CSV de ejemplo** como plantilla para tus importaciones

---

## 🎨 Paleta de Colores

| Elemento | Color |
|----------|-------|
| Fondo principal | #0F1E2B (Azul petróleo oscuro) |
| Panel lateral | #153C59 (Azul marca) |
| Acento amarillo | #F8B41C (Botones principales) |
| Verde OK | #5CB85C (Exportar, éxito) |
| Rojo alerta | #D9534F (Eliminar, errores) |

¡Disfruta usando DtcDesk! 🚗⚡
