# Ejemplos de prueba para el DtcParser

## Ejemplo 1: Códigos horizontales (como mencionaste)
```
Entrada: C073 P0420 B1234 U0100 P0300 C073
Salida (vertical):
1. C073
2. P0420
3. B1234
4. U0100
5. P0300
(C073 duplicado eliminado)
```

## Ejemplo 2: Códigos hexadecimales
```
Entrada: 0001 0002 FFFF 1A2B 0001
Salida (vertical):
1. 0001
2. 0002
3. FFFF
4. 1A2B
(0001 duplicado eliminado)
```

## Ejemplo 3: Mezcla con ruido (como desde WinOLS)
```
Entrada: "Error codes: P0420, P0300, C073 - B1234;U0100"
Salida (vertical):
1. P0420
2. P0300
3. C073
4. B1234
5. U0100
```

## Ejemplo 4: Texto multilínea
```
Entrada:
P0420 P0300
C073 B1234
U0100

Salida (orden de izquierda a derecha, arriba a abajo):
1. P0420
2. P0300
3. C073
4. B1234
5. U0100
```

## Ejemplo 5: Códigos en minúsculas (se normalizan)
```
Entrada: p0420 c073 b1234
Salida:
1. P0420
2. C073
3. B1234
```

## Categorías detectadas automáticamente:
- **P** → Powertrain (motor/transmisión)
- **C** → Chassis (frenos/suspensión)
- **B** → Body (carrocería/eléctrica)
- **U** → Network (comunicación CAN)
- **Hex** → Códigos hexadecimales (4 caracteres)

## Formatos válidos:
✅ P0420 (letra + 4 dígitos)
✅ C073
✅ B1234
✅ U0100
✅ FFFF (4 caracteres hexadecimales)
✅ 0001

❌ P042 (solo 3 dígitos)
❌ P04200 (5 dígitos)
❌ XYZ123 (letra inválida)
