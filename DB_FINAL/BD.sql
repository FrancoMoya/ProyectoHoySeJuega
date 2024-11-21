CREATE DATABASE HOY_SE_JUEGA;
GO
USE HOY_SE_JUEGA;
GO

CREATE TABLE TITULO_REPORTE (
    ID_titulo_reporte INT IDENTITY(1,1) PRIMARY KEY,
    Titulo_Reporte NVARCHAR(80) NOT NULL
);

CREATE TABLE REPORTE (
    ID_reporte INT IDENTITY(1,1) PRIMARY KEY,
    Mes_Reporte TINYINT NOT NULL,
    Anio_Reporte SMALLINT NOT NULL,
    Usuarios_Registrados_Reporte INT NULL,
    Reservas_Realizadas_Reporte INT NULL,
    Fecha_De_Reporte DATETIME NULL DEFAULT GETDATE(),
	Descripcion_Reporte VARCHAR(1000) NOT NULL, --Ajuste
	ID_titulo_reporte INT NULL,
    FOREIGN KEY (ID_titulo_reporte) REFERENCES TITULO_REPORTE(ID_titulo_reporte) ON DELETE SET NULL
);

CREATE TABLE ROL (
    ID_rol INT IDENTITY(1,1) PRIMARY KEY,
    Nombre_Rol NVARCHAR(50) NOT NULL
);

CREATE TABLE USUARIO ( 
    ID_usuario INT IDENTITY(1,1) PRIMARY KEY,
    Nombre_Usuario NVARCHAR(30) NOT NULL,
    Apellido_Usuario NVARCHAR(30) NOT NULL,
    Correo_Usuario VARCHAR(100) UNIQUE NOT NULL,
    Contrasenia_Usuario NVARCHAR(255) NOT NULL,
    Telefono_Usuario INT NOT NULL,
	Activo BIT DEFAULT 1,
	PasswordResetToken NVARCHAR(255) NULL, --Ajuste
    PasswordResetTokenExpiry DATETIME NULL,
    EmailConfirmationToken NVARCHAR(255) NULL,
    EmailConfirmed BIT NULL,
	Ultima_Sesion DATE NULL DEFAULT GETDATE(), --Nuevo campo
    ID_rol INT NULL,
	Fecha_Registro DATE NULL DEFAULT GETDATE(), --Nuevo campo
    FOREIGN KEY (ID_rol) REFERENCES ROL(ID_rol) ON DELETE SET NULL -- El valor de ID_rol se establece a NULL si se elimina el rol
);

CREATE TABLE PERMISO (
    ID_permiso INT IDENTITY(1,1) PRIMARY KEY,
    Nombre_Permiso NVARCHAR(50) NOT NULL 
);

CREATE TABLE PERMISO_ROL (
    ID_permiso INT,
    ID_rol INT,
    FOREIGN KEY (ID_permiso) REFERENCES PERMISO(ID_permiso) ON DELETE CASCADE,
    FOREIGN KEY (ID_rol) REFERENCES ROL(ID_rol) ON DELETE CASCADE, -- Elimina la relación si se elimina un rol
    PRIMARY KEY (ID_rol, ID_permiso)
);

CREATE TABLE ACCION_REALIZADA (
    ID_accion_realizada INT IDENTITY(1,1) PRIMARY KEY,
    Titulo_Accion_Realizada NVARCHAR(70) NOT NULL
);

CREATE TABLE AUDITORIA (
    ID_auditoria INT IDENTITY(1,1) PRIMARY KEY,
    ID_usuario INT NULL, -- Permitir NULL en caso de que no haya o no se pueda agregar un usuario
    Fecha_Auditoria DATETIME NOT NULL DEFAULT GETDATE(),
	Seccion VARCHAR(50) NOT NULL, -- Para poder listar por tablas modificadas
    Descripcion_De_Accion NVARCHAR(1000) NOT NULL, --Ajuste
	ID_accion_realizada INT NULL,
    FOREIGN KEY (ID_accion_realizada) REFERENCES ACCION_REALIZADA(ID_accion_realizada) ON DELETE SET NULL,
);

CREATE TABLE CANCHA (
    ID_cancha INT IDENTITY(1,1) PRIMARY KEY,
    Nombre_Cancha NVARCHAR(70) NOT NULL,
    Ubicacion_Cancha NVARCHAR(120) NOT NULL
);

CREATE TABLE HORARIO_DISPONIBLE (
    ID_horario_disponible INT IDENTITY(1,1) PRIMARY KEY,
    ID_cancha INT NOT NULL,
    Fecha_Horario DATE NOT NULL DEFAULT GETDATE(),
    Hora_Inicio TIME NOT NULL,
    Hora_Fin TIME NOT NULL,
    Disponible_Horario BIT DEFAULT 1,
    FOREIGN KEY (ID_cancha) REFERENCES CANCHA(ID_cancha) ON DELETE CASCADE
);

CREATE TABLE ESTADO_RESERVA (
    ID_estado_reserva INT IDENTITY(1,1) PRIMARY KEY,
    Nombre_Estado_Reserva VARCHAR(25) NOT NULL
);

CREATE TABLE RESERVA (
    ID_reserva INT IDENTITY(1,1) PRIMARY KEY,
    ID_usuario INT NULL,
    ID_horario_disponible INT NULL,
    Fecha_Reserva DATETIME NULL DEFAULT GETDATE(),
    ID_estado_reserva INT NULL,
    FOREIGN KEY (ID_estado_reserva) REFERENCES ESTADO_RESERVA(ID_estado_reserva) ON DELETE SET NULL,
    FOREIGN KEY (ID_usuario) REFERENCES USUARIO(ID_usuario) ON DELETE SET NULL,
    FOREIGN KEY (ID_horario_disponible) REFERENCES HORARIO_DISPONIBLE(ID_horario_disponible) ON DELETE SET NULL
);

CREATE TABLE PAGO (
    ID_pago INT IDENTITY(1,1) PRIMARY KEY,
    ID_reserva INT NULL,
    Monto_Pago DECIMAL(10, 2) NOT NULL,
    Fecha_Pago DATETIME NULL DEFAULT GETDATE(),
    FOREIGN KEY (ID_reserva) REFERENCES RESERVA(ID_reserva)ON DELETE SET NULL
);

CREATE TABLE TITULO_NOTIFICACION (
    ID_titulo_notificacion INT IDENTITY(1,1) PRIMARY KEY,
    Titulo_Notificacion NVARCHAR(80) NOT NULL
);

CREATE TABLE NOTIFICACION (
    ID_notificacion INT IDENTITY(1,1) PRIMARY KEY,
    ID_usuario INT NOT NULL,
    ID_reserva INT NULL,
    Mensaje_Notificacion NVARCHAR(1000) NOT NULL, --ajuste
    Fecha_Envio_Notificacion DATETIME NOT NULL DEFAULT GETDATE(),
    ID_titulo_notificacion INT NULL,
    FOREIGN KEY (ID_titulo_notificacion) REFERENCES TITULO_NOTIFICACION(ID_titulo_notificacion) ON DELETE SET NULL,
    FOREIGN KEY (ID_usuario) REFERENCES USUARIO(ID_usuario) ON DELETE CASCADE,
    FOREIGN KEY (ID_reserva) REFERENCES RESERVA(ID_reserva) ON DELETE CASCADE
);

--NUEVA TABLA
CREATE TABLE EVENTO (
    ID_evento INT IDENTITY(1,1) PRIMARY KEY,
    Nombre_Evento NVARCHAR(80) NOT NULL DEFAULT 'CUMPLEAÑOS', -- Nombre o título del evento
    Descripcion_Evento NVARCHAR(1000) NULL, -- Detalles o notas adicionales sobre el evento y la persona que reserva
	Correo_Cliente_Evento VARCHAR(100) NULL,  -- Correo para luego hacer una busqueda y conectarlo a su usuario si existe
	Telefono_Cliente_Evento INT NULL,  -- Telefono para luego hacer una busqueda y conectarlo a su usuario si existe
	Fecha_Evento DATETIME NULL DEFAULT GETDATE(),
    ID_horario_disponible INT NULL,
    ID_estado_reserva INT NULL,
    FOREIGN KEY (ID_estado_reserva) REFERENCES ESTADO_RESERVA(ID_estado_reserva) ON DELETE SET NULL,
    FOREIGN KEY (ID_horario_disponible) REFERENCES HORARIO_DISPONIBLE(ID_horario_disponible) ON DELETE SET NULL
);

--NUEVA TABLA
CREATE TABLE CONFIGURACION_PAGO (
    ID_Configuracion INT IDENTITY(1,1) PRIMARY KEY,
    Monto_Sena DECIMAL(10, 2) NOT NULL, 
    Fecha_Modificacion DATETIME NOT NULL DEFAULT GETDATE(),
	CelularCancelaciones INT NULL -- NUEVO CAMPO
);

--NUEVAS TABLAS PRUEBAS(en BD en nube)
CREATE TABLE EVENTO_RECURRENTE (
    ID_evento_recurrente INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(80) NOT NULL,
    Descripcion NVARCHAR(1000) NULL,
	CorreoCliente VARCHAR(100) NULL,
	TelefonoCliente VARCHAR(15) NULL,
    HoraInicio TIME NOT NULL, -- La hora en que comienza el evento
    DiaSemana INT NOT NULL,  -- El día de la semana (1 = lunes, 7 = domingo)
    FechaInicio DATE NOT NULL, -- Fecha de inicio (puede ser el primer jueves del mes)
    FechaFin DATE NOT NULL, -- Fecha de fin (por ejemplo, último jueves del mes)
);


--EJEMPLOS: 

--Aún sin implementar
--INSERT INTO TITULO_REPORTE (Titulo_Reporte) VALUES 
--('Reporte Mensual de Usuarios'),
--('Reporte Mensual de Reservas');
--INSERT INTO REPORTE (Mes_Reporte, Anio_Reporte, Usuarios_Registrados_Reporte, Reservas_Realizadas_Reporte, Descripcion_Reporte, ID_titulo_reporte) VALUES 
--(10, 2024, 100, 200, 'Informe sobre el crecimiento de usuarios y reservas en octubre de 2024.', 1),
--(9, 2024, 150, 180, 'Informe sobre el rendimiento de reservas en septiembre de 2024.', 2);

INSERT INTO ROL (Nombre_Rol) VALUES 
('CLIENTE'), 
('ADMIN'), 
('EMPLEADO'); 
INSERT INTO PERMISO (Nombre_Permiso) VALUES 
('Crear reservas'),
('Administrar horarios'),
('Administrar reservas'),
('Administrar eventos'),
('Administrar usuarios'),
('Administrar roles'),
('Ver estadísticas'),
('Administrar monto de pago'),
('Ver auditoría');
-- Asignar permisos a Administrador
INSERT INTO PERMISO_ROL (ID_rol, ID_permiso) VALUES 
(2, 1), 
(2, 2), 
(2, 3), 
(2, 4),
(2, 5), 
(2, 6), 
(2, 7),
(2, 8), 
(2, 9); 
-- Asignar permisos a Empleado
INSERT INTO PERMISO_ROL (ID_rol, ID_permiso) VALUES 
(3, 1), 
(3, 2),
(3, 3), 
(3, 4); 
-- Asignar permisos a Cliente
INSERT INTO PERMISO_ROL (ID_rol, ID_permiso) VALUES 
(1, 1);
INSERT INTO ACCION_REALIZADA (Titulo_Accion_Realizada) VALUES 
('CREACIÓN'),
('MODIFICACIÓN'),
('ELIMINACIÓN');
INSERT INTO CANCHA (Nombre_Cancha, Ubicacion_Cancha) VALUES 
('Hoy se juega', 'Tabaré 697, CABA');
INSERT INTO ESTADO_RESERVA (Nombre_Estado_Reserva) VALUES 
('PENDIENTE'),
('CONFIRMADA'),
('CANCELADA');

--Aún sin implementar
--INSERT INTO TITULO_NOTIFICACION (Titulo_Notificacion) VALUES 
--('Confirmación de Reserva'),
--('Cancelación de Reserva');
--INSERT INTO NOTIFICACION (ID_usuario, ID_reserva, Mensaje_Notificacion, ID_titulo_notificacion) VALUES 
--(2, 1, 'Su reserva está pendiente de confirmación.', 1), 
--(1, 2, 'Su reserva ha sido confirmada con éxito.', 1);
INSERT INTO CONFIGURACION_PAGO(Monto_Sena) VALUES
(500);


------------------------------------------------------
SELECT * FROM USUARIO
UPDATE USUARIO 
SET
EmailConfirmationToken= NULL,
EmailConfirmed= 1,
ID_rol = 2
WHERE ID_usuario = 5;
