using System;
using System.Data.SQLite;
using NITGEN.SDK.NBioBSP;

namespace ControlDeHuellaSol
{
    public class HuellaManager
    {
        private readonly NBioAPI m_NBioAPI;
        private readonly string connectionString = "Data Source=ControlAcceso.db;Version=3;";

        public HuellaManager()
        {
            m_NBioAPI = new NBioAPI();
            CrearBaseDeDatos();
        }

        // 📌 🔹 Método para crear la base de datos si no existe
        private void CrearBaseDeDatos()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "CREATE TABLE IF NOT EXISTS Usuarios (ID INTEGER PRIMARY KEY AUTOINCREMENT, Nombre TEXT NOT NULL, Huella TEXT NOT NULL);";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        // 📌 🔹 Capturar huella y convertir a texto
        public string CapturarHuella()
        {
            uint ret;
            NBioAPI.Type.HFIR hCapturedFIR;
            ret = m_NBioAPI.Capture(NBioAPI.Type.FIR_PURPOSE.VERIFY, out hCapturedFIR, NBioAPI.Type.TIMEOUT.DEFAULT, null, null);

            if (ret == NBioAPI.Error.NONE)
            {
                NBioAPI.Type.FIR_TEXTENCODE textFIR;
                ret = m_NBioAPI.GetTextFIRFromHandle(hCapturedFIR, out textFIR, true);
                return ret == NBioAPI.Error.NONE ? textFIR.TextFIR : null;
            }
            return null;
        }

        // 📌 🔹 Guardar la huella en la base de datos
        public bool GuardarHuella(string nombreUsuario, string huellaTexto)
        {
            if (string.IsNullOrEmpty(nombreUsuario) || string.IsNullOrEmpty(huellaTexto)) return false;

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Usuarios (Nombre, Huella) VALUES (@Nombre, @Huella)";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", nombreUsuario);
                    cmd.Parameters.AddWithValue("@Huella", huellaTexto);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // 📌 🔹 Verificar si la huella existe en la base de datos
        public bool VerificarHuella(string huellaTexto)
        {
            if (string.IsNullOrEmpty(huellaTexto)) return false;

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Huella FROM Usuarios";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (huellaTexto == reader["Huella"].ToString())
                                return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
