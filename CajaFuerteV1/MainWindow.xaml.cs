using Azure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

// Excel export
using OfficeOpenXml;
using OfficeOpenXml.ExternalReferences;
using System.IO;


namespace CajaFuerteV1
{
    public partial class MainWindow : Window
    {
        // lista para almacenar labels de unidades de denominaciones en retiro y deposito

        List<Label> labelsTotal = new List<Label>();
        List<Label> labels100 = new List<Label>();
        List<Label> labels200 = new List<Label>();
        List<Label> labels500 = new List<Label>();
        List<Label> labels1000 = new List<Label>();
        List<Label> labels2000 = new List<Label>();
        List<Label> labels10000 = new List<Label>();
        List<Label> labels20000 = new List<Label>();

        // lista para almacenar los text box de unidades a retirar
        List<TextBox> txtBoxUnid100 = new List<TextBox>();
        List<TextBox> txtBoxUnid200 = new List<TextBox>();
        List<TextBox> txtBoxUnid500 = new List<TextBox>();
        List<TextBox> txtBoxUnid1000 = new List<TextBox>();
        List<TextBox> txtBoxUnid2000 = new List<TextBox>();
        List<TextBox> txtBoxUnid10000 = new List<TextBox>();
        List<TextBox> txtBoxUnid20000 = new List<TextBox>();

        //listas para texts boxes de retiro y deposito
        List<TextBox> txtBoxesRet = new List<TextBox>();
        List<TextBox> txtBoxesDep = new List<TextBox>();

        bool sesionIniciada = false;
        //private const string strConexion = "Data Source=MSI\\SQLEXPRESS;Initial Catalog=CajaFuerte;Integrated Security=True;Encrypt=False";
        //private const string strConexion = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\alex_\\source\\repos\\CajaFuerteV1\\CajaFuerteV1\\DataBase\\CajaFuerteDB.mdf;Integrated Security = True";

        //private const string strConexion = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\DataBase\CajaFuerteDB.mdf;Integrated Security=True";

        //private const string strConexion = @"Data Source=(LocalDB)\DataBase\CajaFuerteDB.mdf;Integrated Security=True";

        private string strConexion = ConfigurationManager.ConnectionStrings["CajaFuerteV1.Properties.Settings.CajaFuerteConnectionString"].ConnectionString;

        int idUsuario;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                // agregamos los names de los labels a la lista
                labelsTotal.Add(lblRetSaldoTotal); labelsTotal.Add(lblDepSaldoTotal);
                labels100.Add(lblRetUnid100); labels100.Add(lblDepUnid100);
                labels200.Add(lblRetUnid200); labels200.Add(lblDepUnid200);
                labels500.Add(lblRetUnid500); labels500.Add(lblDepUnid500);
                labels1000.Add(lblRetUnid1000); labels1000.Add(lblDepUnid1000);
                labels2000.Add(lblRetUnid2000); labels2000.Add(lblDepUnid2000);
                labels10000.Add(lblRetUnid10000); labels10000.Add(lblDepUnid10000);
                labels20000.Add(lblRetUnid20000); labels20000.Add(lblDepUnid20000);


                // agregamos los names de los text box de unidades
                txtBoxUnid100.Add(txtBoxRetUnid100); txtBoxUnid100.Add(txtBoxDepUnid100);
                txtBoxUnid200.Add(txtBoxRetUnid200); txtBoxUnid200.Add(txtBoxDepUnid200);
                txtBoxUnid500.Add(txtBoxRetUnid500); txtBoxUnid500.Add(txtBoxDepUnid500);
                txtBoxUnid1000.Add(txtBoxRetUnid1000); txtBoxUnid1000.Add(txtBoxDepUnid1000);
                txtBoxUnid2000.Add(txtBoxRetUnid2000); txtBoxUnid2000.Add(txtBoxDepUnid2000);
                txtBoxUnid10000.Add(txtBoxRetUnid10000); txtBoxUnid10000.Add(txtBoxDepUnid10000);
                txtBoxUnid20000.Add(txtBoxRetUnid20000); txtBoxUnid20000.Add(txtBoxDepUnid20000);

                
                //creamos los objetos textBox


                txtBoxesRet.Add(txtBoxRetUnid100); txtBoxesRet.Add(txtBoxRetUnid200); txtBoxesRet.Add(txtBoxRetUnid500); txtBoxesRet.Add(txtBoxRetUnid1000);
                txtBoxesRet.Add(txtBoxRetUnid2000); txtBoxesRet.Add(txtBoxRetUnid10000); txtBoxesRet.Add(txtBoxRetUnid20000);

                txtBoxesDep.Add(txtBoxDepUnid100); txtBoxesDep.Add(txtBoxDepUnid200); txtBoxesDep.Add(txtBoxDepUnid500); txtBoxesDep.Add(txtBoxDepUnid1000);
                txtBoxesDep.Add(txtBoxDepUnid2000); txtBoxesDep.Add(txtBoxDepUnid10000); txtBoxesDep.Add(txtBoxDepUnid20000);
                
                // iniciamos siempre en incognito
                estadoIncognito();
            }
            catch (Exception ex) { MessageBox.Show("Error al inicializar la App " + ex.Message); }


        }

        private void Depositar()
        {
            if (sesionIniciada)
            {
                if (validarTxtBoxesNullsDeposito())
                {
                    if (warningOperacion() == true)
                    {
                        // variables
                        string tipoOperacion = "Depósito";

                        // obtengo los datos de la operacion anterior

                        SqlConnection conexion = abrirConexionSQL(strConexion);

                        string queryConsultaDatosPasados = "SELECT *\r\nFROM [Operacion-]\r\nWHERE ID_Operacion = (SELECT MAX(ID_Operacion) FROM [Operacion-]);\r\n";

                        SqlCommand cmdConsultaDatosPasados = new SqlCommand(queryConsultaDatosPasados, conexion);

                        SqlDataReader lector = cmdConsultaDatosPasados.ExecuteReader();

                        decimal ultimoSaldo = 0;


                        if (lector.Read())
                        {

                            int ultimoId = lector.GetInt32(0);
                            int ultimocUsuario = lector.GetInt32(1);
                            string ultimoTipoOperacion = lector.GetString(2);
                            ultimoSaldo = lector.GetDecimal(3);
                            string ultimaFecha = lector.GetDateTime(4).ToString();
                            int ultimoUnid_ARS_100 = lector.GetInt32(5);
                            int ultimoUnid_ARS_200 = lector.GetInt32(6);
                            int ultimoUnid_ARS_500 = lector.GetInt32(7);
                            int ultimoUnid_ARS_1000 = lector.GetInt32(8);
                            int ultimoUnid_ARS_2000 = lector.GetInt32(9);
                            int ultimoUnid_ARS_10000 = lector.GetInt32(10);
                            int ultimoUnid_ARS_20000 = lector.GetInt32(11);

                        }
                        else
                        {
                            MessageBox.Show("No se encontro una operacion previa.\nIngresando un deposito con valores 0");
                        }

                        lector.Close();
                        cerrarConexionSQL(conexion);



                        //capturar los campos
                        int unidades100 = int.Parse(txtBoxDepUnid100.Text);
                        int unidades200 = int.Parse(txtBoxDepUnid200.Text);
                        int unidades500 = int.Parse(txtBoxDepUnid500.Text);
                        int unidades1000 = int.Parse(txtBoxDepUnid1000.Text);
                        int unidades2000 = int.Parse(txtBoxDepUnid2000.Text);
                        int unidades10000 = int.Parse(txtBoxDepUnid10000.Text);
                        int unidades20000 = int.Parse(txtBoxDepUnid20000.Text);

                        //operacion de deposito
                        decimal SaldoActual = ultimoSaldo + (100 * unidades100) + (200 * unidades200) + (500 * unidades500) + (1000 * unidades1000) + (2000 * unidades2000)
                                          + (10000 * unidades10000) + (20000 * unidades20000);


                        //cargarlos en la db
                        string queryDeposito = "INSERT INTO [Operacion-] (cUsuario, Tipo, Saldo, Fecha, Unid_ARS_100, Unid_ARS_200, Unid_ARS_500," +
                                               " Unid_ARS_1000, Unid_ARS_2000, Unid_ARS_10000, Unid_ARS_20000)\n" +
                                               "VALUES (@idUsuario, @Tipo, @Saldo, @Fecha, @Unid_ARS_100, @Unid_ARS_200, @Unid_ARS_500, @Unid_ARS_1000, " +
                                               "@Unid_ARS_2000, @Unid_ARS_10000, @Unid_ARS_20000)";

                        SqlConnection conexionDeposito = abrirConexionSQL(strConexion);

                        SqlCommand cmdCargarDeposito = new SqlCommand(queryDeposito, conexionDeposito);

                        using (cmdCargarDeposito)
                        {
                            cmdCargarDeposito.Parameters.AddWithValue("@idUsuario", idUsuario);
                            cmdCargarDeposito.Parameters.AddWithValue("@Tipo", tipoOperacion);
                            cmdCargarDeposito.Parameters.AddWithValue("@Saldo", SaldoActual);
                            cmdCargarDeposito.Parameters.AddWithValue("@Fecha", DateTime.Now);
                            cmdCargarDeposito.Parameters.AddWithValue("@Unid_ARS_100", unidades100);
                            cmdCargarDeposito.Parameters.AddWithValue("@Unid_ARS_200", unidades200);
                            cmdCargarDeposito.Parameters.AddWithValue("@Unid_ARS_500", unidades500);
                            cmdCargarDeposito.Parameters.AddWithValue("@Unid_ARS_1000", unidades1000);
                            cmdCargarDeposito.Parameters.AddWithValue("@Unid_ARS_2000", unidades2000);
                            cmdCargarDeposito.Parameters.AddWithValue("@Unid_ARS_10000", unidades10000);
                            cmdCargarDeposito.Parameters.AddWithValue("@Unid_ARS_20000", unidades20000);

                            int filasAfectadas = cmdCargarDeposito.ExecuteNonQuery();

                            MessageBox.Show("Depósito Exitoso\n Se introdujo " + filasAfectadas.ToString() + "operación");

                            recalcularMontos();
                            ActualizarTextoLabels();
                            resetTxtBoxUnidades();
                        }
                    }
                }
                else { MessageBox.Show("Verificar de no ingresar espacios es blanco o nulos en las casillas"); }

            }
            else
            {
                MessageBox.Show("Para realizar un deposito debe inciar sesion");
            }


        }

        private void Retirar()
        {
            if (sesionIniciada)
            {
                if (validarTxtBoxesNullsRetiro())
                {
                    if (warningOperacion() == true)
                    {

                        // variables
                        string tipoOperacion = "Retiro";
                        /*
                        int ultimoUnid_ARS_100 = 0;
                        int ultimoUnid_ARS_200 = 0;
                        int ultimoUnid_ARS_500 = 0;
                        int ultimoUnid_ARS_1000 = 0;
                        int ultimoUnid_ARS_2000 = 0;
                        int ultimoUnid_ARS_10000 = 0;
                        int ultimoUnid_ARS_20000 = 0;*/

                        // obtengo las unidades actuales en la boveda
                        Dictionary<string, int> unidadesActuales = capturaUnidadesActuales();





                        // obtengo los datos de la operacion anterior

                        SqlConnection conexion = abrirConexionSQL(strConexion);

                        string queryConsultaDatosPasados = "SELECT *\r\nFROM [Operacion-]\r\nWHERE ID_Operacion = (SELECT MAX(ID_Operacion) FROM [Operacion-]);\r\n";

                        SqlCommand cmdConsultaDatosPasados = new SqlCommand(queryConsultaDatosPasados, conexion);

                        SqlDataReader lector = cmdConsultaDatosPasados.ExecuteReader();

                        decimal ultimoSaldo = 0;


                        if (lector.Read())
                        {

                            int ultimoId = lector.GetInt32(0);
                            int ultimocUsuario = lector.GetInt32(1);
                            string ultimoTipoOperacion = lector.GetString(2);
                            ultimoSaldo = lector.GetDecimal(3);
                            string ultimaFecha = lector.GetDateTime(4).ToString();
                            /*
                            ultimoUnid_ARS_100 = lector.GetInt32(5);
                            ultimoUnid_ARS_200 = lector.GetInt32(6);
                            ultimoUnid_ARS_500 = lector.GetInt32(7);
                            ultimoUnid_ARS_1000 = lector.GetInt32(8);
                            ultimoUnid_ARS_2000 = lector.GetInt32(9);
                            ultimoUnid_ARS_10000 = lector.GetInt32(10);
                            ultimoUnid_ARS_20000 = lector.GetInt32(11);*/

                        }
                        else
                        {
                            MessageBox.Show("No se encontro una operacion previa.\nPor favor, ingrese un primer depósito con valores 0");
                        }

                        lector.Close();
                        cerrarConexionSQL(conexion);


                        //capturar los campos de unidades a retirar

                        int unidades100 = int.Parse(txtBoxRetUnid100.Text);
                        int unidades200 = int.Parse(txtBoxRetUnid200.Text);
                        int unidades500 = int.Parse(txtBoxRetUnid500.Text);
                        int unidades1000 = int.Parse(txtBoxRetUnid1000.Text);
                        int unidades2000 = int.Parse(txtBoxRetUnid2000.Text);
                        int unidades10000 = int.Parse(txtBoxRetUnid10000.Text);
                        int unidades20000 = int.Parse(txtBoxRetUnid20000.Text);


                        // condicion de chequeo para no ingresar un retiro que exceda la cantidad de unidades existente en boveda
                        if (unidades100 > unidadesActuales["ARS100"] ||
                            unidades200 > unidadesActuales["ARS200"] ||
                            unidades500 > unidadesActuales["ARS500"] ||
                            unidades1000 > unidadesActuales["ARS1000"] ||
                            unidades2000 > unidadesActuales["ARS2000"] ||
                            unidades10000 > unidadesActuales["ARS10000"] ||
                            unidades20000 > unidadesActuales["ARS20000"])
                        {
                            MessageBox.Show("El retiro no puede exceder la cantidad de unidades disponibles para retirar");
                        }
                        else
                        {
                            // cargamos el retiro
                            //operacion de retiro
                            decimal SaldoActual = ultimoSaldo - (100 * unidades100) - (200 * unidades200) - (500 * unidades500) - (1000 * unidades1000) - (2000 * unidades2000)
                                              - (10000 * unidades10000) - (20000 * unidades20000);


                            //cargarlo en la db
                            string queryRetiro = "INSERT INTO [Operacion-] (cUsuario, Tipo, Saldo, Fecha, Unid_ARS_100, Unid_ARS_200, Unid_ARS_500," +
                                                   " Unid_ARS_1000, Unid_ARS_2000, Unid_ARS_10000, Unid_ARS_20000)\n" +
                                                   "VALUES (@idUsuario, @Tipo, @Saldo, @Fecha, @Unid_ARS_100, @Unid_ARS_200, @Unid_ARS_500, @Unid_ARS_1000, " +
                                                   "@Unid_ARS_2000, @Unid_ARS_10000, @Unid_ARS_20000)";

                            SqlConnection conexionRetiro = abrirConexionSQL(strConexion);

                            SqlCommand cmdCargarRetiro = new SqlCommand(queryRetiro, conexionRetiro);

                            using (cmdCargarRetiro)
                            {
                                cmdCargarRetiro.Parameters.AddWithValue("@idUsuario", idUsuario);
                                cmdCargarRetiro.Parameters.AddWithValue("@Tipo", tipoOperacion);
                                cmdCargarRetiro.Parameters.AddWithValue("@Saldo", SaldoActual);
                                cmdCargarRetiro.Parameters.AddWithValue("@Fecha", DateTime.Now);
                                cmdCargarRetiro.Parameters.AddWithValue("@Unid_ARS_100", unidades100);
                                cmdCargarRetiro.Parameters.AddWithValue("@Unid_ARS_200", unidades200);
                                cmdCargarRetiro.Parameters.AddWithValue("@Unid_ARS_500", unidades500);
                                cmdCargarRetiro.Parameters.AddWithValue("@Unid_ARS_1000", unidades1000);
                                cmdCargarRetiro.Parameters.AddWithValue("@Unid_ARS_2000", unidades2000);
                                cmdCargarRetiro.Parameters.AddWithValue("@Unid_ARS_10000", unidades10000);
                                cmdCargarRetiro.Parameters.AddWithValue("@Unid_ARS_20000", unidades20000);

                                int filasAfectadas = cmdCargarRetiro.ExecuteNonQuery();

                                MessageBox.Show("Retiro Exitoso\n Se introdujo " + filasAfectadas.ToString() + " operación");

                            }
                            recalcularMontos();

                            ActualizarTextoLabels();

                            resetTxtBoxUnidades();
                        }
                    }
                }
                else{ MessageBox.Show("Verificar de no ingresar espacios es blanco o nulos en las casillas"); }
            }
            else
            {
                MessageBox.Show("Para realizar una extracción, debe inciar sesion");
            }


        }

        private SqlConnection abrirConexionSQL(string strConexion)
        {

            
            SqlConnection conexion = new SqlConnection(strConexion);

            conexion.Open();
            //SqlCommand cmd = new SqlCommand()

            //sqlConnection conexion = new sqlConnection();
            return conexion;
        }

        private void cerrarConexionSQL(SqlConnection conexion)
        {
            conexion.Close();
        }

        private decimal capturaSaldoTotalActual()
        {
            string querySaldoTotal = "SELECT Saldo FROM [Operacion-] WHERE ID_Operacion = (SELECT MAX(ID_Operacion) FROM [Operacion-]);";
            decimal SaldoTotal = 0;

            SqlConnection conexion = abrirConexionSQL(strConexion);

            using (conexion)
            {
                SqlCommand cmdConsultaSaldo = new SqlCommand(querySaldoTotal, conexion);

                object resultado = cmdConsultaSaldo.ExecuteScalar();

                if (resultado != null)
                {
                    SaldoTotal = Convert.ToDecimal(resultado);
                }
            }
            return SaldoTotal;
        }
          
        private Dictionary <string, int>  capturaUnidadesActuales()
        {
            Dictionary <string, int> unidadesActuales = new Dictionary<string, int>();

            string queryConsultaUnidades = "SELECT SUM(CASE WHEN Tipo = 'Depósito' THEN Unid_ARS_100 ELSE 0 END) AS Total_Unid_ARS_100_Deposito, SUM(CASE WHEN Tipo = 'Depósito' THEN Unid_ARS_200 ELSE 0 END) AS Total_Unid_ARS_200_Deposito, SUM(CASE WHEN Tipo = 'Depósito' THEN Unid_ARS_500 ELSE 0 END) AS Total_Unid_ARS_500_Deposito, SUM(CASE WHEN Tipo = 'Depósito' THEN Unid_ARS_1000 ELSE 0 END) AS Total_Unid_ARS_1000_Deposito, SUM(CASE WHEN Tipo = 'Depósito' THEN Unid_ARS_2000 ELSE 0 END) AS Total_Unid_ARS_2000_Deposito, SUM(CASE WHEN Tipo = 'Depósito' THEN Unid_ARS_10000 ELSE 0 END) AS Total_Unid_ARS_10000_Deposito, SUM(CASE WHEN Tipo = 'Depósito' THEN Unid_ARS_20000 ELSE 0 END) AS Total_Unid_ARS_20000_Deposito, SUM(CASE WHEN Tipo = 'Retiro' THEN Unid_ARS_100 ELSE 0 END) AS Total_Unid_ARS_100_Retiro, SUM(CASE WHEN Tipo = 'Retiro' THEN Unid_ARS_200 ELSE 0 END) AS Total_Unid_ARS_200_Retiro, SUM(CASE WHEN Tipo = 'Retiro' THEN Unid_ARS_500 ELSE 0 END) AS Total_Unid_ARS_500_Retiro, SUM(CASE WHEN Tipo = 'Retiro' THEN Unid_ARS_1000 ELSE 0 END) AS Total_Unid_ARS_1000_Retiro, SUM(CASE WHEN Tipo = 'Retiro' THEN Unid_ARS_2000 ELSE 0 END) AS Total_Unid_ARS_2000_Retiro, SUM(CASE WHEN Tipo = 'Retiro' THEN Unid_ARS_10000 ELSE 0 END) AS Total_Unid_ARS_10000_Retiro, SUM(CASE WHEN Tipo = 'Retiro' THEN Unid_ARS_20000 ELSE 0 END) AS Total_Unid_ARS_20000_Retiro FROM [Operacion-];";



            SqlConnection conexion = abrirConexionSQL(strConexion);

            using (conexion)
            {
                SqlCommand cmdConsultaUnidades = new SqlCommand(queryConsultaUnidades, conexion);

                SqlDataReader lector = cmdConsultaUnidades.ExecuteReader();

                while (lector.Read())
                {
                    unidadesActuales["ARS100"] = (Convert.ToInt32(lector["Total_Unid_ARS_100_Deposito"])-(Convert.ToInt32(lector["Total_Unid_ARS_100_Retiro"])));
                    unidadesActuales["ARS200"] = (Convert.ToInt32(lector["Total_Unid_ARS_200_Deposito"]) - (Convert.ToInt32(lector["Total_Unid_ARS_200_Retiro"])));
                    unidadesActuales["ARS500"] = (Convert.ToInt32(lector["Total_Unid_ARS_500_Deposito"]) - (Convert.ToInt32(lector["Total_Unid_ARS_500_Retiro"])));
                    unidadesActuales["ARS1000"] = (Convert.ToInt32(lector["Total_Unid_ARS_1000_Deposito"]) - (Convert.ToInt32(lector["Total_Unid_ARS_1000_Retiro"])));
                    unidadesActuales["ARS2000"] = (Convert.ToInt32(lector["Total_Unid_ARS_2000_Deposito"]) - (Convert.ToInt32(lector["Total_Unid_ARS_2000_Retiro"])));
                    unidadesActuales["ARS10000"] = (Convert.ToInt32(lector["Total_Unid_ARS_10000_Deposito"]) - (Convert.ToInt32(lector["Total_Unid_ARS_10000_Retiro"])));
                    unidadesActuales["ARS20000"] = (Convert.ToInt32(lector["Total_Unid_ARS_20000_Deposito"]) - (Convert.ToInt32(lector["Total_Unid_ARS_20000_Retiro"])));
                }
                lector.Close();
                return unidadesActuales;    
            }


        }

        private void ActualizarTextoLabels()
        {
            if (sesionIniciada)
            {
                Dictionary<string, int> unidades = new Dictionary<string, int>();

                try
                {
                    unidades = capturaUnidadesActuales();
                    decimal SaldoTotal = capturaSaldoTotalActual();
                    muestraOperaciones();

                    foreach (Label label in labelsTotal)
                    {
                        label.Content = "$ " + SaldoTotal.ToString("#,##0.00");
                    }

                    foreach (Label label in labels100)
                    {
                        label.Content = (unidades["ARS100"]).ToString();
                    }

                    foreach (Label label in labels200)
                    {
                        label.Content = (unidades["ARS200"]).ToString();
                    }

                    foreach (Label label in labels500)
                    {
                        label.Content = (unidades["ARS500"]).ToString();
                    }
                    foreach (Label label in labels1000)
                    {
                        label.Content = (unidades["ARS1000"]).ToString();
                    }

                    foreach (Label label in labels2000)
                    {
                        label.Content = (unidades["ARS2000"]).ToString();
                    }

                    foreach (Label label in labels10000)
                    {
                        label.Content = (unidades["ARS10000"]).ToString();
                    }
                    foreach (Label label in labels20000)
                    {
                        label.Content = (unidades["ARS20000"]).ToString();
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Error al obtener los datos de la base de datos. Probablemente esté vacía. \n" + ex.Message +
                        "Por favor, relice un primer depósito con valores 0" );
                    return;
                }

                
            }
            else
            {
                foreach (Label label in labelsTotal)
                {
                    label.Content = "XXXXXX";
                }
                foreach (Label label in labels100)
                {
                    label.Content = "XXX";
                }

                foreach (Label label in labels200)
                {
                    label.Content = "XXX";
                }

                foreach (Label label in labels500)
                {
                    label.Content = "XXX";
                }
                foreach (Label label in labels1000)
                {
                    label.Content = "XXX";
                }

                foreach (Label label in labels2000)
                {
                    label.Content = "XXX";
                }

                foreach (Label label in labels10000)
                {
                    label.Content = "XXX";
                }
                foreach (Label label in labels20000)
                {
                    label.Content = "XXX";
                }
            }
        }

        private void recalcularMontos()
        {
            
            SqlConnection conexion = abrirConexionSQL(strConexion);

            string queryActualizaMontos = "UPDATE [Operacion-] SET Monto = Saldo - (SELECT TOP 1 Saldo FROM [Operacion-] o2 WHERE o2.ID_Operacion < [Operacion-].ID_Operacion ORDER BY o2.ID_Operacion DESC);\r\n";

            try{

                using (conexion)
                {
                    SqlCommand cmdActualizaMontos = new SqlCommand(queryActualizaMontos, conexion);

                    cmdActualizaMontos.ExecuteScalar();

                }
            }catch (Exception ex) { MessageBox.Show("Error al actualizar los montos en la base de datos: " + ex); }
            
        }

        void resetTxtBoxUnidades()
        {

            foreach (TextBox txtBox in txtBoxUnid100)
            {
                txtBox.Text = "0";
            }

            foreach (TextBox txtBox in txtBoxUnid200)
            {
                txtBox.Text = "0";
            }

            foreach (TextBox txtBox in txtBoxUnid500)
            {
                txtBox.Text = "0";
            }
            foreach (TextBox txtBox in txtBoxUnid1000)
            {
                txtBox.Text = "0";
            }
            foreach (TextBox txtBox in txtBoxUnid2000)
            {
                txtBox.Text = "0";
            }
            foreach (TextBox txtBox in txtBoxUnid10000)
            {
                txtBox.Text = "0";
            }
            foreach (TextBox txtBox in txtBoxUnid20000)
            {
                txtBox.Text = "0";
            }
        }

        private void estadoIncognito()
        {
            sesionIniciada = false;
            ActualizarTextoLabels();
            resetTxtBoxUnidades();
            ocultarOperaciones();

        }

        private void estadoVisible()
        {
            sesionIniciada = true;
            recalcularMontos();
            ActualizarTextoLabels();
            resetTxtBoxUnidades();
           
        }

        private void iniciarSesionBtn(object sender, RoutedEventArgs e)
        {
            string usuario, contraseña;

            usuario = usuarioTxt.Text;
            contraseña = contraseñaTxt.Password;

            try
            {
                // Validacion de usuario
                using (SqlConnection conexion = abrirConexionSQL(strConexion))
                {
                    string queryCompararIdentidad = "SELECT COUNT(*) FROM Usuario WHERE Usuario = @Usuario AND Contraseña = @Contraseña";
                    string queryIdUsuarioAutenticado = "SELECT ID_Usuario FROM Usuario WHERE Usuario = @Usuario AND Contraseña = @Contraseña";

                    using (SqlCommand cmdVerificarIdentidad = new SqlCommand(queryCompararIdentidad, conexion))
                    {
                        cmdVerificarIdentidad.Parameters.AddWithValue("@Usuario", usuario);
                        cmdVerificarIdentidad.Parameters.AddWithValue("@Contraseña", contraseña);

                        int cont = (int)cmdVerificarIdentidad.ExecuteScalar();

                        if (cont > 0)
                        {
                            //capturo el id del usuario autenticado
                            using (SqlCommand cmdCapturaIdUsuario = new SqlCommand(queryIdUsuarioAutenticado, conexion))
                            {
                                cmdCapturaIdUsuario.Parameters.AddWithValue("@Usuario", usuario);
                                cmdCapturaIdUsuario.Parameters.AddWithValue("@Contraseña", contraseña);

                                idUsuario = (int)cmdCapturaIdUsuario.ExecuteScalar();
                            }

                            // mensaje bienvenida
                            MessageBox.Show("Bienvenid@!");

                            // limpiamos casilleros
                            usuarioTxt.Clear();
                            contraseñaTxt.Clear();

                            // iniciamos estado de visibilidad
                            estadoVisible();
                        }
                        else
                        {
                            MessageBox.Show("Usuario o clave incorrectos, intente nuevamente");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Se produjo un error al intentar iniciar sesión: " + ex.Message);
            }
        }

        private void cerrarSesionIconClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            estadoIncognito();
        }

        private void depositarBtnClick(object sender, RoutedEventArgs e)
        {
            Depositar();
        }

        private void retirarBtnClick(object sender, RoutedEventArgs e)
        {
            Retirar();
        }

        private void muestraOperaciones()
        {
            //string queryConsultaOperaciones = "SELECT [Operacion-].ID_Operacion, Usuario.Usuario, [Operacion-].Tipo, [Operacion-].Monto, [Operacion-].Saldo, [Operacion-].Fecha FROM [Operacion-] INNER JOIN Usuario ON Usuario.ID_Usuario = [Operacion-].cUsuario ORDER BY [Operacion-].ID_Operacion DESC;\r\n";
            string queryConsultaOperaciones = "SELECT [Operacion-].ID_Operacion, Usuario.Usuario, [Operacion-].Tipo, FORMAT([Operacion-].Monto, 'N0', 'es-AR') AS Monto, FORMAT([Operacion-].Saldo, 'N0', 'es-AR') AS Saldo, [Operacion-].Fecha FROM [Operacion-] INNER JOIN Usuario ON Usuario.ID_Usuario = [Operacion-].cUsuario ORDER BY [Operacion-].ID_Operacion DESC;\r\n";


            SqlConnection conexion = abrirConexionSQL(strConexion);

            SqlDataAdapter miAdaptadorSQL = new SqlDataAdapter(queryConsultaOperaciones, conexion);

            using (miAdaptadorSQL)
            {
                DataTable operacionesTabla = new DataTable();

                miAdaptadorSQL.Fill(operacionesTabla);

                // Configurar ambos DataGrids para mostrar los datos de la misma tabla
                DataGridOperaciones.ItemsSource = operacionesTabla.DefaultView;
                DataGridOperacionesDep.ItemsSource = operacionesTabla.DefaultView;
            }

            cerrarConexionSQL(conexion);
        }

        private void ocultarOperaciones()
        {
            // Limpiar tabla de operaciones
            DataGridOperaciones.ItemsSource = null;
            DataGridOperaciones.Items.Clear();

            // Limpiar tabla de operaciones de depósito
            DataGridOperacionesDep.ItemsSource = null;
            DataGridOperacionesDep.Items.Clear();

        }

        private void validarNumeroEntero(object sender, TextCompositionEventArgs e)
        {
            // este evento es activado al introducir el texto en el txtBox
            // si el nuero ingresado no es un entero o 0, cancela la entrada
            if (!int.TryParse(e.Text, out int result) || result < 0)
            {
                e.Handled = true; // Cancela la entrada
            }
        }

        private void UsuarioTxt_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && textBox.Text == "Usuario")
            {
                textBox.Text = "";
            }
        }

        private void contraseñaTxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Llama al método de inicio de sesión cuando se presiona Enter
                iniciarSesionBtn(sender, e);
            }
        }

        private void borrarBtnClick(object sender, RoutedEventArgs e)
        {
            if (sesionIniciada)
            {
                try
                {

                    Button btn = sender as Button;

                    DataRowView filaSeleccionada = null;

                    if (btn == btnBorrarRet)
                    {
                        if (DataGridOperaciones.SelectedItem != null)
                        {
                            filaSeleccionada = DataGridOperaciones.SelectedItem as DataRowView;
                        }
                    }
                    else if (btn == btnBorrarDep)
                    {
                        if (DataGridOperacionesDep.SelectedItem != null)
                        {
                            filaSeleccionada = DataGridOperacionesDep.SelectedItem as DataRowView;
                        }
                    }


                    // Comprueba si hay una fila seleccionada
                    if (filaSeleccionada != null)
                    {
                        // Accede a los valores de la fila seleccionada
                        int idOperacion = (int)filaSeleccionada["ID_Operacion"];


                        //consultamos si esta seguro que desea borrar
                        MessageBoxResult rtaUsuario = MessageBox.Show("¿Está seguro de que desea borrar el registro con el ID: " + idOperacion + "?", "Confirmar borrado", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (rtaUsuario == MessageBoxResult.Yes)
                        {
                            
                            borrarRegistro(idOperacion, btn);
                        }
                        
                    }
                    else
                    {
                        MessageBox.Show("Por favor, selecciona un registro antes de borrar.");
                    }
                }
                catch (Exception) { MessageBox.Show("Por favor, ingrese una fila con Datos Válidos"); }
            }
            else { MessageBox.Show("Primero debe iniciar sesión para poder eliminar un registro"); }
            
             
            

        }

        private void borrarRegistro(int idOperacion,Button btn)
        {

            string queryBorrarRegistro = "DELETE FROM [Operacion-] WHERE ID_Operacion=@idOperacion";

            SqlConnection conexion = abrirConexionSQL(strConexion);

            SqlCommand cmdBorrarRegistro= new SqlCommand(queryBorrarRegistro, conexion);

            cmdBorrarRegistro.Parameters.AddWithValue("@idOperacion", idOperacion);


            cmdBorrarRegistro.ExecuteNonQuery();

            cerrarConexionSQL(conexion);

            ActualizarTextoLabels();
        }

        private bool warningOperacion()
        {
            //consultamos si esta seguro que desea borrar
            MessageBoxResult rtaUsuario = MessageBox.Show("¿Está seguro/a de que desea cargar el registro? ", "Confirmar registro", MessageBoxButton.YesNo, MessageBoxImage.Question);

            return rtaUsuario == MessageBoxResult.Yes ? true : false;

        }

        private DataTable obtenerDB()
        {
            string queryDataBase = "SELECT [Operacion-].ID_Operacion, [Operacion-].cUsuario, [Operacion-].Tipo, Usuario.Usuario, [Operacion-].Monto, [Operacion-].Saldo, [Operacion-].Fecha, [Operacion-].Unid_ARS_100, [Operacion-].Unid_ARS_200, [Operacion-].Unid_ARS_500, [Operacion-].Unid_ARS_1000, [Operacion-].Unid_ARS_2000, [Operacion-].Unid_ARS_10000, [Operacion-].Unid_ARS_20000, Usuario.ID_Usuario FROM [Operacion-] INNER JOIN Usuario ON Usuario.ID_Usuario = [Operacion-].cUsuario;\r\n";

            DataTable tablaDataBase = new DataTable();

            try
            {
                SqlConnection conexion = abrirConexionSQL(strConexion);
                SqlDataAdapter miAdaptadorSQL = new SqlDataAdapter(queryDataBase, conexion);

                using (miAdaptadorSQL)
                {
                    miAdaptadorSQL.Fill(tablaDataBase);
                }

                cerrarConexionSQL(conexion);

                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener la base de datos: " + ex.Message);
                // En caso de error, retornamos null
                return null;
            }

            return tablaDataBase;
        }


        bool validarTxtBoxesNullsRetiro()
        {
            foreach (TextBox textBox in txtBoxesRet)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    return false;
                }
            }
            return true; // Si no se encontraron cuadros de texto en blanco, se devuelve true
        }

        bool validarTxtBoxesNullsDeposito()
        {
            foreach (TextBox textBox in txtBoxesDep)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    return false;
                }
            }
            return true; // Si no se encontraron cuadros de texto en blanco, se devuelve true
        }



        private void exportarExcel(DataTable tabla)
        {
            try
            {
                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("tabla");

                    worksheet.Cells.LoadFromDataTable(tabla, true);

                    // Modificar el campo de fecha a tipo fecha
                    ExcelRange column = worksheet.Cells["G:G"];
                    column.Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";

                    FileInfo archivoExcel = new FileInfo("\\\\192.168.1.251\\shared\\APP Cajero\\Reporte.xlsx");
                    excelPackage.SaveAs(archivoExcel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al exportar la base de datos: " + ex.Message);
            }
        }


        private void btnExportatClick(object sender, RoutedEventArgs e)
        {
            if (sesionIniciada)
            {
                if (obtenerDB() != null)
                {
                    DataTable tabla = obtenerDB();
                    exportarExcel(tabla);
                    MessageBox.Show("Exportado exitoso");
                }

            }
            else
            {
                MessageBox.Show("Debe iniciar sesion para poder exportar los datos");
            }
            
            
            
        }

    }

}
