# 🚀 Opciones de Despliegue - TPI Sistema de Mensajería

## 📊 Comparación Rápida

| Opción | Costo | Complejidad | Tiempo Setup | Mejor Para |
|--------|-------|-------------|--------------|------------|
| **Híbrido** (Recomendado) | 💰 Gratis | ⭐⭐ Fácil | 15 min | Facultad/Demo |
| **Render Full** | 💰💰 Paid | ⭐⭐⭐ Media | 20 min | Producción |
| **Local Only** | 💰 Gratis | ⭐ Muy Fácil | 2 min | Desarrollo |

---

## 🎯 Opción 1: Despliegue Híbrido (RECOMENDADO)

### 💡 Concepto
- **Backend**: Corre en tu PC (gratis, tus datos)
- **Frontend**: Desplegado en Vercel (gratis, ilimitado, profesional)
- **Túnel**: ngrok expone tu PC (gratis para demos)

### ✅ Ventajas
- ✅ 100% gratis
- ✅ URL pública profesional
- ✅ HTTPS automático
- ✅ Tus datos en tu PC
- ✅ Perfecto para proyectos de facultad

### ⚠️ Desventajas
- Tu PC debe estar encendida durante demos
- URLs de ngrok cambian cada vez (a menos que pagues)

### 📖 Guía
Lee **[QUICK_START_HYBRID.md](QUICK_START_HYBRID.md)** - Paso a paso en español

**Resumen ultra-rápido:**
```powershell
# 1. Instalar ngrok (una vez)
choco install ngrok

# 2. Configurar (una vez)
ngrok config add-authtoken TU_TOKEN

# 3. Iniciar backend (cada demo)
.\start-local-backend.ps1

# 4. Iniciar túnel (cada demo)
ngrok start --all --config ngrok.yml

# 5. Actualizar config.js con URLs de ngrok

# 6. Desplegar frontend (una vez)
vercel --prod

# ¡Listo! Comparte la URL de Vercel
```

---

## 🌐 Opción 2: Render.com (Todo en la Nube)

### 💡 Concepto
- Todo desplegado en Render.com (4 servicios + 3 bases de datos)
- Configuración automática con `render.yaml`

### ✅ Ventajas
- No necesitas tu PC encendida
- URLs fijas
- Más "profesional"

### ⚠️ Desventajas
- 💰 Bases de datos expiran en 90 días (free tier)
- 💰 Servicios se duermen después de 15 min
- Tardan ~30-60s en despertar

### 📖 Guía
Lee **[RENDER_DEPLOYMENT.md](RENDER_DEPLOYMENT.md)** - Guía completa

**Resumen:**
```powershell
# 1. Ve a dashboard.render.com
# 2. New + → Blueprint
# 3. Selecciona tu repo TPI-.NET
# 4. Apply
# 5. Configura JWT Secret manualmente
# Espera 15-20 minutos
```

---

## 🏠 Opción 3: Solo Local (Desarrollo)

### 💡 Concepto
Todo corre en tu PC, sin despliegue

### ✅ Ventajas
- Súper rápido
- No requiere configuración extra
- Perfecto para desarrollo

### ⚠️ Desventajas
- No hay URL pública
- Solo tú puedes acceder

### 📖 Uso
```powershell
# Iniciar backend
.\start-local-backend.ps1

# Iniciar frontend
cd src/UI
dotnet run

# Abre http://localhost:8080
```

---

## 🤔 ¿Cuál elegir?

### Para tu TPI de facultad:
👉 **Opción 1: Híbrido** - Frontend en Vercel + Backend en tu PC con ngrok

**¿Por qué?**
- Es gratis al 100%
- Tienes una URL profesional para compartir
- No gastas en hosting
- Funciona perfecto para demos y presentaciones
- Tus datos están seguros en tu PC

### Para un proyecto real en producción:
👉 **Opción 2: Render Full** - Todo en la nube

**¿Por qué?**
- Está siempre disponible
- No depende de tu PC
- URLs fijas
- Más confiable para uso continuo

### Solo para desarrollo/testing:
👉 **Opción 3: Local Only**

---

## 📚 Archivos de Documentación

- **[QUICK_START_HYBRID.md](QUICK_START_HYBRID.md)** - Paso a paso híbrido (RECOMENDADO)
- **[HYBRID_DEPLOYMENT.md](HYBRID_DEPLOYMENT.md)** - Detalles técnicos del despliegue híbrido
- **[RENDER_DEPLOYMENT.md](RENDER_DEPLOYMENT.md)** - Guía completa para Render.com
- **[RENDER_SETUP_SUMMARY.md](RENDER_SETUP_SUMMARY.md)** - Resumen de configuración Render

## 🛠️ Scripts Disponibles

- **`start-local-backend.ps1`** - Inicia los 3 servicios backend
- **`prepare-render.ps1`** - Verifica preparación para Render
- **`ngrok.yml`** - Configuración de túneles ngrok

## 🆘 Ayuda

Si tienes problemas:
1. Revisa la guía correspondiente (QUICK_START_HYBRID.md o RENDER_DEPLOYMENT.md)
2. Busca en la sección "Solución de Problemas" de cada guía
3. Verifica que PostgreSQL esté corriendo
4. Revisa los logs de cada servicio

---

## 💡 Recomendación Final

Para tu proyecto de facultad (TPI), usa el **despliegue híbrido**:

1. Sigue **[QUICK_START_HYBRID.md](QUICK_START_HYBRID.md)**
2. Solo necesitas ~15 minutos de setup inicial
3. Antes de cada demo/presentación:
   - Inicia backend: `.\start-local-backend.ps1`
   - Inicia ngrok: `ngrok start --all`
   - Actualiza URLs en `config.js`
   - Despliega: `vercel --prod`
4. Comparte la URL de Vercel

**Costo total: $0** 💰

**Tiempo de setup: 15 minutos** ⏱️

**Resultado: URL profesional funcionando** ✨

---

**¡Éxito con tu TPI!** 🎓
