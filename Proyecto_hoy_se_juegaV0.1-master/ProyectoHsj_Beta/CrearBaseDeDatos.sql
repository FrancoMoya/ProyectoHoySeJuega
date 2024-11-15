IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [ACCION_REALIZADA] (
    [ID_accion_realizada] int NOT NULL IDENTITY,
    [Titulo_Accion_Realizada] nvarchar(70) NOT NULL,
    CONSTRAINT [PK__ACCION_R__2DC6E876AE3CA4B6] PRIMARY KEY ([ID_accion_realizada])
);
GO

CREATE TABLE [CANCHA] (
    [ID_cancha] int NOT NULL IDENTITY,
    [Nombre_Cancha] nvarchar(70) NOT NULL,
    [Ubicacion_Cancha] nvarchar(120) NOT NULL,
    CONSTRAINT [PK__CANCHA__A2D3DBCF8F513BBF] PRIMARY KEY ([ID_cancha])
);
GO

CREATE TABLE [ESTADO_RESERVA] (
    [ID_estado_reserva] int NOT NULL IDENTITY,
    [Nombre_Estado_Reserva] varchar(25) NOT NULL,
    CONSTRAINT [PK__ESTADO_R__746A84F0EE561664] PRIMARY KEY ([ID_estado_reserva])
);
GO

CREATE TABLE [HistorialReservas] (
    [IdHistorialReserva] int NOT NULL IDENTITY,
    [IdReserva] int NOT NULL,
    [ID_usuario] int NOT NULL,
    [FechaReserva] datetime2 NOT NULL,
    [HoraInicio] time NOT NULL,
    [HoraFin] time NOT NULL,
    [EstadoReserva] nvarchar(50) NOT NULL,
    [FechaEliminacion] datetime2 NOT NULL,
    [NombreUsuario] nvarchar(100) NOT NULL,
    [TelefonoUsuario] nvarchar(15) NOT NULL,
    CONSTRAINT [PK_HistorialReservas] PRIMARY KEY ([IdHistorialReserva])
);
GO

CREATE TABLE [PERMISO] (
    [ID_permiso] int NOT NULL IDENTITY,
    [Nombre_Permiso] nvarchar(50) NOT NULL,
    CONSTRAINT [PK__PERMISO__74B1E219FA556AF3] PRIMARY KEY ([ID_permiso])
);
GO

CREATE TABLE [ROL] (
    [ID_rol] int NOT NULL IDENTITY,
    [Nombre_Rol] nvarchar(50) NOT NULL,
    CONSTRAINT [PK__ROL__182A5412E3FEA872] PRIMARY KEY ([ID_rol])
);
GO

CREATE TABLE [TITULO_NOTIFICACION] (
    [ID_titulo_notificacion] int NOT NULL IDENTITY,
    [Titulo_Notificacion] nvarchar(80) NOT NULL,
    CONSTRAINT [PK__TITULO_N__DA36445EB454B7E0] PRIMARY KEY ([ID_titulo_notificacion])
);
GO

CREATE TABLE [TITULO_REPORTE] (
    [ID_titulo_reporte] int NOT NULL IDENTITY,
    [Titulo_Reporte] nvarchar(80) NOT NULL,
    CONSTRAINT [PK__TITULO_R__5F9C0CCDB6AEF5F9] PRIMARY KEY ([ID_titulo_reporte])
);
GO

CREATE TABLE [HORARIO_DISPONIBLE] (
    [ID_horario_disponible] int NOT NULL IDENTITY,
    [ID_cancha] int NOT NULL,
    [Fecha_Horario] date NOT NULL DEFAULT ((getdate())),
    [Hora_Inicio] time NOT NULL,
    [Hora_Fin] time NOT NULL,
    [Disponible_Horario] bit NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK__HORARIO___7D5601B788E66056] PRIMARY KEY ([ID_horario_disponible]),
    CONSTRAINT [FK__HORARIO_D__ID_ca__5441852A] FOREIGN KEY ([ID_cancha]) REFERENCES [CANCHA] ([ID_cancha])
);
GO

CREATE TABLE [PERMISO_ROL] (
    [ID_rol] int NOT NULL,
    [ID_permiso] int NOT NULL,
    CONSTRAINT [PK__PERMISO___9F614A336556D35C] PRIMARY KEY ([ID_rol], [ID_permiso]),
    CONSTRAINT [FK__PERMISO_R__ID_pe__45F365D3] FOREIGN KEY ([ID_permiso]) REFERENCES [PERMISO] ([ID_permiso]),
    CONSTRAINT [FK__PERMISO_R__ID_ro__46E78A0C] FOREIGN KEY ([ID_rol]) REFERENCES [ROL] ([ID_rol])
);
GO

CREATE TABLE [USUARIO] (
    [ID_usuario] int NOT NULL IDENTITY,
    [Nombre_Usuario] nvarchar(30) NOT NULL,
    [Apellido_Usuario] nvarchar(30) NOT NULL,
    [Correo_Usuario] varchar(100) NOT NULL,
    [Contrasenia_Usuario] nvarchar(255) NOT NULL,
    [Telefono_Usuario] int NOT NULL,
    [Activo] bit NULL DEFAULT CAST(1 AS bit),
    [PasswordResetToken] nvarchar(max) NULL,
    [PasswordResetTokenExpiry] datetime NULL,
    [EmailConfirmationToken] nvarchar(255) NULL,
    [EmailConfirmed] bit NULL,
    [ID_rol] int NOT NULL,
    CONSTRAINT [PK__USUARIO__DF3D425202CA1D01] PRIMARY KEY ([ID_usuario]),
    CONSTRAINT [FK__USUARIO__ID_rol__412EB0B6] FOREIGN KEY ([ID_rol]) REFERENCES [ROL] ([ID_rol])
);
GO

CREATE TABLE [REPORTE] (
    [ID_reporte] int NOT NULL IDENTITY,
    [Mes_Reporte] tinyint NOT NULL,
    [Anio_Reporte] smallint NOT NULL,
    [Usuarios_Registrados_Reporte] int NULL,
    [Reservas_Realizadas_Reporte] int NULL,
    [Fecha_De_Reporte] datetime NULL DEFAULT ((getdate())),
    [Descripcion_Reporte] varchar(max) NOT NULL,
    [ID_titulo_reporte] int NOT NULL,
    CONSTRAINT [PK__REPORTE__41AEEB64E2FCCC33] PRIMARY KEY ([ID_reporte]),
    CONSTRAINT [FK__REPORTE__ID_titu__3A81B327] FOREIGN KEY ([ID_titulo_reporte]) REFERENCES [TITULO_REPORTE] ([ID_titulo_reporte])
);
GO

CREATE TABLE [AUDITORIA] (
    [ID_auditoria] int NOT NULL IDENTITY,
    [ID_usuario] int NOT NULL,
    [Fecha_Auditoria] datetime NOT NULL DEFAULT ((getdate())),
    [Descripcion_De_Accion] nvarchar(max) NOT NULL,
    [ID_accion_realizada] int NOT NULL,
    CONSTRAINT [PK__AUDITORI__F6FFFB8CE3101847] PRIMARY KEY ([ID_auditoria]),
    CONSTRAINT [FK__AUDITORIA__ID_ac__4CA06362] FOREIGN KEY ([ID_accion_realizada]) REFERENCES [ACCION_REALIZADA] ([ID_accion_realizada]),
    CONSTRAINT [FK__AUDITORIA__ID_us__4D94879B] FOREIGN KEY ([ID_usuario]) REFERENCES [USUARIO] ([ID_usuario])
);
GO

CREATE TABLE [RESERVA] (
    [ID_reserva] int NOT NULL IDENTITY,
    [ID_usuario] int NOT NULL,
    [ID_horario_disponible] int NOT NULL,
    [Fecha_Reserva] datetime NULL DEFAULT ((getdate())),
    [ID_estado_reserva] int NOT NULL,
    CONSTRAINT [PK__RESERVA__CD692CB066E0B8CB] PRIMARY KEY ([ID_reserva]),
    CONSTRAINT [FK__RESERVA__ID_esta__59FA5E80] FOREIGN KEY ([ID_estado_reserva]) REFERENCES [ESTADO_RESERVA] ([ID_estado_reserva]),
    CONSTRAINT [FK__RESERVA__ID_hora__5BE2A6F2] FOREIGN KEY ([ID_horario_disponible]) REFERENCES [HORARIO_DISPONIBLE] ([ID_horario_disponible]),
    CONSTRAINT [FK__RESERVA__ID_usua__5AEE82B9] FOREIGN KEY ([ID_usuario]) REFERENCES [USUARIO] ([ID_usuario])
);
GO

CREATE TABLE [NOTIFICACION] (
    [ID_notificacion] int NOT NULL IDENTITY,
    [ID_usuario] int NOT NULL,
    [ID_reserva] int NOT NULL,
    [Mensaje_Notificacion] nvarchar(max) NOT NULL,
    [Fecha_Envio_Notificacion] datetime NOT NULL DEFAULT ((getdate())),
    [ID_titulo_notificacion] int NOT NULL,
    CONSTRAINT [PK__NOTIFICA__99BC7E5EC04B01F2] PRIMARY KEY ([ID_notificacion]),
    CONSTRAINT [FK__NOTIFICAC__ID_re__6754599E] FOREIGN KEY ([ID_reserva]) REFERENCES [RESERVA] ([ID_reserva]),
    CONSTRAINT [FK__NOTIFICAC__ID_ti__656C112C] FOREIGN KEY ([ID_titulo_notificacion]) REFERENCES [TITULO_NOTIFICACION] ([ID_titulo_notificacion]),
    CONSTRAINT [FK__NOTIFICAC__ID_us__66603565] FOREIGN KEY ([ID_usuario]) REFERENCES [USUARIO] ([ID_usuario])
);
GO

CREATE TABLE [PAGO] (
    [ID_pago] int NOT NULL IDENTITY,
    [ID_reserva] int NOT NULL,
    [Monto_Pago] decimal(10,2) NOT NULL,
    [Fecha_Pago] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__PAGO__808903EC47A081C7] PRIMARY KEY ([ID_pago]),
    CONSTRAINT [FK__PAGO__ID_reserva__5FB337D6] FOREIGN KEY ([ID_reserva]) REFERENCES [RESERVA] ([ID_reserva])
);
GO

CREATE INDEX [IX_AUDITORIA_ID_accion_realizada] ON [AUDITORIA] ([ID_accion_realizada]);
GO

CREATE INDEX [IX_AUDITORIA_ID_usuario] ON [AUDITORIA] ([ID_usuario]);
GO

CREATE INDEX [IX_HORARIO_DISPONIBLE_ID_cancha] ON [HORARIO_DISPONIBLE] ([ID_cancha]);
GO

CREATE INDEX [IX_NOTIFICACION_ID_reserva] ON [NOTIFICACION] ([ID_reserva]);
GO

CREATE INDEX [IX_NOTIFICACION_ID_titulo_notificacion] ON [NOTIFICACION] ([ID_titulo_notificacion]);
GO

CREATE INDEX [IX_NOTIFICACION_ID_usuario] ON [NOTIFICACION] ([ID_usuario]);
GO

CREATE INDEX [IX_PAGO_ID_reserva] ON [PAGO] ([ID_reserva]);
GO

CREATE INDEX [IX_PERMISO_ROL_ID_permiso] ON [PERMISO_ROL] ([ID_permiso]);
GO

CREATE INDEX [IX_REPORTE_ID_titulo_reporte] ON [REPORTE] ([ID_titulo_reporte]);
GO

CREATE INDEX [IX_RESERVA_ID_estado_reserva] ON [RESERVA] ([ID_estado_reserva]);
GO

CREATE INDEX [IX_RESERVA_ID_horario_disponible] ON [RESERVA] ([ID_horario_disponible]);
GO

CREATE INDEX [IX_RESERVA_ID_usuario] ON [RESERVA] ([ID_usuario]);
GO

CREATE INDEX [IX_USUARIO_ID_rol] ON [USUARIO] ([ID_rol]);
GO

CREATE UNIQUE INDEX [UQ__USUARIO__A7126311EFE6490D] ON [USUARIO] ([Correo_Usuario]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241104074429_InitialCreate', N'8.0.10');
GO

COMMIT;
GO

