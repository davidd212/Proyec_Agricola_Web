-- Crear tabla Mensajes para el formulario de contacto
CREATE TABLE Mensajes (
    MensajeID int IDENTITY(1,1) PRIMARY KEY,
    Nombre nvarchar(100) NOT NULL,
    Email nvarchar(200) NOT NULL,
    Telefono nvarchar(20) NULL,
    Asunto nvarchar(200) NULL,
    MensajeTexto nvarchar(max) NOT NULL,
    FechaEnvio datetime NOT NULL DEFAULT GETDATE(),
    Leido bit NOT NULL DEFAULT 0,
    FechaLeido datetime NULL
);
GO