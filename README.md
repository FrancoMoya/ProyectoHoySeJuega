Para probarlo:

EJECUTAR toda la query DB

(Menos lo del UPDATE usuario, que es para evitar la verificación de correo luego de registrarsr)

EJECUTAR los STORED PROCEDURES de SPs(1 por 1)

Hacer los 4 SETEOS desde POWERSHELL moviendose a la carpeta del proyecto

-- EMAIL

// seteo de contraseña de aplicacion

// dotnet user-secrets set "smtp:Password" "dzur havz dgxu vtgd"

// dotnet user-secrets set "smtp:Password" "ibgx iwca lltn tduv"


-- GOOGLE

// Setear los secrets para posteriormente usarlos en el builder (De lo posible hacerlo en la termianal de powershell )

// dotnet user-secrets set "Authentication:Google:ClientId" "925616110531-4n85qcuq48afgvi06sdp0qos60g2q8ph.apps.googleusercontent.com"

// dotnet user-secrets set "Authentication:Google:ClientSecret" "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"
