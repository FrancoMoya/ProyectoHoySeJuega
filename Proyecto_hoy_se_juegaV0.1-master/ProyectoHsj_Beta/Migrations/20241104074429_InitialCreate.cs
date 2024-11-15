using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoHsj_Beta.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ACCION_REALIZADA",
                columns: table => new
                {
                    ID_accion_realizada = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo_Accion_Realizada = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ACCION_R__2DC6E876AE3CA4B6", x => x.ID_accion_realizada);
                });

            migrationBuilder.CreateTable(
                name: "CANCHA",
                columns: table => new
                {
                    ID_cancha = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Cancha = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    Ubicacion_Cancha = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CANCHA__A2D3DBCF8F513BBF", x => x.ID_cancha);
                });

            migrationBuilder.CreateTable(
                name: "ESTADO_RESERVA",
                columns: table => new
                {
                    ID_estado_reserva = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Estado_Reserva = table.Column<string>(type: "varchar(25)", unicode: false, maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ESTADO_R__746A84F0EE561664", x => x.ID_estado_reserva);
                });

            migrationBuilder.CreateTable(
                name: "HistorialReservas",
                columns: table => new
                {
                    IdHistorialReserva = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdReserva = table.Column<int>(type: "int", nullable: false),
                    ID_usuario = table.Column<int>(type: "int", nullable: false),
                    FechaReserva = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "time", nullable: false),
                    EstadoReserva = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NombreUsuario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TelefonoUsuario = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialReservas", x => x.IdHistorialReserva);
                });

            migrationBuilder.CreateTable(
                name: "PERMISO",
                columns: table => new
                {
                    ID_permiso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Permiso = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PERMISO__74B1E219FA556AF3", x => x.ID_permiso);
                });

            migrationBuilder.CreateTable(
                name: "ROL",
                columns: table => new
                {
                    ID_rol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Rol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ROL__182A5412E3FEA872", x => x.ID_rol);
                });

            migrationBuilder.CreateTable(
                name: "TITULO_NOTIFICACION",
                columns: table => new
                {
                    ID_titulo_notificacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo_Notificacion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TITULO_N__DA36445EB454B7E0", x => x.ID_titulo_notificacion);
                });

            migrationBuilder.CreateTable(
                name: "TITULO_REPORTE",
                columns: table => new
                {
                    ID_titulo_reporte = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo_Reporte = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TITULO_R__5F9C0CCDB6AEF5F9", x => x.ID_titulo_reporte);
                });

            migrationBuilder.CreateTable(
                name: "HORARIO_DISPONIBLE",
                columns: table => new
                {
                    ID_horario_disponible = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_cancha = table.Column<int>(type: "int", nullable: false),
                    Fecha_Horario = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(getdate())"),
                    Hora_Inicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    Hora_Fin = table.Column<TimeOnly>(type: "time", nullable: false),
                    Disponible_Horario = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HORARIO___7D5601B788E66056", x => x.ID_horario_disponible);
                    table.ForeignKey(
                        name: "FK__HORARIO_D__ID_ca__5441852A",
                        column: x => x.ID_cancha,
                        principalTable: "CANCHA",
                        principalColumn: "ID_cancha");
                });

            migrationBuilder.CreateTable(
                name: "PERMISO_ROL",
                columns: table => new
                {
                    ID_rol = table.Column<int>(type: "int", nullable: false),
                    ID_permiso = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PERMISO___9F614A336556D35C", x => new { x.ID_rol, x.ID_permiso });
                    table.ForeignKey(
                        name: "FK__PERMISO_R__ID_pe__45F365D3",
                        column: x => x.ID_permiso,
                        principalTable: "PERMISO",
                        principalColumn: "ID_permiso");
                    table.ForeignKey(
                        name: "FK__PERMISO_R__ID_ro__46E78A0C",
                        column: x => x.ID_rol,
                        principalTable: "ROL",
                        principalColumn: "ID_rol");
                });

            migrationBuilder.CreateTable(
                name: "USUARIO",
                columns: table => new
                {
                    ID_usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Usuario = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Apellido_Usuario = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Correo_Usuario = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Contrasenia_Usuario = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Telefono_Usuario = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    PasswordResetToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "datetime", nullable: true),
                    EmailConfirmationToken = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: true),
                    ID_rol = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__USUARIO__DF3D425202CA1D01", x => x.ID_usuario);
                    table.ForeignKey(
                        name: "FK__USUARIO__ID_rol__412EB0B6",
                        column: x => x.ID_rol,
                        principalTable: "ROL",
                        principalColumn: "ID_rol");
                });

            migrationBuilder.CreateTable(
                name: "REPORTE",
                columns: table => new
                {
                    ID_reporte = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Mes_Reporte = table.Column<byte>(type: "tinyint", nullable: false),
                    Anio_Reporte = table.Column<short>(type: "smallint", nullable: false),
                    Usuarios_Registrados_Reporte = table.Column<int>(type: "int", nullable: true),
                    Reservas_Realizadas_Reporte = table.Column<int>(type: "int", nullable: true),
                    Fecha_De_Reporte = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Descripcion_Reporte = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    ID_titulo_reporte = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__REPORTE__41AEEB64E2FCCC33", x => x.ID_reporte);
                    table.ForeignKey(
                        name: "FK__REPORTE__ID_titu__3A81B327",
                        column: x => x.ID_titulo_reporte,
                        principalTable: "TITULO_REPORTE",
                        principalColumn: "ID_titulo_reporte");
                });

            migrationBuilder.CreateTable(
                name: "AUDITORIA",
                columns: table => new
                {
                    ID_auditoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_usuario = table.Column<int>(type: "int", nullable: false),
                    Fecha_Auditoria = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    Descripcion_De_Accion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ID_accion_realizada = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AUDITORI__F6FFFB8CE3101847", x => x.ID_auditoria);
                    table.ForeignKey(
                        name: "FK__AUDITORIA__ID_ac__4CA06362",
                        column: x => x.ID_accion_realizada,
                        principalTable: "ACCION_REALIZADA",
                        principalColumn: "ID_accion_realizada");
                    table.ForeignKey(
                        name: "FK__AUDITORIA__ID_us__4D94879B",
                        column: x => x.ID_usuario,
                        principalTable: "USUARIO",
                        principalColumn: "ID_usuario");
                });

            migrationBuilder.CreateTable(
                name: "RESERVA",
                columns: table => new
                {
                    ID_reserva = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_usuario = table.Column<int>(type: "int", nullable: false),
                    ID_horario_disponible = table.Column<int>(type: "int", nullable: false),
                    Fecha_Reserva = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    ID_estado_reserva = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RESERVA__CD692CB066E0B8CB", x => x.ID_reserva);
                    table.ForeignKey(
                        name: "FK__RESERVA__ID_esta__59FA5E80",
                        column: x => x.ID_estado_reserva,
                        principalTable: "ESTADO_RESERVA",
                        principalColumn: "ID_estado_reserva");
                    table.ForeignKey(
                        name: "FK__RESERVA__ID_hora__5BE2A6F2",
                        column: x => x.ID_horario_disponible,
                        principalTable: "HORARIO_DISPONIBLE",
                        principalColumn: "ID_horario_disponible");
                    table.ForeignKey(
                        name: "FK__RESERVA__ID_usua__5AEE82B9",
                        column: x => x.ID_usuario,
                        principalTable: "USUARIO",
                        principalColumn: "ID_usuario");
                });

            migrationBuilder.CreateTable(
                name: "NOTIFICACION",
                columns: table => new
                {
                    ID_notificacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_usuario = table.Column<int>(type: "int", nullable: false),
                    ID_reserva = table.Column<int>(type: "int", nullable: false),
                    Mensaje_Notificacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fecha_Envio_Notificacion = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    ID_titulo_notificacion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NOTIFICA__99BC7E5EC04B01F2", x => x.ID_notificacion);
                    table.ForeignKey(
                        name: "FK__NOTIFICAC__ID_re__6754599E",
                        column: x => x.ID_reserva,
                        principalTable: "RESERVA",
                        principalColumn: "ID_reserva");
                    table.ForeignKey(
                        name: "FK__NOTIFICAC__ID_ti__656C112C",
                        column: x => x.ID_titulo_notificacion,
                        principalTable: "TITULO_NOTIFICACION",
                        principalColumn: "ID_titulo_notificacion");
                    table.ForeignKey(
                        name: "FK__NOTIFICAC__ID_us__66603565",
                        column: x => x.ID_usuario,
                        principalTable: "USUARIO",
                        principalColumn: "ID_usuario");
                });

            migrationBuilder.CreateTable(
                name: "PAGO",
                columns: table => new
                {
                    ID_pago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_reserva = table.Column<int>(type: "int", nullable: false),
                    Monto_Pago = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Fecha_Pago = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PAGO__808903EC47A081C7", x => x.ID_pago);
                    table.ForeignKey(
                        name: "FK__PAGO__ID_reserva__5FB337D6",
                        column: x => x.ID_reserva,
                        principalTable: "RESERVA",
                        principalColumn: "ID_reserva");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AUDITORIA_ID_accion_realizada",
                table: "AUDITORIA",
                column: "ID_accion_realizada");

            migrationBuilder.CreateIndex(
                name: "IX_AUDITORIA_ID_usuario",
                table: "AUDITORIA",
                column: "ID_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_HORARIO_DISPONIBLE_ID_cancha",
                table: "HORARIO_DISPONIBLE",
                column: "ID_cancha");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICACION_ID_reserva",
                table: "NOTIFICACION",
                column: "ID_reserva");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICACION_ID_titulo_notificacion",
                table: "NOTIFICACION",
                column: "ID_titulo_notificacion");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICACION_ID_usuario",
                table: "NOTIFICACION",
                column: "ID_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_PAGO_ID_reserva",
                table: "PAGO",
                column: "ID_reserva");

            migrationBuilder.CreateIndex(
                name: "IX_PERMISO_ROL_ID_permiso",
                table: "PERMISO_ROL",
                column: "ID_permiso");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTE_ID_titulo_reporte",
                table: "REPORTE",
                column: "ID_titulo_reporte");

            migrationBuilder.CreateIndex(
                name: "IX_RESERVA_ID_estado_reserva",
                table: "RESERVA",
                column: "ID_estado_reserva");

            migrationBuilder.CreateIndex(
                name: "IX_RESERVA_ID_horario_disponible",
                table: "RESERVA",
                column: "ID_horario_disponible");

            migrationBuilder.CreateIndex(
                name: "IX_RESERVA_ID_usuario",
                table: "RESERVA",
                column: "ID_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_ID_rol",
                table: "USUARIO",
                column: "ID_rol");

            migrationBuilder.CreateIndex(
                name: "UQ__USUARIO__A7126311EFE6490D",
                table: "USUARIO",
                column: "Correo_Usuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AUDITORIA");

            migrationBuilder.DropTable(
                name: "HistorialReservas");

            migrationBuilder.DropTable(
                name: "NOTIFICACION");

            migrationBuilder.DropTable(
                name: "PAGO");

            migrationBuilder.DropTable(
                name: "PERMISO_ROL");

            migrationBuilder.DropTable(
                name: "REPORTE");

            migrationBuilder.DropTable(
                name: "ACCION_REALIZADA");

            migrationBuilder.DropTable(
                name: "TITULO_NOTIFICACION");

            migrationBuilder.DropTable(
                name: "RESERVA");

            migrationBuilder.DropTable(
                name: "PERMISO");

            migrationBuilder.DropTable(
                name: "TITULO_REPORTE");

            migrationBuilder.DropTable(
                name: "ESTADO_RESERVA");

            migrationBuilder.DropTable(
                name: "HORARIO_DISPONIBLE");

            migrationBuilder.DropTable(
                name: "USUARIO");

            migrationBuilder.DropTable(
                name: "CANCHA");

            migrationBuilder.DropTable(
                name: "ROL");
        }
    }
}
