USE HOY_SE_JUEGA;
GO

-- STORED PROCEDURES

-- CREAR AUDITORIA (PARAMETROS NECESARIOS: ID_usuario[INT], Seccion[VARCHAR(50)], Descripcion_De_Accion [NVARCHAR(1000)], ID_accion_realizada[INT])
CREATE PROCEDURE SP_AUDITORIA
	@ID_usuario INT,
	@Seccion VARCHAR(50),
	@Descripccion_De_Accion NVARCHAR(1000),
	@ID_accion_realizada INT
AS
BEGIN
	DECLARE @Fecha_Auditoria DATETIME = GETDATE();
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

--Trae las reservas en estado pendiente desde hace m�s de 10 minutos
CREATE PROCEDURE SP_GET_RESERVAS_PENDIENTES
AS
BEGIN
	SELECT ID_reserva FROM RESERVA 
	WHERE ID_estado_reserva = 1 
	AND DATEDIFF(MINUTE, Fecha_Reserva, GETDATE()) >= 10;
END;

--

CREATE PROCEDURE SP_CANCELAR_RESERVAS
    @ID_reserva INT
AS
BEGIN
    --Actualizar el estado de la reserva a 'cancelada'
    UPDATE RESERVA
    SET ID_estado_reserva = 3
    WHERE ID_reserva = @ID_reserva;
	--Obtener el ID del horario disponible asociado a la reserva cancelada
    DECLARE @ID_horario_disponible INT;
    SELECT @ID_horario_disponible = ID_horario_disponible
    FROM RESERVA
    WHERE ID_reserva = @ID_reserva;
	--Marcar el horario como disponible
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
	DECLARE @Fecha_Reserva DATETIME = GETDATE();
	DECLARE @ID_estado_reserva INT = 1;
	INSERT INTO RESERVA(ID_usuario, Fecha_Reserva, ID_horario_disponible, ID_estado_reserva)
    VALUES (@ID_usuario, @Fecha_Reserva, @ID_horario_disponible, @ID_estado_reserva);
	SET @ID_reserva = SCOPE_IDENTITY();  -- Esto devuelve el �ltimo ID insertado
	--CAMBIAR EL HORARIO A NO DISPONIBLE
	UPDATE HORARIO_DISPONIBLE
    SET Disponible_Horario = 0
    WHERE ID_horario_disponible = @ID_horario_disponible;
END;

---------------------
-- SP PARA LISTAR LAS RESERVAS DEL USUARIO
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
        COALESCE(h.Hora_Fin, '00:00:00') AS HoraFin  
    FROM 
        RESERVA r
    LEFT JOIN 
        HORARIO_DISPONIBLE h ON r.ID_horario_disponible = h.ID_horario_disponible  -- Usamos LEFT JOIN para incluir reservas aunque no tengan un horario relacionado
    LEFT JOIN 
        ESTADO_RESERVA e ON r.ID_estado_reserva = e.ID_estado_reserva  -- Usamos LEFT JOIN para incluir reservas aunque no tengan un estado relacionado
    WHERE r.ID_usuario = @ID_usuario
    ORDER BY
        h.Fecha_Horario DESC, h.Hora_Inicio DESC, h.Hora_Fin DESC
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
END;



----------------------------------------------------------
----------------------------------------------------------
-- STORED PROCEDURES SIN APLICARSE



