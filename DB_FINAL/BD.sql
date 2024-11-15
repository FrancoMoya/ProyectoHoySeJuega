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
    Nombre_Evento VARCHAR(80) NOT NULL DEFAULT 'CUMPLEAÑOS', -- Nombre o título del evento
    Descripcion_Evento VARCHAR(1000) NULL, -- Detalles o notas adicionales sobre el evento y la persona que reserva
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
    Fecha_Modificacion DATETIME NOT NULL DEFAULT GETDATE()
);

--EJEMPLOS: 
INSERT INTO TITULO_REPORTE (Titulo_Reporte) VALUES 
('Reporte Mensual de Usuarios'),
('Reporte Mensual de Reservas');
INSERT INTO REPORTE (Mes_Reporte, Anio_Reporte, Usuarios_Registrados_Reporte, Reservas_Realizadas_Reporte, Descripcion_Reporte, ID_titulo_reporte) VALUES 
(10, 2024, 100, 200, 'Informe sobre el crecimiento de usuarios y reservas en octubre de 2024.', 1),
(9, 2024, 150, 180, 'Informe sobre el rendimiento de reservas en septiembre de 2024.', 2);
INSERT INTO ROL (Nombre_Rol) VALUES 
('Admin'),
('Empleado'),
('Cliente');
INSERT INTO USUARIO (Nombre_Usuario, Apellido_Usuario, Correo_Usuario, Contrasenia_Usuario, Telefono_Usuario, ID_rol) VALUES 
('Carlos', 'Sánchez', 'franco@gmail.com', 'hola', '1234567890', 1),
('Laura', 'Méndez', 'laura.mendez@example.com', 'hashedpassword2', '0987654321', 3),
('Laura2', 'Méndez2', 'laura.mendez2@example.com', 'hashedpassword2', '0987654321', 3),
('Lauta', 'Méndez3', 'laura.mendez3@example.com', 'hashedpassword2', '0987654321', 2);
INSERT INTO PERMISO (Nombre_Permiso) VALUES 
('Crear Reserva'),
('Cancelar Reserva'),
('Ver Reportes'),
('Gestionar Canchas');
-- Asignar permisos a Administrador
INSERT INTO PERMISO_ROL (ID_rol, ID_permiso) VALUES 
(1, 1), -- Administrador - Crear Reserva
(1, 2), -- Administrador - Cancelar Reserva
(1, 3), -- Administrador - Ver Reportes
(1, 4); -- Administrador - Gestionar Canchas
-- Asignar permisos a Empleado
INSERT INTO PERMISO_ROL (ID_rol, ID_permiso) VALUES 
(2, 3), -- Empleado - Ver Reportes
(2, 4); -- Empleado - Gestionar Canchas
-- Asignar permisos a Cliente
INSERT INTO PERMISO_ROL (ID_rol, ID_permiso) VALUES 
(3, 1), -- Usuario - Crear Reserva
(3, 2); -- Usuario - Cancelar Reserva
INSERT INTO ACCION_REALIZADA (Titulo_Accion_Realizada) VALUES 
('Login'),
('Reserva Creada'),
('Pago Realizado');
INSERT INTO AUDITORIA (ID_usuario, Seccion, Descripcion_De_Accion, ID_accion_realizada) VALUES 
(1, 'USUARIO', 'El usuario Carlos inició sesión.', 1),
(2, 'RESERVA', 'El usuario Laura realizó una reserva.', 2);
INSERT INTO CANCHA (Nombre_Cancha, Ubicacion_Cancha) VALUES 
('Cancha A', 'Centro Deportivo Norte');
INSERT INTO HORARIO_DISPONIBLE (ID_cancha, Fecha_Horario, Hora_Inicio, Hora_Fin) VALUES 
(1, '2024-11-16', '08:00:00', '09:00:00'),
(1, '2024-11-16', '09:00:00', '10:00:00'),
(1, '2024-11-16', '10:00:00', '11:00:00'),
(1, '2024-11-16', '11:00:00', '12:00:00'),
(1, '2024-11-15', '12:00:00', '13:00:00'),
(1, '2024-11-15', '13:00:00', '14:00:00');
SELECT * FROM HORARIO_DISPONIBLE
INSERT INTO ESTADO_RESERVA (Nombre_Estado_Reserva) VALUES 
('PENDIENTE'),
('CONFIRMADA'),
('CANCELADA');
INSERT INTO RESERVA (ID_usuario, ID_horario_disponible, ID_estado_reserva) VALUES 
(2, 1, 1), -- Laura hizo una reserva en estado pendiente
(2, 2, 1), -- La
(3, 3, 2); -- Carlos tiene una reserva confirmada
INSERT INTO PAGO (ID_reserva, Monto_Pago) VALUES 
(1, 500.00), -- Pago de Laura por su reserva pendiente
(2, 700.00); -- Pago de Carlos por su reserva confirmada
INSERT INTO TITULO_NOTIFICACION (Titulo_Notificacion) VALUES 
('Confirmación de Reserva'),
('Cancelación de Reserva');
INSERT INTO NOTIFICACION (ID_usuario, ID_reserva, Mensaje_Notificacion, ID_titulo_notificacion) VALUES 
(2, 1, 'Su reserva está pendiente de confirmación.', 1), 
(1, 2, 'Su reserva ha sido confirmada con éxito.', 1);
INSERT INTO EVENTO (Descripcion_Evento, ID_horario_disponible, ID_estado_reserva) VALUES 
('Cumple de Carlos, reserva a nombre Lucas Martinez, celular: 11-4444-3333', 4, 1), -- Laura hizo una reserva en estado pendiente
('Cumple de Matias, reserva a nombre Julian Alvarez, celular: 11-4444-5555', 5, 1), -- La
('Cumple de Gonzalo, reserva a nombre Tomas Ojeda, celular: 11-6666-5555', 6, 2); -- Carlos tiene una reserva confirmada
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
