-- INDICES (Ayudan a mejorar la eficiencia de las consultas, haci�ndolas m�s r�pidas)

-- AUDITORIA
CREATE INDEX IX_AUDITORIA_FECHA ON AUDITORIA (Fecha_Auditoria);
--RESERVA
CREATE INDEX IX_RESERVA_ID_USUARIO ON RESERVA (ID_usuario);
CREATE INDEX IX_RESERVA_ID_HORARIO_DISPONIBLE ON RESERVA (ID_horario_disponible);
CREATE INDEX IX_RESERVA_ID_ESTADO_RESERVA ON RESERVA (ID_estado_reserva);
--HORARIO_DISPONIBLE
CREATE INDEX IX_HORARIO_DISPONIBLE_ID_HORARIO_DISPONIBLE ON HORARIO_DISPONIBLE (ID_horario_disponible);
CREATE INDEX IX_HORARIO_DISPONIBLE_FECHA ON HORARIO_DISPONIBLE (Fecha_Horario) WHERE Disponible_Horario = 1;
--USUARIO
CREATE INDEX IX_USUARIO_ID_USUARIO ON USUARIO (ID_usuario);
CREATE INDEX IX_USUARIO_EMAIL ON USUARIO (Correo_Usuario);
--ESTADO_RESERVA
CREATE INDEX IX_ESTADO_RESERVA_ID_ESTADO_RESERVA ON ESTADO_RESERVA (ID_estado_reserva);
--PAGO
CREATE INDEX IX_PAGO_ID_RESERVA ON PAGO (ID_reserva);
CREATE INDEX IX_PAGO_FECHA_PAGO ON PAGO (Fecha_Pago);
--EVENTO
CREATE INDEX IX_EVENTO_ID_HORARIO_DISPONIBLE ON EVENTO (ID_horario_disponible);
