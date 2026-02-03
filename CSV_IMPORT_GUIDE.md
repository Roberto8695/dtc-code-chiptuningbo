# 📋 Guía para Crear el CSV de Importación de Códigos DTC

## 🎯 Estructura del Archivo CSV

Tu archivo CSV debe tener **exactamente estas columnas** (en este orden):

```csv
Code,Description,Category,Source,Notes
```

### 📊 Descripción de Columnas

| Columna | Obligatorio | Tipo | Descripción | Ejemplo |
|---------|-------------|------|-------------|---------|
| **Code** | ✅ SÍ | Texto | Código DTC (P/C/B/U + 4 dígitos o 4 hex) | `P0420` |
| **Description** | ✅ SÍ | Texto | Descripción del error/falla | `Eficiencia del catalizador por debajo del umbral` |
| **Category** | ❌ No | Texto | Categoría del código | `Powertrain` |
| **Source** | ❌ No | Texto | Fuente/fabricante | `OBD-II Standard`, `VAG Group` |
| **Notes** | ❌ No | Texto | Notas adicionales/soluciones | `Verificar convertidor catalítico` |

---

## 📝 Reglas Importantes

### ✅ Formato del Código (Code)

**Formatos válidos:**
- `P0420` - Powertrain (letra P + 4 dígitos)
- `C0073` - Chassis (letra C + 4 dígitos)  
- `B1234` - Body (letra B + 4 dígitos)
- `U0100` - Network (letra U + 4 dígitos)
- `FFFF` - Hexadecimal (4 caracteres hex: 0-9, A-F)
- `00C8` - Hexadecimal con ceros

**NO válidos:**
- ❌ `P042` (solo 3 dígitos)
- ❌ `P04200` (5 dígitos)
- ❌ `X1234` (letra inválida)

### ✅ Formato de la Descripción (Description)

- **Obligatoria** para todos los códigos
- Puede contener comas, puntos, guiones, etc.
- Si contiene comas, debe ir entre comillas: `"Sensor MAF, circuito bajo"`
- Longitud recomendada: 20-200 caracteres

### ✅ Categorías Recomendadas (Category)

| Valor | Descripción |
|-------|-------------|
| `Powertrain` | Motor, transmisión, control de emisiones |
| `Chassis` | Frenos, suspensión, dirección, ABS |
| `Body` | Carrocería, airbags, iluminación, confort |
| `Network` | Comunicación CAN, módulos |
| `Hex` | Códigos hexadecimales genéricos |
| `Otro` | Otros tipos |

**Nota:** Si dejas vacío, la app detecta automáticamente por la letra inicial (P/C/B/U).

### ✅ Fuentes Comunes (Source)

Ejemplos de valores útiles:
- `OBD-II Standard` - Códigos estándar OBD-II
- `VAG Group` - Volkswagen, Audi, Seat, Skoda
- `BMW` - Códigos específicos BMW
- `Mercedes-Benz` - Códigos Mercedes
- `Ford` - Códigos Ford
- `General Motors` - GM
- `Toyota` - Toyota/Lexus
- `Nissan` - Nissan/Infiniti
- `Manufacturer Specific` - Específicos del fabricante

---

## 📄 Plantilla del Archivo CSV

### Opción 1: Plantilla Básica (solo obligatorios)

```csv
Code,Description,Category,Source,Notes
P0420,Eficiencia del catalizador por debajo del umbral,,,
P0300,Fallo de encendido aleatorio detectado,,,
C0073,Sensor de velocidad de rueda delantero izquierdo,,,
```

### Opción 2: Plantilla Completa (con todos los campos)

```csv
Code,Description,Category,Source,Notes
P0420,Eficiencia del catalizador por debajo del umbral - Banco 1,Powertrain,OBD-II Standard,Verificar convertidor catalítico y sondas lambda
P0300,Fallo de encendido aleatorio detectado,Powertrain,OBD-II Standard,Revisar bujías y bobinas de encendido
C0073,Sensor de velocidad de rueda delantero izquierdo - Señal inválida,Chassis,VAG Group,Verificar sensor ABS y cableado
B1234,Fallo en el circuito del airbag del conductor,Body,OBD-II Standard,Verificar conectores y resistencia del airbag
U0100,Comunicación perdida con ECM/PCM,Network,OBD-II Standard,Revisar bus CAN y conectividad
```

### Opción 3: Con Comas en Descripción (usar comillas)

```csv
Code,Description,Category,Source,Notes
P0171,"Sistema demasiado pobre, Banco 1",Powertrain,OBD-II Standard,"Verificar fugas de vacío, MAF sucio"
P0172,"Sistema demasiado rico, Banco 1",Powertrain,OBD-II Standard,"Verificar inyectores, presión de combustible"
```

---

## 🛠️ Cómo Crear tu CSV

### Método 1: Usando Excel

1. **Abre Excel** y crea una nueva hoja
2. **Escribe los encabezados** en la primera fila:
   ```
   A1: Code
   B1: Description  
   C1: Category
   D1: Source
   E1: Notes
   ```
3. **Llena los datos** fila por fila:
   ```
   A2: P0420
   B2: Eficiencia del catalizador por debajo del umbral
   C2: Powertrain
   D2: OBD-II Standard
   E2: Verificar catalizador
   ```
4. **Guarda como CSV**:
   - Archivo → Guardar como
   - Tipo: `CSV UTF-8 (delimitado por comas) (*.csv)`
   - Nombre: `mis_codigos_dtc.csv`

### Método 2: Usando Google Sheets

1. **Crea una nueva hoja** en Google Sheets
2. **Estructura igual** que en Excel (columnas A-E)
3. **Llena tus datos**
4. **Exporta**:
   - Archivo → Descargar → Valores separados por comas (.csv)

### Método 3: Usando Notepad++ o VS Code

1. **Crea un archivo** `codigos.csv`
2. **Copia la plantilla** de arriba
3. **Añade tus códigos** línea por línea
4. **Guarda con codificación UTF-8**

---

## 📋 Ejemplo Completo de CSV Real

```csv
Code,Description,Category,Source,Notes
P0100,Circuito del sensor de flujo de aire masivo (MAF),Powertrain,OBD-II Standard,Verificar MAF y cableado
P0101,Sensor MAF - Rango/rendimiento del circuito,Powertrain,OBD-II Standard,Limpiar o reemplazar MAF
P0102,Sensor MAF - Entrada de circuito baja,Powertrain,OBD-II Standard,Revisar voltaje de alimentación
P0103,Sensor MAF - Entrada de circuito alta,Powertrain,OBD-II Standard,Cortocircuito a positivo
P0104,Sensor MAF - Circuito intermitente,Powertrain,OBD-II Standard,Verificar conectores
P0105,Presión absoluta del colector (MAP) - Mal funcionamiento del circuito,Powertrain,OBD-II Standard,
P0106,Sensor MAP - Rango/rendimiento del circuito,Powertrain,OBD-II Standard,
P0107,Sensor MAP - Entrada de circuito baja,Powertrain,OBD-II Standard,
P0108,Sensor MAP - Entrada de circuito alta,Powertrain,OBD-II Standard,
P0109,Sensor MAP - Circuito intermitente,Powertrain,OBD-II Standard,
P0110,Sensor de temperatura del aire de admisión - Mal funcionamiento,Powertrain,OBD-II Standard,
P0171,Sistema demasiado pobre - Banco 1,Powertrain,OBD-II Standard,Fugas de vacío o MAF sucio
P0172,Sistema demasiado rico - Banco 1,Powertrain,OBD-II Standard,Inyectores con fuga
P0420,Eficiencia del catalizador - Banco 1,Powertrain,OBD-II Standard,Catalizador deteriorado
P0430,Eficiencia del catalizador - Banco 2,Powertrain,OBD-II Standard,Catalizador deteriorado
C0035,Velocidad de rueda delantera izquierda - Señal incorrecta,Chassis,VAG Group,Sensor ABS defectuoso
C0040,Velocidad de rueda delantera derecha - Señal incorrecta,Chassis,VAG Group,Sensor ABS defectuoso
C0045,Velocidad de rueda trasera izquierda - Señal incorrecta,Chassis,VAG Group,Sensor ABS defectuoso
C0050,Velocidad de rueda trasera derecha - Señal incorrecta,Chassis,VAG Group,Sensor ABS defectuoso
B0001,Resistencia del airbag del conductor - Circuito abierto,Body,OBD-II Standard,
U0100,Comunicación perdida con ECM/PCM,Network,OBD-II Standard,Bus CAN interrumpido
U0101,Comunicación perdida con TCM,Network,OBD-II Standard,Bus CAN interrumpido
FFFF,Código de prueba hexadecimal,Hex,,Código de ejemplo
00C8,Código hexadecimal 200 decimal,Hex,,Ejemplo numérico
```

---

## ✨ Consejos y Buenas Prácticas

### 1. 🎯 Organización

- **Ordena alfabéticamente** por código para facilitar la búsqueda
- **Agrupa por categoría** (todos los P juntos, luego C, B, U)
- **Numera los códigos** en secuencia si son de una misma serie

### 2. 📝 Descripciones

- **Sé específico** pero conciso (50-150 caracteres ideal)
- **Incluye "Banco 1" o "Banco 2"** cuando aplique
- **Usa verbos de acción**: "Verificar", "Revisar", "Reemplazar"
- **Evita abreviaturas** poco claras

### 3. 🔍 Categorización

- **Usa categorías consistentes** (siempre "Powertrain", no "Motor")
- Si no estás seguro, **déjalo vacío** (la app detecta automáticamente)

### 4. 📌 Notas Útiles

Incluye en Notes:
- ✅ Soluciones comunes
- ✅ Componentes a verificar
- ✅ Procedimientos de diagnóstico
- ✅ Costos estimados de reparación
- ✅ Tiempo de mano de obra

Ejemplo:
```csv
P0420,Eficiencia del catalizador,Powertrain,OBD-II Standard,"Verificar: 1) Sondas lambda 2) Catalizador 3) Fugas de escape. Costo: 300-800€"
```

### 5. ⚠️ Caracteres Especiales

Si tu descripción contiene:
- **Comas**: Usa comillas → `"Sensor MAF, circuito bajo"`
- **Comillas**: Duplica las comillas → `"Error ""crítico"" detectado"`
- **Saltos de línea**: Evítalos o usa comillas

---

## 🚀 Proceso de Importación

### Paso a Paso:

1. **Crea tu CSV** siguiendo esta guía
2. **Guarda el archivo** (ej: `mis_2000_codigos.csv`)
3. **Abre DtcDesk**
4. Haz clic en **"Importar"**
5. Selecciona tu archivo CSV
6. **Confirma** la importación
7. ✅ Verás: `"Importación completada. Total procesados: 2000"`

### ⏱️ Tiempos Estimados:

- 100 códigos → ~1 segundo
- 1,000 códigos → ~5 segundos
- 2,000 códigos → ~10 segundos
- 10,000 códigos → ~30 segundos

---

## 🔧 Solución de Problemas

### ❌ Error: "No se encontraron códigos válidos"

**Causa**: Columnas mal nombradas o faltantes

**Solución**: 
- Verifica que la primera fila sea exactamente: `Code,Description,Category,Source,Notes`
- Asegúrate que Code y Description no estén vacíos

### ❌ Error: "Formato de código inválido"

**Causa**: Código no sigue el formato P####, C####, etc.

**Solución**:
- Verifica que sea P/C/B/U + 4 dígitos
- O 4 caracteres hexadecimales (0-9, A-F)

### ❌ Error: "Descripción vacía"

**Causa**: Falta la descripción (obligatoria)

**Solución**:
- Añade una descripción en la columna Description
- Mínimo: poner "Sin descripción" si no la tienes

### ⚠️ Advertencia: "X códigos duplicados"

**Causa**: El mismo código aparece varias veces

**Solución**:
- La app usa `INSERT OR IGNORE` (solo inserta el primero)
- Limpia duplicados en Excel antes de importar

---

## 📥 Plantilla Descargable

Usa el archivo incluido como plantilla:

📁 **[sample_dtc_codes.csv](sample_dtc_codes.csv)** - 14 códigos de ejemplo

Puedes:
1. Abrirlo en Excel/Google Sheets
2. Borrar los ejemplos
3. Llenar con tus códigos
4. Guardar y listo para importar

---

## 🎓 Ejemplo Práctico: Creando tu Primer CSV

### Escenario: Tienes estos códigos en un TXT

```
P0420 - Cat efficiency
P0300 - Random misfire
C0073 - ABS sensor FL
```

### Paso 1: Convierte a CSV

```csv
Code,Description,Category,Source,Notes
P0420,Eficiencia del catalizador,Powertrain,OBD-II Standard,Verificar catalizador
P0300,Fallo de encendido aleatorio,Powertrain,OBD-II Standard,Revisar bujías
C0073,Sensor ABS delantero izquierdo,Chassis,VAG Group,Verificar sensor y cableado
```

### Paso 2: Guarda como `mis_codigos.csv`

### Paso 3: Importa en DtcDesk

✅ ¡Listo! Tus 3 códigos están en la base de datos.

---

## 💡 Tips Finales

1. **Empieza pequeño**: Importa 50-100 códigos primero para probar
2. **Haz backups**: Guarda copias de tu CSV antes de importar
3. **Actualiza progresivamente**: Puedes importar múltiples veces
4. **Usa Excel**: Es más fácil que editar texto plano
5. **Documenta bien**: Incluye notas útiles para ti mismo

---

## 📞 ¿Necesitas Ayuda?

Si tienes problemas:
1. Revisa que el CSV tenga las 5 columnas exactas
2. Verifica que Code y Description no estén vacíos
3. Asegúrate que el archivo esté en UTF-8
4. Prueba con el [sample_dtc_codes.csv](sample_dtc_codes.csv) incluido

---

¡Buena suerte llenando tu diccionario de códigos DTC! 🚗⚡
