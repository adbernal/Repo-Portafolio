using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.DataAccess.Client;
using System.Data;

namespace Web_ViajesOnTour.Sesion
{
    public partial class portal : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            lbl_rut.Text = Session["rut"].ToString();
             string strConexion = "DATA SOURCE=localhost:1521/xe;USER ID=ADOLFO ; Password= 123";
             string sql = "SELECT nombre FROM APODERADOS where rut='"+ lbl_rut.Text +"'";
             OracleConnection con = new OracleConnection(strConexion);
             con.Open();
             OracleCommand sqlCmd = new OracleCommand(sql, con);
            sqlCmd.CommandType = System.Data.CommandType.Text;

            OracleDataAdapter sqlDa = new OracleDataAdapter();

            sqlDa.SelectCommand = sqlCmd;

            DataSet DsRetorno = new DataSet();

            sqlDa.Fill(DsRetorno);

            if (DsRetorno.Tables.Count > 0)
            {
                lbl_nombre.Text = DsRetorno.Tables[0].Rows[0]["nombre"].ToString();
            }
            else
            {
                Response.Redirect("../Login/Login_apoderado.aspx");
            }

            con.Close();

        }
    }
}