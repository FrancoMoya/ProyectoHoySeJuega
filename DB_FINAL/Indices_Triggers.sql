-- INDICES (Ayudan a mejorar la eficiencia de las consultas, haciéndolas más rápidas)

-- USUARIO
-- Listar usuarios por correo
CREATE INDEX idx_usuario_corre ON USUARIO (Correo_Usuario);

-- AUDITORIA
-- Listar auditorias por fecha
CREATE INDEX idx_auditoria_fecha ON AUDITORIA (Fecha_Auditoria);
-- Listar auditorias por seccion
CREATE INDEX idx_auditoria_seccion ON AUDITORIA (Seccion);

-- HORARIO_DISPONIBLE
-- Listar horarios por dia
CREATE INDEX idx_horarioDisponible_fecha ON HORARIO_DISPONIBLE (Fecha_Horario);
-- Listar horarios por dia y disponible TRUE
CREATE INDEX idx_horarioDisponible_fecha_disponible ON HORARIO_DISPONIBLE (Fecha_Horario) WHERE Disponible_Horario = 1;;
-- -- Listar horarios por dia y ID (Para JOINS entre RESERVA/EVENTO y HORARIO DISPONIBLE)
CREATE INDEX idx_horario_fecha_id ON HORARIO_DISPONIBLE (Fecha_Horario, ID_horario_disponible);
-- Fecha_Horario: Es la columna principal para el filtro.
-- ID_horario_disponible: Está incluido en el índice porque se utiliza para hacer el JOIN con RESERVA.

-- NOTIFICACION
-- Listar notificaciones por usuario
CREATE INDEX idx_notificacion_usuario ON NOTIFICACION (ID_usuario);

-- RESERVA
-- Listar todas las reservas por horario
CREATE INDEX idx_reserva_horario ON RESERVA (ID_horario_disponible);
-- Listar todas las reservas por usuario
CREATE INDEX idx_reserva_usuario ON RESERVA (ID_usuario);

-- EVENTO
-- Listar todos los eventos por horario
CREATE INDEX idx_evento_horario ON EVENTO (ID_horario_disponible);

-- PAGO
-- Listar todos los pagos por fecha
CREATE INDEX idx_pago_fecha ON PAGO (Fecha_Pago);



-- TRIGGERS (Disparadores de eventos)

-- RESERVA Y HORARIO_DISPONIBLE
-- Trigger para actualizar el horario a NO DISPONIBLE luego de CREAR UNA RESERVA
CREATE TRIGGER trg_UpdateReservaDisponibilidadHorario
ON RESERVA
AFTER INSERT
AS
BEGIN
    DECLARE @id_horario INT;

    SELECT @id_horario = ID_horario_disponible FROM inserted;

    UPDATE HORARIO_DISPONIBLE
    SET Disponible_Horario = 0 -- Marca como no disponible
    WHERE ID_horario_disponible = @id_horario;
END;

-- Trigger para actualizar el horario a DISPONIBLE luego de que cambie el estado de la reserva a CANCELADA
CREATE TRIGGER trg_UpdateReservaDisponibilidadEnCancelacion
ON RESERVA
AFTER UPDATE
AS
BEGIN
    DECLARE @id_horario_disponible INT;
    DECLARE @nuevo_estado_reserva INT;
    DECLARE @anterior_estado_reserva INT;

    -- Obtener el ID del horario disponible y los estados anterior y nuevo de la reserva
    SELECT @id_horario_disponible = inserted.ID_horario_disponible,
           @nuevo_estado_reserva = inserted.ID_estado_reserva,
           @anterior_estado_reserva = deleted.ID_estado_reserva
    FROM inserted
    JOIN deleted ON inserted.ID_reserva = deleted.ID_reserva;

    -- Si el estado cambia a CANCELADA (ID_estado_reserva = 3) y no estaba ya cancelada antes
    IF @nuevo_estado_reserva = 3 AND @anterior_estado_reserva <> 3
    BEGIN
        -- Actualizar el horario como disponible (Disponible_Horario = 1)
        UPDATE HORARIO_DISPONIBLE
        SET Disponible_Horario = 1
        WHERE ID_horario_disponible = @id_horario_disponible;
    END
END;


-- EVENTO Y HORARIO_DISPONIBLE
-- Trigger para actualizar el horario a NO DISPONIBLE luego de CREAR UN EVENTO
CREATE TRIGGER trg_UpdateEventoDisponibilidadHorario
ON EVENTO
AFTER INSERT
AS
BEGIN
    DECLARE @id_horario INT;

    SELECT @id_horario = ID_horario_disponible FROM inserted;

    UPDATE HORARIO_DISPONIBLE
    SET Disponible_Horario = 0 -- Marca como no disponible
    WHERE ID_horario_disponible = @id_horario;
END;

-- Trigger para actualizar el horario a DISPONIBLE luego de que cambie el estado del evento a CANCELADA
CREATE TRIGGER trg_UpdateEventoDisponibilidadEnCancelacion
ON EVENTO
AFTER UPDATE
AS
BEGIN
    DECLARE @id_horario_disponible INT;
    DECLARE @nuevo_estado_reserva INT;
    DECLARE @anterior_estado_reserva INT;

    -- Obtener el ID del horario disponible y los estados anterior y nuevo de la reserva
    SELECT @id_horario_disponible = inserted.ID_horario_disponible,
           @nuevo_estado_reserva = inserted.ID_estado_reserva,
           @anterior_estado_reserva = deleted.ID_estado_reserva
    FROM inserted
    JOIN deleted ON inserted.ID_evento = deleted.ID_evento;

    -- Si el estado cambia a CANCELADA (ID_estado_reserva = 3) y no estaba ya cancelada antes
    IF @nuevo_estado_reserva = 3 AND @anterior_estado_reserva <> 3
    BEGIN
        -- Actualizar el horario como disponible (Disponible_Horario = 1)
        UPDATE HORARIO_DISPONIBLE
        SET Disponible_Horario = 1
        WHERE ID_horario_disponible = @id_horario_disponible;
    END
END;
