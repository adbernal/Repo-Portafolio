using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Text;
using Oracle.DataAccess.Client;
/// <summary>
/// Clase de Conexión y empleo de la DB Oracle.
/// ODP.NET Oracle managed provider
/// </summary>
public class DBOracle : IDisposable
{

    // Variables.
    private OracleConnection ora_Connection;
    private OracleTransaction ora_Transaction;
    public OracleDataReader ora_DataReader;

    private struct stConnDB
    {
        public string CadenaConexion;
        public string ErrorDesc;
        public int ErrorNum;
    }
    private stConnDB info;

    // Indica el numero de intentos de conectar a la BD sin exito.
    public byte ora_intentos = 0;

    #region "Propiedades"

    /// <summary>
    /// Devuelve la descripcion de error de la clase.
    /// </summary>
    public string ErrDesc
    {
        get { return this.info.ErrorDesc; }
    }

    /// <summary>
    /// Devuelve el numero de error de la clase.
    /// </summary>
    public string ErrNum
    {
        get { return info.ErrorNum.ToString(); }
    }

    #endregion


    /// <summary>
    /// Constructor.
    /// </summary>
    public DBOracle(string Servidor, string Usuario, string Password)
    {
        // Creamos la cadena de conexión de la base de datos.
        info.CadenaConexion = string.Format("Data Source={0};User Id={1};Password={2};", Servidor, Usuario, Password);

        // Instanciamos objeto conecction.
        ora_Connection = new OracleConnection();

    }

    /// <summary>
    /// Implement IDisposable.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose de la clase.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Liberamos objetos manejados.
        }

        try
        {
            // Liberamos los obtetos no manejados.
            if (ora_DataReader != null)
            {
                ora_DataReader.Close();
                ora_DataReader.Dispose();
            }

            // Cerramos la conexión a DB.
            if (!Desconectar())
            {
                // Grabamos Log de Error...
            }

        }
        catch (Exception ex)
        {
            // Asignamos error.
            AsignarError(ref ex);
        }

    }


    /// <summary>
    /// Destructor.
    /// </summary>
    ~DBOracle()
    {
        Dispose(false);
    }


    /// <summary>
    /// Se conecta a una base de datos de Oracle.
    /// </summary>
    /// <returns>True si se conecta bien.</returns>
    private bool Conectar()
    {

        bool ok = false;

        try
        {
            if (ora_Connection != null)
            {
                // Fijamos la cadena de conexión de la base de datos.
                ora_Connection.ConnectionString = info.CadenaConexion;
                ora_Connection.Open();
                ok = true;
            }
        }
        catch (Exception ex)
        {
            // Desconectamos y liberamos memoria.
            Desconectar();
            // Asignamos error.
            AsignarError(ref ex);
            // Asignamos error de función
            ok = false;
        }

        return ok;

    }


    /// <summary>
    /// Cierra la conexión de BBDD.
    /// </summary>
    public bool Desconectar()
    {
        try
        {
            // Cerramos la conexion
            if (ora_Connection != null)
            {
                if (ora_Connection.State != ConnectionState.Closed)
                {
                    ora_Connection.Close();
                }
            }
            // Liberamos su memoria.
            ora_Connection.Dispose();
            return true;
        }
        catch (Exception ex)
        {
            AsignarError(ref ex);
            return false;
        }
    }


    /// <summary>
    /// Ejecuta un procedimiento almacenado de Oracle.
    /// </summary>
    /// <param name="oraCommand">Objeto Command con los datos del procedimiento.</param>
    /// <param name="SpName">Nombre del procedimiento almacenado.</param>
    /// <returns>True si el procedimiento se ejecuto bien.</returns>
    public bool EjecutaSP(ref OracleCommand OraCommand, string SpName)
    {

        bool ok = true;

        try
        {
            // Si no esta conectado, se conecta.
            if (!IsConected())
            {
                ok = Conectar();
            }

            if (ok)
            {
                OraCommand.Connection = ora_Connection;
                OraCommand.CommandText = SpName;
                OraCommand.CommandType = CommandType.StoredProcedure;
                OraCommand.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            AsignarError(ref ex);
            ok = false;
        }

        return ok;

    }


    /// <summary>
    /// Ejecuta una sql que rellenar un DataReader (sentencia select).
    /// </summary>
    /// <param name="SqlQuery">sentencia sql a ejecutar</param>
    /// <returns></returns> 
    public bool EjecutaSQL(string SqlQuery)
    {

        bool ok = true;

        OracleCommand ora_Command = new OracleCommand();

        try
        {

            // Si no esta conectado, se conecta.
            if (!IsConected())
            {
                ok = Conectar();
            }

            if (ok)
            {
                // Cerramos cursores abiertos, para evitar el error ORA-1000
                if ((ora_DataReader != null))
                {
                    ora_DataReader.Close();
                    ora_DataReader.Dispose();
                }

                ora_Command.Connection = ora_Connection;
                ora_Command.CommandType = CommandType.Text;
                ora_Command.CommandText = SqlQuery;

                // Ejecutamos sql.
                ora_DataReader = ora_Command.ExecuteReader();
            }

        }
        catch (Exception ex)
        {
            AsignarError(ref ex);
            ok = false;
        }
        finally
        {
            if (ora_Command != null)
            {
                ora_Command.Dispose();
            }
        }

        return ok;

    }



    /// <summary>
    /// Ejecuta una sql que no devuelve datos (update, delete, insert).
    /// </summary>
    /// <param name="SqlQuery">sentencia sql a ejecutar</param>
    /// <param name="FilasAfectadas">Fila afectadas por la sentencia SQL</param>
    /// <returns></returns>
    public bool EjecutaSQL(string SqlQuery, ref int FilasAfectadas)
    {

        bool ok = true;
        OracleCommand ora_Command = new OracleCommand();

        try
        {

            // Si no esta conectado, se conecta.
            if (!IsConected())
            {
                ok = Conectar();
            }

            if (ok)
            {
                ora_Transaction = ora_Connection.BeginTransaction();
                ora_Command = ora_Connection.CreateCommand();
                ora_Command.CommandType = CommandType.Text;
                ora_Command.CommandText = SqlQuery;
                FilasAfectadas = ora_Command.ExecuteNonQuery();
                ora_Transaction.Commit();
            }

        }
        catch (Exception ex)
        {
            // Hacemos rollback.
            ora_Transaction.Rollback();
            AsignarError(ref ex);
            ok = false;
        }
        finally
        {
            // Recolectamos objetos para liberar su memoria.
            if (ora_Command != null)
            {
                ora_Command.Dispose();
            }
        }

        return ok;

    }


    /// <summary>
    /// Captura Excepciones
    /// </summary>
    /// <param name="ex">Excepcion producida.</param>
    private void AsignarError(ref Exception ex)
    {
        // Si es una excepcion de Oracle.
        if (ex is OracleException)
        {
            info.ErrorNum = ((OracleException)ex).Number;
            info.ErrorDesc = ex.Message;
        }
        else
        {
            info.ErrorNum = 0;
            info.ErrorDesc = ex.Message;
        }
        // Grabamos Log de Error...
    }



    /// <summary>
    /// Devuelve el estado de la base de datos
    /// </summary>
    /// <returns>True si esta conectada.</returns>
    public bool IsConected()
    {

        bool ok = false;

        try
        {
            // Si el objeto conexion ha sido instanciado
            if (ora_Connection != null)
            {
                // Segun el estado de la Base de Datos.
                switch (ora_Connection.State)
                {
                    case ConnectionState.Closed:
                    case ConnectionState.Broken:
                    case ConnectionState.Connecting:
                        ok = false;
                        break;
                    case ConnectionState.Open:
                    case ConnectionState.Fetching:
                    case ConnectionState.Executing:
                        ok = true;
                        break;
                }
            }
            else
            {
                ok = false;
            }

        }
        catch (Exception ex)
        {
            AsignarError(ref ex);
            ok = false;
        }

        return ok;

    }

}