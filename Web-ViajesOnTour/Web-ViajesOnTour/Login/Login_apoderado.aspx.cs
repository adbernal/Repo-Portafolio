using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.DataAccess.Client;
using System.Data;




namespace Web_ViajesOnTour.Login
{
    public partial class Login_cliente : System.Web.UI.Page
    {




        protected void Page_Load(object sender, EventArgs e)
        {


        }
        


        protected void btn_login_Click(object sender, EventArgs e)
        {
            string oradb = "DATA SOURCE=localhost:1521/xe;USER ID=ADOLFO ; Password= 123";

            OracleConnection conn = new OracleConnection(oradb); // C#

            conn.Open();
            DataSet ds = new DataSet();
            OracleCommand cmd = new OracleCommand();

            cmd = new OracleCommand("SELECT rut,pass,nombre FROM APODERADOS where rut=:user_u and pass=:pass_u", conn);
            cmd.Parameters.Add(new OracleParameter(":user_u", txt_user.Text));
            cmd.Parameters.Add(new OracleParameter(":pass_u", txt_pass.Text));
            

            OracleDataAdapter da = new OracleDataAdapter(cmd);
            da.Fill(ds);
            int i = ds.Tables[0].Rows.Count;

            if (i == 1)
            {

                Session["rut"] = txt_user.Text;
                
                Response.Redirect("../Sesion/portal.aspx");
                Session.RemoveAll();

               

                
            }
            else 
            {
                lbl_si.Text = "NOOOOO2";
            }
            
        }
    }
}
    
