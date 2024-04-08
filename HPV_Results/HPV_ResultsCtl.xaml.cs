using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;


using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Patholab_Common;

using Patholab_DAL_V1;
using CheckBox = System.Windows.Controls.CheckBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

//using MessageBox = System.Windows.Controls.MessageBox;


namespace HPV_Results
{
    /// <summary>
    /// Interaction logic for WpfShipmentCtl.xaml
    /// </summary>
    public partial class HPV_ResultsCtl : System.Windows.Controls.UserControl
    {
        #region Ctor
        public HPV_ResultsCtl(HPV_ResultsCls controller)
        {
            InitializeComponent();
            GridHPV.Visibility = Visibility.Visible;
            // GridLogin.Visibility = Visibility.Visible;
            this.Controller = controller;
            this.DataContext = this;

            SetPicture("U");
            InitialMode();
            FirstFocus();

        }

        #endregion
        #region Timer

        private Timer _timerFocus = null;

        private void FirstFocus()
        {
            //First focus because nautius's bag
            _timerFocus = new Timer { Interval = 20000 };
            _timerFocus.Tick += timerFocus_Tick;
            _timerFocus.Start();
            // txtUn.Focus();
            txtInternalNbr.Focus();
        }

        private void timerFocus_Tick(object sender, EventArgs e)
        {
            //txtUn.Focus();
            txtInternalNbr.Focus();
            _timerFocus.Stop();

        }

        #endregion
        #region CONSTANTS

        private const string MboxHeader = "הזנת תוצאות - HPV";
        private const string HpvPos = "HPV Pos";
        private const string HpvNeg = "HPV Neg";
        private const string HpvOthers = "HPV Others";
        private const string HpvRemark = "HPV Remark";


        private const string Hr = "HR";
        private const string HrType = "HR Type";
        private const string Lr = "LR";
        private const string LrType = "LR Type";
        private const string UnknownType = "Unknown Type";
        private const string ProntoName = "Pronto Name";

        #endregion
        #region Local fields



        public bool DEBUG;
        public List<PHRASE_ENTRY> ListLowRisk { get; set; }
        public List<PHRASE_ENTRY> ListHighRisk { get; set; }



        private readonly HPV_ResultsCls Controller;

        private Request _request;

        public ObservableCollection<RiskType> ListHighRisk2 { get; set; }
        public ObservableCollection<RiskType> ListLowRisk2 { get; set; }
        #endregion

        public bool CloseQuery()
        {
            Controller.CloseSite();
            return true;
        }

        #region PRIVATE METHODS

        private void InitialMode()
        {

            _request = null;
            lblHeader.Text = string.Empty;
            txtInternalNbr.Text = string.Empty;

            cbUnKnown.IsChecked = false;
            cbHR.IsChecked = false;
            cbLR.IsChecked = false;
            rbNeg.IsChecked = false;
            rbPos.IsChecked = false;
            rbOther.IsChecked = false;
            //expLr.IsExpanded = false;
            //expHr.IsExpanded = false;
            cbUnKnown.IsChecked = false;


            rbNeg.IsEnabled = false;
            rbPos.IsEnabled = false;
            rbOther.IsEnabled = false;


            txtPronto.Clear();
            txtRemark.Clear();
            SetPicture("U");


            if (ListHighRisk2 != null) ListHighRisk2.Foreach(x => x.Checked = false);
            if (ListLowRisk2 != null) ListLowRisk2.Foreach(x => x.Checked = false);
            EditMode(false);
        }

        private void EditMode(bool p)
        {


            rbNeg.IsEnabled = p;
            rbPos.IsEnabled = p;
            rbOther.IsEnabled = p;

            txtRemark.IsEnabled = p;
            txtPronto.IsEnabled = p;

            btnOK.IsEnabled = p;
            btnManager.IsEnabled = p;
            if (p)
            {
                btnManager.IsEnabled = Controller.IsManager;
            }
        }

        private void SetPicture(string status)
        {

            try
            {


                const string ResourcePath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Thermo\Nautilus\9.2\Directory";

                var path = (string)Registry.GetValue(ResourcePath, "Resource", null);

                if (path != null)
                {
                    path += "\\";
                    const string _tableName = "SDG";


                    var uri = new Uri(path + _tableName + status + ".ico");
                    var bitmap = new BitmapImage(uri);
                    imgStatus.Source = bitmap;

                }
            }
            catch (Exception ex)
            {

                Logger.WriteLogFile(ex);
            }
        }

        #endregion

        #region Logic

        private void TxtInternalNbr_OnKeyDown(object o, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter || e.Key == Key.Tab)
                {

                    //    System.Diagnostics.Debugger.Launch();
                    if (string.IsNullOrEmpty(txtInternalNbr.Text)) return;

                    Request request = Controller.GetSdgByName(txtInternalNbr.Text);

                    if (request == null)
                    {
                        txtInternalNbr.Text = string.Empty;
                        MessageBox.Show(Controller.GetSdgRes, MboxHeader, MessageBoxButton.OK, MessageBoxImage.Hand);
                    }

                    else
                    {
                        LoadRequest(request);

                        EditMode("VPC".Contains(request.Status));
                    }
                }
            }
            catch
                (Exception ex)
            {
                MessageBox.Show(".שגיאה בטעינת הדרישה" + ex.Message,
                                MboxHeader, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.WriteLogFile(ex);

            }


        }


        private void LoadRequest(Request request)
        {
            this._request = request;
            lblHeader.Text = request.Header;
            txtPronto.Text = request.PontoNum;
            SetPicture(request.Status);
            LoadResults(request.Results);

        }

        private void LoadResults(List<WResult> results)
        {


            foreach (WResult r in results)
            {
                switch (r.Name)
                {

                    case HpvPos:
                        rbPos.IsChecked = (r.Value == "T" || r.Value == "True");
                        break;
                    case HpvNeg:
                        rbNeg.IsChecked = (r.Value == "T" || r.Value == "True");
                        break;
                    case HpvOthers:
                        rbOther.IsChecked = (r.Value == "T" || r.Value == "True");
                        break;
                    case HpvRemark:
                        txtRemark.Text = r.Value;
                        break;
                    case Hr:
                        cbHR.IsChecked = (r.Value == "T" || r.Value == "True");
                        break;
                    case HrType:
                        SetRiskType(ListHighRisk2, r.Value);
                        break;
                    case Lr:
                        cbLR.IsChecked = (r.Value == "T" || r.Value == "True");
                        break;
                    case LrType:
                        SetRiskType(ListLowRisk2, r.Value);
                        break;
                    case UnknownType:
                        cbUnKnown.IsChecked = (r.Value == "T" || r.Value == "True");
                        break;
                    case ProntoName:
                        txtPronto.Text = r.Value;
                        break;
                }
            }
        }

        private void SaveResult()
        {

            foreach (WResult wr in _request.Results)
            {

                wr.Status = "C";
                switch (wr.Name)
                {

                    case HpvPos:
                        wr.Value = rbPos.IsChecked == true ? "True" : "False";
                        break;
                    case HpvNeg:
                        wr.Value = rbNeg.IsChecked == true ? "True" : "False";
                        break;
                    case HpvOthers:
                        wr.Value = rbOther.IsChecked == true ? "True" : "False";
                        break;
                    case HpvRemark:
                        wr.Value = txtRemark.Text;
                        break;
                    case Hr:
                        wr.Value = cbHR.IsChecked == true ? "True" : "False";
                        break;
                    case HrType:
                        wr.Value = GetRiskType(ListHighRisk2);
                        break;
                    case Lr:
                        wr.Value = cbLR.IsChecked == true ? "True" : "False";
                        break;
                    case LrType:
                        wr.Value = GetRiskType(ListLowRisk2);

                        break;
                    case UnknownType:
                        wr.Value = cbUnKnown.IsChecked == true ? "True" : "False";
                        break;
                    case ProntoName:
                        wr.Value = txtPronto.Text;
                        break;
                }
            }
            _request.PontoNum = txtPronto.Text;



            string b = Controller.SaveReq(_request);
            if (!string.IsNullOrEmpty(b))
            {
                MessageBox.Show("Error on save data by result entry xml " + b,
                                MboxHeader, MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.WriteLogFile(b);
            }
            else
            {
                InitialMode();
            }
        }

        private void SetRiskType(ObservableCollection<RiskType> wp, string val)
        {

            foreach (RiskType riskType in wp)
            {
                riskType.Checked = false;
            }
            if (val != null)
            {
                var splited = val.Split(';');
                foreach (string s in splited)
                {
                    var po = wp.FirstOrDefault(x => x.RiskVal == s);
                    if (po != null) po.Checked = true;
                }
            }
        }

        private string GetRiskType(ObservableCollection<RiskType> wp)
        {

            var selectedVal = wp.Where(x => x.Checked).Select(x => x.RiskVal);
            string res = null;
            foreach (string s in selectedVal)
            {
                res += s + ";";
            }
            return res;

        }

        private bool IsValid4Save()
        {
            if (rbNeg.IsChecked == false && rbPos.IsChecked == false && rbOther.IsChecked == false) // לא נבחרה תוצאה
                return false;

            if (rbPos.IsChecked == true) //נבחרה תוצאה חיובית
            {
                List<CheckBox> checkBoxes = new List<CheckBox>() { cbHR, cbLR, cbUnKnown };
                if (checkBoxes.All(x => x.IsChecked != true))
                {
                    //לא נבחרה סוג תוצאה חיובית
                    return false;
                }

                if (cbHR.IsChecked == true && string.IsNullOrEmpty(GetRiskType(ListHighRisk2)))
                    return false;
                if (cbLR.IsChecked == true && string.IsNullOrEmpty(GetRiskType(ListLowRisk2)))
                    return false;
            }
            return true;

        }

        public void SetList()
        {
            ListLowRisk2 = (from item in ListLowRisk select new RiskType() { RiskVal = item.PHRASE_NAME, Checked = false }).ToObservableCollection();
            ListHighRisk2 = (from item in ListHighRisk select new RiskType() { RiskVal = item.PHRASE_NAME, Checked = false }).ToObservableCollection();
        }

        #endregion

        #region UI events

        private void btnExit_Click(object sender, EventArgs e)
        {

            var dg = MessageBox.Show("?האם אתה בטוח שברצונך לצאת",
                                     MboxHeader, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dg == MessageBoxResult.Yes)
                Controller.CloseSite();
        }

        private void ButtonOK_OnClick(object sender, RoutedEventArgs e)
        {
            if (IsValid4Save())
            {
                SaveResult();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("לא הוזנה תוצאה כראוי!!", MboxHeader);
            }
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            InitialMode();
        }

        private void ButtonManger_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {


                var msg = Controller.Authorize();
                if (!string.IsNullOrEmpty(msg))
                {
                    var dg = MessageBox.Show("Error on approve request ",
                                             MboxHeader, MessageBoxButton.YesNo, MessageBoxImage.Error);
                }
            }


            catch (Exception ex)
            {

                MessageBox.Show("Error on approve request " + ex.Message,
                                         MboxHeader, MessageBoxButton.YesNo, MessageBoxImage.Error);
            }
            finally
            {
                InitialMode();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            return;

            string s = Controller.CheckIdentity(txtUn.Text, txtPass.Text);
            if (string.IsNullOrEmpty(s))
            {
                GridHPV.Visibility = Visibility.Visible;
                GridLogin.Visibility = Visibility.Hidden;
                txtUser.Text = txtUn.Text;

            }
            else
            {
                MessageBox.Show(s, MboxHeader);
            }
        }

        private void To_english(object sender, RoutedEventArgs e)
        {
            zLang.English();
        }

        private void ToHebrew(object sender, RoutedEventArgs e)
        {
            zLang.Hebrew();
        }

        #endregion

        private void txtInternalNbr_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

    }

    public class RiskType : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _checked;
        public bool Checked
        {
            get { return _checked; }
            set
            {
                this._checked = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("Checked");


            }
        }

        private string _riskVal;
        public string RiskVal
        {
            get { return _riskVal; }
            set
            {
                _riskVal = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("RiskVal");
            }
        }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}



