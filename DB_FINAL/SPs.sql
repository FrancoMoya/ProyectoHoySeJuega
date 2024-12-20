-- STORED PROCEDURES
--AUDITORIA
CREATE PROCEDURE SP_AUDITORIA
	@ID_usuario INT,
	@Seccion VARCHAR(50),
	@Descripccion_De_Accion NVARCHAR(1000),
	@ID_accion_realizada INT
AS
BEGIN
	DECLARE @Fecha_Auditoria DATETIME = DATEADD(HOUR, 3, GETDATE()); --Para el server que tiene 3 horas atrasadas
	INSERT INTO AUDITORIA(ID_usuario, Fecha_Auditoria, Seccion, Descripcion_De_Accion, ID_accion_realizada)
	VALUES(@ID_usuario, @Fecha_Auditoria, @Seccion, @Descripccion_De_Accion, @ID_accion_realizada);
END;
-- LISTAR AUDITORIAS PARA EL PANEL ADMIN
CREATE PROCEDURE SP_GET_AUDITORIA 
AS
BEGIN
    SELECT 
        a.ID_auditoria AS IdAuditoria, 
        a.Fecha_Auditoria AS FechaAuditoria, 
        COALESCE(ac.Titulo_Accion_Realizada,'TituloEliminado') AS TituloAccionRealizada, 
        COALESCE(u.Nombre_Usuario, 'UsuarioEliminado') AS NombreUsuario, 
        COALESCE(u.Apellido_Usuario, 'UsuarioEliminado') AS ApellidoUsuario, 
		COALESCE(u.ID_usuario, '0') AS IdUsuario, 
        a.Seccion AS Seccion, 
        a.Descripcion_De_Accion AS DescripcionDeAccion
    FROM 
        AUDITORIA a
    LEFT JOIN 
        ACCION_REALIZADA ac ON a.ID_accion_realizada = ac.ID_accion_realizada
    LEFT JOIN 
        USUARIO u ON a.ID_Usuario = u.ID_Usuario
	WHERE a.Fecha_Auditoria >= DATEADD(DAY, -7, GETDATE()) -- Filtra los pagos de los �ltimos 7 d�as
	ORDER BY a.Fecha_Auditoria DESC;
END;
-------------------------------------
---HISTORIAL COMPLETO AUDITORIA
CREATE PROCEDURE SP_GET_ALL_AUDITORIA 
AS
BEGIN
    SELECT 
        a.ID_auditoria AS IdAuditoria, 
        a.Fecha_Auditoria AS FechaAuditoria, 
        COALESCE(ac.Titulo_Accion_Realizada,'TituloEliminado') AS TituloAccionRealizada, 
        COALESCE(u.Nombre_Usuario, 'UsuarioEliminado') AS NombreUsuario, 
        COALESCE(u.Apellido_Usuario, 'UsuarioEliminado') AS ApellidoUsuario, 
		COALESCE(u.ID_usuario, '0') AS IdUsuario, 
        a.Seccion AS Seccion, 
        a.Descripcion_De_Accion AS DescripcionDeAccion
    FROM 
        AUDITORIA a
    LEFT JOIN 
        ACCION_REALIZADA ac ON a.ID_accion_realizada = ac.ID_accion_realizada
    LEFT JOIN 
        USUARIO u ON a.ID_Usuario = u.ID_Usuario
	ORDER BY a.Fecha_Auditoria DESC;
END;
---------------------------
-- STORED PARA CANCELACION DE RESERVAS AUTOMATICA
CREATE PROCEDURE SP_GET_RESERVAS_PENDIENTES
AS
BEGIN
    -- Sumamos 3 horas a la hora actual del servidor para corregir la diferencia
    DECLARE @HoraServidor DATETIME = DATEADD(HOUR, 3, GETDATE());

    SELECT ID_reserva 
    FROM RESERVA 
    WHERE ID_estado_reserva = 1 
    AND DATEDIFF(SECOND, Fecha_Reserva, @HoraServidor) >= 180; --3m
END;
---------
CREATE PROCEDURE SP_GET_RESERVAS_PAGOENCURSO
AS
BEGIN
    -- Sumamos 3 horas a la hora actual del servidor para corregir la diferencia
    DECLARE @HoraServidor DATETIME = DATEADD(HOUR, 3, GETDATE());

    SELECT ID_reserva 
    FROM RESERVA 
    WHERE ID_estado_reserva = 4 
    AND DATEDIFF(SECOND, Fecha_Reserva, @HoraServidor) >= 370; -- PENDIENTES + 3m10s = 6m 10s
END;
----------
CREATE PROCEDURE SP_CANCELAR_RESERVAS
    @ID_reserva INT
AS
BEGIN
    UPDATE RESERVA
    SET ID_estado_reserva = 3
    WHERE ID_reserva = @ID_reserva;
	--Obtener el ID del horario disponible asociado a la reserva cancelada
    DECLARE @ID_horario_disponible INT;
    SELECT @ID_horario_disponible = ID_horario_disponible
    FROM RESERVA
    WHERE ID_reserva = @ID_reserva;
    UPDATE HORARIO_DISPONIBLE
    SET Disponible_Horario = 1
    WHERE ID_horario_disponible = @ID_horario_disponible;
END;
-----------------------------
-- STORED PARA CREACION DE RESERVAS
CREATE PROCEDURE SP_RESERVA_CREATE
	@ID_usuario INT,
	@ID_horario_disponible INT,
	@ID_reserva INT OUTPUT  -- A�adir par�metro de salida para el ID de la reserva
AS
BEGIN
	DECLARE @Fecha_Reserva DATETIME = DATEADD(HOUR, 3, GETDATE()); --Para el server que tiene 3 horas atrasadas
	DECLARE @ID_estado_reserva INT = 1;
	INSERT INTO RESERVA(ID_usuario, Fecha_Reserva, ID_horario_disponible, ID_estado_reserva)
    VALUES (@ID_usuario, @Fecha_Reserva, @ID_horario_disponible, @ID_estado_reserva);
	SET @ID_reserva = SCOPE_IDENTITY();  -- Esto devuelve el �ltimo ID insertado
	UPDATE HORARIO_DISPONIBLE
    SET Disponible_Horario = 0
    WHERE ID_horario_disponible = @ID_horario_disponible;
END;
---------------------
-- SP PARA LISTAR LAS RESERVAS DEL USUARIO (CON DETALLES DE PAGO)
CREATE PROCEDURE SP_GET_MIS_RESERVAS
    @ID_usuario INT
AS
BEGIN
    SELECT 
        r.ID_reserva AS IdReserva,
        r.Fecha_Reserva AS FechaReserva,
        COALESCE(e.Nombre_Estado_Reserva, 'EstadoEliminado') AS NombreEstadoReserva,  
        COALESCE(h.Fecha_Horario, '2000-01-01') AS FechaHorario, 
        COALESCE(h.Hora_Inicio, '00:00:00') AS HoraInicio, 
        COALESCE(h.Hora_Fin, '00:00:00') AS HoraFin,
		COALESCE(p.ID_pago, '0') AS IdPago,
		COALESCE(p.Fecha_Pago, '2000-01-01 00:00:00') AS FechaPago,
		COALESCE(p.Monto_Pago, '0') AS Monto
    FROM 
        RESERVA r
    LEFT JOIN 
        HORARIO_DISPONIBLE h ON r.ID_horario_disponible = h.ID_horario_disponible 
    LEFT JOIN 
        PAGO p ON r.ID_reserva = p.ID_reserva
	LEFT JOIN 
        ESTADO_RESERVA e ON r.ID_estado_reserva = e.ID_estado_reserva 
    WHERE r.ID_usuario = @ID_usuario
    ORDER BY
        r.Fecha_Reserva DESC
END;
-----------------------------------------
-- LISTAR USUARIOS ADMIN
CREATE PROCEDURE SP_GET_USERS
AS
BEGIN
    SELECT 
		u.ID_usuario AS IdUsuario,
        u.Nombre_Usuario AS NombreUsuario,
		u.Apellido_Usuario AS ApellidoUsuario,
		u.Correo_Usuario AS CorreoUsuario,
		u.Telefono_Usuario AS TelefonoUsuario,
        COALESCE(r.Nombre_Rol, 'RolEliminado') AS RolUsuario,
		u.Activo AS Activo,
		u.Ultima_Sesion AS UltimaSesion
    FROM 
        USUARIO u
    LEFT JOIN 
        ROL r ON u.ID_rol = r.ID_rol
	WHERE u.EmailConfirmed = 1
	ORDER BY u.Fecha_Registro DESC
END;
---------------------------------------------
-- LISTAR ROLES CON PERMISOS ADMIN
CREATE PROCEDURE SP_GET_ROLES_PERMISOS
AS
BEGIN
	SELECT 
		R.ID_rol AS IdRol,
		R.Nombre_Rol AS NombreRol, 
		COALESCE(STRING_AGG(P.Nombre_Permiso, ', '), 'Sin permisos') AS Permisos
	FROM 
		ROL R
	LEFT JOIN 
		PERMISO_ROL PR ON R.ID_rol = PR.ID_rol
	LEFT JOIN 
		PERMISO P ON PR.ID_permiso = P.ID_permiso
	GROUP BY 
		R.ID_rol, R.Nombre_Rol
	ORDER BY 
		R.Nombre_Rol;
END;
-----------------------------------
-- LISTAR EVENTOS ADMIN (NUEVO)
CREATE PROCEDURE SP_GET_EVENTOS_ADMIN
AS
BEGIN
-- Fecha y hora actual del servidor ajustada con +3 horas
    DECLARE @FechaHoraActual DATETIME = DATEADD(HOUR, 3, GETDATE());
	SELECT
	e.ID_evento AS IdEvento,
	e.Fecha_Evento AS FechaEvento,
	e.Nombre_Evento AS NombreEvento,
	COALESCE(e.Descripcion_Evento, 'Sin descripci�n') AS DescripcionEvento,
	COALESCE(e.Correo_Cliente_Evento, 'Sin correo') AS CorreoClienteEvento,
	COALESCE(e.Telefono_Cliente_Evento, '0000000000') AS TelefonoClienteEvento,
	COALESCE(h.Fecha_Horario, '2000-01-01') AS FechaHorario,
	COALESCE(h.Hora_Inicio, '00:00:00')AS HoraInicio,
	COALESCE(h.Hora_Fin, '00:00:00') AS HoraFin,
	COALESCE(es.Nombre_Estado_Reserva, 'EstadoEliminado') AS NombreEstadoReserva
	FROM EVENTO e
	LEFT JOIN HORARIO_DISPONIBLE h ON e.ID_horario_disponible = h.ID_horario_disponible
	LEFT JOIN ESTADO_RESERVA es ON e.ID_estado_reserva = es.ID_estado_reserva
	WHERE es.ID_estado_reserva = 2
	AND CAST(h.Fecha_Horario AS DATE) >= CAST(@FechaHoraActual AS DATE)
 -- Filtra eventos a partir de la fecha y hora actual ajustada
	ORDER BY h.Fecha_Horario, h.Hora_Inicio
END;
--------------------------
--HISTORIAL EVENTOS ADMIN
CREATE PROCEDURE SP_GET_RESERVAS_ADMIN_HISTORIAL
AS
BEGIN
	SELECT
	e.ID_evento AS IdEvento,
	e.Fecha_Evento AS FechaEvento,
	e.Nombre_Evento AS NombreEvento,
	COALESCE(e.Descripcion_Evento, 'Sin descripci�n') AS DescripcionEvento,
	COALESCE(e.Correo_Cliente_Evento, 'Sin correo') AS CorreoClienteEvento,
	COALESCE(e.Telefono_Cliente_Evento, '0000000000') AS TelefonoClienteEvento,
	COALESCE(h.Fecha_Horario, '2000-01-01') AS FechaHorario,
	COALESCE(h.Hora_Inicio, '00:00:00')AS HoraInicio,
	COALESCE(h.Hora_Fin, '00:00:00') AS HoraFin,
	COALESCE(es.Nombre_Estado_Reserva, 'EstadoEliminado') AS NombreEstadoReserva
	FROM EVENTO e
	LEFT JOIN HORARIO_DISPONIBLE h ON e.ID_horario_disponible = h.ID_horario_disponible
	LEFT JOIN ESTADO_RESERVA es ON e.ID_estado_reserva = es.ID_estado_reserva
	ORDER BY e.ID_evento DESC
END;
------------------------------------
-- VER RESERVAS CLIENTES ADMIN (CALENDARIO AEBL)
CREATE PROCEDURE SP_GET_RESERVAS_ADMIN
AS
BEGIN
	DECLARE @FechaHoraActual DATETIME = DATEADD(HOUR, 3, GETDATE());
    SELECT
        r.ID_reserva AS idReserva, 
        UPPER(COALESCE(u.Apellido_Usuario, 'UsuarioEliminado')) AS title,
		COALESCE(
		CAST(
			CONCAT(
				CONVERT(VARCHAR, h.Fecha_Horario, 120), -- Convierte la fecha a formato 'yyyy-mm-dd'
				' ',
				CONVERT(VARCHAR, h.Hora_Inicio, 108)  -- Convierte la hora a formato 'hh:mm:ss'
			) AS DATETIME),
		'2000-01-01 00:00:00'
		) AS start,
 -- Fecha + HoraInicio
        COALESCE(es.Nombre_Estado_Reserva, 'EstadoEliminado') AS estado,
        COALESCE(
            CONCAT(
                COALESCE(u.Nombre_Usuario, 'UsuarioEliminado'), 
                ' ',  -- Espacio
                COALESCE(u.Apellido_Usuario, 'UsuarioEliminado') 
            ),
            'UsuarioEliminado'  -- Valor por defecto si ambos son NULL
        ) AS usuarioNombre,
        COALESCE(u.Telefono_Usuario, '0000000000') AS usuarioTelefono
    FROM RESERVA r
    LEFT JOIN USUARIO u ON r.ID_usuario = u.ID_usuario
    LEFT JOIN ESTADO_RESERVA es ON r.ID_estado_reserva = es.ID_estado_reserva
    LEFT JOIN HORARIO_DISPONIBLE h ON r.ID_horario_disponible = h.ID_horario_disponible
	WHERE es.ID_estado_reserva = 2
	AND CAST(h.Fecha_Horario AS DATE) >= CAST(@FechaHoraActual AS DATE)
	ORDER BY h.Fecha_Horario, h.Hora_Inicio
END;
--------------------------------------
-- VER RESERVAS EVENTOS ADMIN (CALENDARIO)
CREATE PROCEDURE SP_GET_RESERVAS_EVENTOS_CALENDAR_ADMIN
AS
BEGIN
	DECLARE @FechaHoraActual DATETIME = DATEADD(HOUR, 3, GETDATE());
    SELECT
        e.ID_evento AS idReserva, 
        e.Nombre_Evento AS title,
		COALESCE(
		CAST(
			CONCAT(
				CONVERT(VARCHAR, h.Fecha_Horario, 120), 
				' ',
				CONVERT(VARCHAR, h.Hora_Inicio, 108) 
			) AS DATETIME),
		'2000-01-01 00:00:00'
		) AS start,
 -- Fecha + HoraInicio
        COALESCE(es.Nombre_Estado_Reserva, 'EstadoEliminado') AS estado,
        COALESCE(e.Descripcion_Evento, 'Sin descripci�n') AS usuarioNombre,
        COALESCE(e.Telefono_Cliente_Evento, '0000000000') AS usuarioTelefono
    FROM EVENTO e
    LEFT JOIN ESTADO_RESERVA es ON e.ID_estado_reserva = es.ID_estado_reserva
    LEFT JOIN HORARIO_DISPONIBLE h ON e.ID_horario_disponible = h.ID_horario_disponible
	WHERE es.ID_estado_reserva = 2
	AND CAST(h.Fecha_Horario AS DATE) >= CAST(@FechaHoraActual AS DATE)
	ORDER BY h.Fecha_Horario, h.Hora_Inicio
END;
------------------------------------
-- VER TODAS LAS RESERVAS ADMIN (CALENDARIO)
CREATE PROCEDURE SP_GET_ALL_RESERVAS_CALENDAR_ADMIN
AS
BEGIN
	DECLARE @FechaHoraActual DATETIME = DATEADD(HOUR, 3, GETDATE());
	--reservas clientes
	SELECT
        r.ID_reserva AS idReserva, 
        UPPER(COALESCE(u.Apellido_Usuario, 'UsuarioEliminado')) AS title,
		COALESCE(
		CAST(
			CONCAT(
				CONVERT(VARCHAR, h.Fecha_Horario, 120), 
				' ',
				CONVERT(VARCHAR, h.Hora_Inicio, 108)  
			) AS DATETIME),
		'2000-01-01 00:00:00'
		) AS start,
 -- Fecha + HoraInicio
        COALESCE(es.Nombre_Estado_Reserva, 'EstadoEliminado') AS estado,
        COALESCE(
            CONCAT(
                COALESCE(u.Nombre_Usuario, 'UsuarioEliminado'), 
                ' ',  -- Espacio
                COALESCE(u.Apellido_Usuario, 'UsuarioEliminado') 
            ),
            'UsuarioEliminado'  -- Valor por defecto si ambos son NULL
        ) AS usuarioNombre,
        COALESCE(u.Telefono_Usuario, '0000000000') AS usuarioTelefono
    FROM RESERVA r
    LEFT JOIN USUARIO u ON r.ID_usuario = u.ID_usuario
    LEFT JOIN ESTADO_RESERVA es ON r.ID_estado_reserva = es.ID_estado_reserva
    LEFT JOIN HORARIO_DISPONIBLE h ON r.ID_horario_disponible = h.ID_horario_disponible
	WHERE es.ID_estado_reserva = 2
	AND CAST(h.Fecha_Horario AS DATE) >= CAST(@FechaHoraActual AS DATE)

	UNION ALL
	--eventos admin
    SELECT
        e.ID_evento AS idReserva, 
        e.Nombre_Evento AS title,
		COALESCE(
		CAST(
			CONCAT(
				CONVERT(VARCHAR, h.Fecha_Horario, 120), 
				' ',
				CONVERT(VARCHAR, h.Hora_Inicio, 108)  
			) AS DATETIME),
		'2000-01-01 00:00:00'
		) AS start,
 -- Fecha + HoraInicio
        COALESCE(es.Nombre_Estado_Reserva, 'EstadoEliminado') AS estado,
        COALESCE(e.Descripcion_Evento, 'Sin descripci�n') AS usuarioNombre,
        COALESCE(e.Telefono_Cliente_Evento, '0000000000') AS usuarioTelefono
    FROM EVENTO e
    LEFT JOIN ESTADO_RESERVA es ON e.ID_estado_reserva = es.ID_estado_reserva
    LEFT JOIN HORARIO_DISPONIBLE h ON e.ID_horario_disponible = h.ID_horario_disponible
	WHERE es.ID_estado_reserva = 2
	AND CAST(h.Fecha_Horario AS DATE) >= CAST(@FechaHoraActual AS DATE) 
END;
---------------------------------------
-- LISTAR FIJOS CREADOS ADMIN
CREATE PROCEDURE SP_GET_FIJOS_CREADOS_ADMIN
AS
BEGIN
    SELECT
        Nombre AS Nombre,
        COALESCE(Descripcion, 'Sin descripci�n') AS Descripcion,
        COALESCE(CorreoCliente, 'Sin correo') AS CorreoCliente,
        COALESCE(TelefonoCliente, '0000000000') AS TelefonoCliente,
        HoraInicio AS HoraInicio,
        CASE DiaSemana
            WHEN 1 THEN 'Lunes'
            WHEN 2 THEN 'Martes'
            WHEN 3 THEN 'Mi�rcoles'
            WHEN 4 THEN 'Jueves'
            WHEN 5 THEN 'Viernes'
            WHEN 6 THEN 'S�bado'
            WHEN 0 THEN 'Domingo'
            ELSE 'Desconocido'
        END AS DiaSemana,
        FechaInicio AS FechaInicio,
        FechaFin AS FechaFin
    FROM EVENTO_RECURRENTE
	ORDER BY ID_evento_recurrente DESC
END;
-----------------------------------------------------------------------------------------------
--LISTAR TODAS LAS RESERVAS ADMIN
CREATE PROCEDURE SP_GET_ALL_RESERVAS_ADMIN
AS
BEGIN
    -- Variable para la fecha actual ajustada
    DECLARE @FechaHoraActual DATETIME = DATEADD(HOUR, 3, GETDATE());

    -- Consultar Reservas y Eventos combinados
    SELECT 
        Id,
        FechaCreacion,
        Titulo,
        Descripcion,
        Celular,
        FechaHorario, 
        HoraInicio, 
        HoraFin,
        Estado
    FROM (
        -- Consultar Reservas
        SELECT 
            r.ID_reserva AS Id,
            r.Fecha_Reserva AS FechaCreacion,
            'RESERVA CLIENTE' AS Titulo,
            COALESCE(
                CONCAT(
                    COALESCE(u.Nombre_Usuario, 'UsuarioEliminado'),  
                    ' ',  -- Espacio
                    COALESCE(u.Apellido_Usuario, 'UsuarioEliminado')  
                ),
                'UsuarioEliminado'  -- Valor por defecto
            ) AS Descripcion,
            COALESCE(u.Telefono_Usuario, '0000000000') AS Celular,
            COALESCE(h.Fecha_Horario, '2000-01-01') AS FechaHorario, 
            COALESCE(h.Hora_Inicio, '00:00:00') AS HoraInicio, 
            COALESCE(h.Hora_Fin, '00:00:00') AS HoraFin,
            COALESCE(es.ID_estado_reserva, '0') AS Estado
        FROM 
            RESERVA r
        LEFT JOIN 
            HORARIO_DISPONIBLE h ON r.ID_horario_disponible = h.ID_horario_disponible  
        LEFT JOIN
            USUARIO u ON r.ID_usuario = u.ID_usuario
        LEFT JOIN 
            ESTADO_RESERVA es ON r.ID_estado_reserva = es.ID_estado_reserva
        WHERE 
            es.ID_estado_reserva = 2
            AND CAST(h.Fecha_Horario AS DATE) >= CAST(@FechaHoraActual AS DATE)

        UNION ALL
        -- Consultar Eventos
        SELECT
            e.ID_evento AS Id,
            e.Fecha_Evento AS FechaCreacion,
            e.Nombre_Evento AS Titulo,
            COALESCE(e.Descripcion_Evento, 'Sin descripci�n') AS Descripcion,
            COALESCE(e.Telefono_Cliente_Evento, '0000000000') AS Celular,
            COALESCE(h.Fecha_Horario, '2000-01-01') AS FechaHorario,
            COALESCE(h.Hora_Inicio, '00:00:00') AS HoraInicio,
            COALESCE(h.Hora_Fin, '00:00:00') AS HoraFin,
            COALESCE(es.ID_estado_reserva, '0') AS Estado
        FROM 
            EVENTO e
        LEFT JOIN 
            HORARIO_DISPONIBLE h ON e.ID_horario_disponible = h.ID_horario_disponible
        LEFT JOIN 
            ESTADO_RESERVA es ON e.ID_estado_reserva = es.ID_estado_reserva
        WHERE 
            es.ID_estado_reserva = 2
            AND CAST(h.Fecha_Horario AS DATE) >= CAST(@FechaHoraActual AS DATE)
    ) AS CombinedResults
    ORDER BY FechaHorario, HoraInicio;
END;
----------------------------
-- HISTORIAL COMPLETO DE RESERVAS (CLIENTES Y ADMIN)
CREATE PROCEDURE SP_GET_ADMIN_HISTORIAL_RESERVAS
AS
BEGIN
    -- Consultar Reservas y Eventos combinados
    SELECT 
        Id,
        FechaCreacion,
        Titulo,
        Descripcion,
        Celular,
        FechaHorario, 
        HoraInicio, 
        HoraFin,
        Estado
    FROM (
        -- Consultar Reservas
        SELECT 
            r.ID_reserva AS Id,
            r.Fecha_Reserva AS FechaCreacion,
            'RESERVA CLIENTE' AS Titulo,
            COALESCE(
                CONCAT(
                    COALESCE(u.Nombre_Usuario, 'UsuarioEliminado'),  
                    ' ',  -- Espacio entre nombre y apellido
                    COALESCE(u.Apellido_Usuario, 'UsuarioEliminado')  
                ),
                'UsuarioEliminado'  -- Valor por defecto si ambos son NULL
            ) AS Descripcion,
            COALESCE(u.Telefono_Usuario, '0000000000') AS Celular,
            COALESCE(h.Fecha_Horario, '2000-01-01') AS FechaHorario, 
            COALESCE(h.Hora_Inicio, '00:00:00') AS HoraInicio, 
            COALESCE(h.Hora_Fin, '00:00:00') AS HoraFin,
            COALESCE(es.Nombre_Estado_Reserva, 'EstadoEliminado') AS Estado
        FROM 
            RESERVA r
        LEFT JOIN 
            HORARIO_DISPONIBLE h ON r.ID_horario_disponible = h.ID_horario_disponible  
        LEFT JOIN
            USUARIO u ON r.ID_usuario = u.ID_usuario
        LEFT JOIN 
            ESTADO_RESERVA es ON r.ID_estado_reserva = es.ID_estado_reserva

        UNION ALL
        -- Consultar Eventos
        SELECT
            e.ID_evento AS Id,
            e.Fecha_Evento AS FechaCreacion,
            e.Nombre_Evento AS Titulo,
            COALESCE(e.Descripcion_Evento, 'Sin descripci�n') AS Descripcion,
            COALESCE(e.Telefono_Cliente_Evento, '0000000000') AS Celular,
            COALESCE(h.Fecha_Horario, '2000-01-01') AS FechaHorario,
            COALESCE(h.Hora_Inicio, '00:00:00') AS HoraInicio,
            COALESCE(h.Hora_Fin, '00:00:00') AS HoraFin,
            COALESCE(es.Nombre_Estado_Reserva, 'EstadoEliminado') AS Estado
        FROM 
            EVENTO e
        LEFT JOIN 
            HORARIO_DISPONIBLE h ON e.ID_horario_disponible = h.ID_horario_disponible
        LEFT JOIN 
            ESTADO_RESERVA es ON e.ID_estado_reserva = es.ID_estado_reserva
    ) AS CombinedResults
    ORDER BY Id DESC
END;
------------------
-- LISTAR LAS RESERVAS CONFIRMADAS DE CLIENTES ADMIN (TARJETAS)
CREATE PROCEDURE SP_GET_RESERVAS_CLIENTES_ADMIN
AS
BEGIN
	DECLARE @FechaHoraActual DATETIME = DATEADD(HOUR, 3, GETDATE());
    SELECT 
        r.ID_reserva AS Id,
		r.Fecha_Reserva AS FechaCreacion,
		COALESCE(u.Nombre_Usuario, 'UsuarioEliminado') AS NombreUsuario,
		COALESCE(u.Apellido_Usuario, 'UsuarioEliminado') AS ApellidoUsuario,
		COALESCE(u.Telefono_Usuario, '0000000000') AS Celular,
        COALESCE(h.Fecha_Horario, '2000-01-01') AS FechaHorario, 
        COALESCE(h.Hora_Inicio, '00:00:00') AS HoraInicio, 
        COALESCE(h.Hora_Fin, '00:00:00') AS HoraFin,
        COALESCE(es.ID_estado_reserva, '0') AS Estado
    FROM 
        RESERVA r
    LEFT JOIN 
        HORARIO_DISPONIBLE h ON r.ID_horario_disponible = h.ID_horario_disponible
	LEFT JOIN 
        USUARIO u ON r.ID_usuario = u.ID_usuario
    LEFT JOIN 
        ESTADO_RESERVA es ON r.ID_estado_reserva = es.ID_estado_reserva 
	WHERE 
        es.ID_estado_reserva = 2
        AND CAST(h.Fecha_Horario AS DATE) >= CAST(@FechaHoraActual AS DATE)
	ORDER BY h.Fecha_Horario, h.Hora_Inicio
END;
--------------------------------
--HISTORIAL RESERVAS CLIENTES ADMIN
CREATE PROCEDURE SP_GET_RESERVAS_CLIENTES_HISTORIAL_ADMIN
AS
BEGIN
    SELECT 
        r.ID_reserva AS Id,
		r.Fecha_Reserva AS FechaCreacion,
		COALESCE(u.Nombre_Usuario, 'UsuarioEliminado') AS NombreUsuario,
		COALESCE(u.Apellido_Usuario, 'UsuarioEliminado') AS ApellidoUsuario,
		COALESCE(u.Telefono_Usuario, '0000000000') AS Celular,
        COALESCE(h.Fecha_Horario, '2000-01-01') AS FechaHorario, 
        COALESCE(h.Hora_Inicio, '00:00:00') AS HoraInicio, 
        COALESCE(h.Hora_Fin, '00:00:00') AS HoraFin,
        COALESCE(es.Nombre_Estado_Reserva, 'EstadoEliminado') AS Estado
    FROM 
        RESERVA r
    LEFT JOIN 
        HORARIO_DISPONIBLE h ON r.ID_horario_disponible = h.ID_horario_disponible
	LEFT JOIN 
        USUARIO u ON r.ID_usuario = u.ID_usuario
    LEFT JOIN 
        ESTADO_RESERVA es ON r.ID_estado_reserva = es.ID_estado_reserva 
	ORDER BY r.ID_reserva DESC
END;
-----------------------------------
--Listar pagos admin
CREATE PROCEDURE SP_GET_PAGOS_ADMIN
AS
BEGIN
	SELECT
	p.ID_pago AS Id,
	p.Fecha_Pago AS FechaPago,
	p.Monto_Pago AS Monto,
	COALESCE(u.Nombre_Usuario, 'UsuarioEliminado') AS NombreUsuario,
	COALESCE(u.Apellido_Usuario, 'UsuarioEliminado') AS ApellidoUsuario,
	COALESCE(r.ID_usuario, '0') AS IdUsuario,
	COALESCE(r.Fecha_Reserva, '2000-01-01 00:00:00') AS FechaCreacionReserva,
	COALESCE(r.ID_reserva, '0') AS IdReserva,
	COALESCE(h.Fecha_Horario, '2000-01-01') AS FechaHorario, 
    COALESCE(h.Hora_Inicio, '00:00:00') AS HoraInicio, 
    COALESCE(h.Hora_Fin, '00:00:00') AS HoraFin,
	CASE es.ID_estado_reserva
            WHEN 1 THEN 'PENDIENTE'
            WHEN 2 THEN 'CONFIRMADA'
            WHEN 3 THEN 'CANCELADA'
			ELSE 'Sin estado'
	END AS Estado
	FROM PAGO p
	LEFT JOIN 
		RESERVA r ON p.ID_reserva = r.ID_reserva
	LEFT JOIN 
		HORARIO_DISPONIBLE h ON r.ID_horario_disponible = h.ID_horario_disponible
	LEFT JOIN 
		USUARIO u ON r.ID_usuario = u.ID_usuario
	LEFT JOIN 
		ESTADO_RESERVA es ON r.ID_estado_reserva = es.ID_estado_reserva
	WHERE p.Fecha_Pago >= DATEADD(DAY, -14, GETDATE()) -- Filtra los pagos de los �ltimos 14 d�as
	ORDER BY p.Fecha_Pago DESC
END;
----------------------------
-- Historial completo de pagos
CREATE PROCEDURE SP_GET_ALL_PAGOS_ADMIN
AS
BEGIN
	SELECT
	p.ID_pago AS Id,
	p.Fecha_Pago AS FechaPago,
	p.Monto_Pago AS Monto,
	COALESCE(u.Nombre_Usuario, 'UsuarioEliminado') AS NombreUsuario,
	COALESCE(u.Apellido_Usuario, 'UsuarioEliminado') AS ApellidoUsuario,
	COALESCE(r.ID_usuario, '0') AS IdUsuario,
	COALESCE(r.Fecha_Reserva, '2000-01-01 00:00:00') AS FechaCreacionReserva,
	COALESCE(r.ID_reserva, '0') AS IdReserva,
	COALESCE(h.Fecha_Horario, '2000-01-01') AS FechaHorario, 
    COALESCE(h.Hora_Inicio, '00:00:00') AS HoraInicio, 
    COALESCE(h.Hora_Fin, '00:00:00') AS HoraFin,
	CASE es.ID_estado_reserva
            WHEN 1 THEN 'PENDIENTE'
            WHEN 2 THEN 'CONFIRMADA'
            WHEN 3 THEN 'CANCELADA'
			ELSE 'Sin estado'
	END AS Estado
	FROM PAGO p
	LEFT JOIN 
		RESERVA r ON p.ID_reserva = r.ID_reserva
	LEFT JOIN 
		HORARIO_DISPONIBLE h ON r.ID_horario_disponible = h.ID_horario_disponible
	LEFT JOIN 
		USUARIO u ON r.ID_usuario = u.ID_usuario
	LEFT JOIN 
		ESTADO_RESERVA es ON r.ID_estado_reserva = es.ID_estado_reserva
	ORDER BY p.Fecha_Pago DESC
END;
----------------------------------------------------------
----------------------------------------------------------
-- STORED PROCEDURES SIN APLICARSE...






