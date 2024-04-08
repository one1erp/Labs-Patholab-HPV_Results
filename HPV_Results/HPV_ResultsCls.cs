using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using LSExtensionWindowLib;
using LSSERVICEPROVIDERLib;
using Patholab_Common;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Patholab_DAL_V1;


using System.Diagnostics;
using Patholab_DAL_V1.Enums;
using Patholab_XmlService;
using MessageBox = System.Windows.Forms.MessageBox;


namespace HPV_Results
{




    [ComVisible(true)]
    [ProgId("HPV_Results.HPV_ResultsCls")]
    public partial class HPV_ResultsCls : UserControl, IExtensionWindow
    {
        #region Members

        private IExtensionWindowSite2 _ntlsSite;
        private INautilusServiceProvider _sp;
        private INautilusDBConnection _ntlsCon;
        public bool DEBUG;
        private DataLayer _dal;
        private Request _request;
        private SDG _sdg;
        private List<RESULT> _hpvResults;
        public List<PHRASE_ENTRY> ListLowRisk { get; set; }
        public List<PHRASE_ENTRY> ListHighRisk { get; set; }
        public string GetSdgRes { get; set; }

        #endregion

        #region Ctor

        public bool RunFromWindow { get; set; }

        public HPV_ResultsCls()
        {
            InitializeComponent();
            BackColor = Color.FromName("Control");
        }


        public void RegRunFromWindow(INautilusServiceProvider sp, INautilusDBConnection ntlsCon)
        {


            RunFromWindow = true;
            this._ntlsCon = ntlsCon;
            Init();
        }

        #endregion


        public void Init()
        {
            HPV_ResultsCtl view = new HPV_ResultsCtl(this);
            if (DEBUG)
                view.DEBUG = true;
            elementHost1.Child = view;


            _dal = new DataLayer();

            if (DEBUG)
                _dal.MockConnect();
            else
                _dal.Connect(_ntlsCon);

            view.ListLowRisk = _dal.GetPhraseEntries("HPV Low Risk").ToList();
            view.ListHighRisk = _dal.GetPhraseEntries("HPV High Risk").ToList();
            view.SetList();
        }

        #region Implement IExtensionWindow

        public bool CloseQuery()
        {
            return true;
        }

        public void Internationalise()
        {
        }

        public void SetSite(object site)
        {
            _ntlsSite = (IExtensionWindowSite2)site;
            _ntlsSite.SetWindowInternalName("");
            _ntlsSite.SetWindowRegistryName("");
            _ntlsSite.SetWindowTitle("הזנת תוצאות - HPV");
        }

        public void PreDisplay()
        {

            this.DEBUG = false;
            Init();

        }

        public WindowButtonsType GetButtons()
        {
            return LSExtensionWindowLib.WindowButtonsType.windowButtonsNone;
        }

        public bool SaveData()
        {
            return false;
        }

        public void SetServiceProvider(object serviceProvider)
        {
            _sp = serviceProvider as NautilusServiceProvider;
            _ntlsCon = Utils.GetNtlsCon(_sp);

        }

        public void SetParameters(string parameters)
        {

        }

        public void Setup()
        {

        }

        public WindowRefreshType DataChange()
        {
            return LSExtensionWindowLib.WindowRefreshType.windowRefreshNone;
        }

        public WindowRefreshType ViewRefresh()
        {
            return LSExtensionWindowLib.WindowRefreshType.windowRefreshNone;
        }

        public void refresh()
        {
        }

        public void SaveSettings(int hKey)
        {
        }

        public void RestoreSettings(int hKey)
        {
        }

        #endregion

        #region LOGIC

        public bool IsManager { get; private set; }

        internal Request GetSdgByName(string p)
        {
            this._sdg = null;
            if (_hpvResults != null) _hpvResults.Clear();

            _request = new Request();


            GetSdgRes = "הכל טוב";


            var barcode = p;// p. Replace(".", "/");
            var bup = barcode.ToUpper();
            //_sdg = _dal.FindBy<SDG>(x => (x.NAME == bup || x.SDG_USER.U_PATHOLAB_NUMBER == bup)
            var smp = _dal.FindBy<SAMPLE>(x => (x.NAME == bup || x.SAMPLE_USER.U_PATHOLAB_SAMPLE_NAME == bup
                || x.SDG.NAME == bup || x.SDG.SDG_USER.U_PATHOLAB_NUMBER == bup)
                                          && "CVP".Contains(x.STATUS))
                        .Include(x => x.SDG.SDG_USER)
                        .Include(x => x.SDG.SDG_USER.CLIENT)
                        .Include(x => x.SDG.SDG_USER.CLIENT.CLIENT_USER)
                        .SingleOrDefault();

            if (smp == null)
            {
                GetSdgRes = ".דרישה לא קיימת או אינה בסטטוס המתאים";
                return null;
            }
            _sdg = smp.SDG;
            if (_sdg.SdgType != SdgType.Pap)
            {
                GetSdgRes = "הזן בקשה ל PAP בלבד.";
                return null;

            }
            var _hpvTest = (from aliq in _dal.FindBy<TEST>(al => al.ALIQUOT.SAMPLE.SDG_ID == _sdg.SDG_ID
                                                              && al.NAME == "HPV"
                                                              && "CVP".Contains(al.STATUS))
                                          .Include(a => a.ALIQUOT.ALIQUOT_USER)
                            select new
                            {
                                PontoNum = aliq.ALIQUOT.ALIQUOT_USER.U_EXTERNAL_LAB_NUM,
                                aliquotId = aliq.ALIQUOT_ID.Value
                            })
                            .FirstOrDefault();
            if (_hpvTest == null)
            {
                GetSdgRes = ".או שאינה בסטטוס המתאים HPV לא קיימת דרישה ל  ";
                return null;
            }
            var c = _sdg.SDG_USER.CLIENT;
            var cu = _sdg.SDG_USER.CLIENT.CLIENT_USER;
            string cln = cu.U_FIRST_NAME + " " + cu.U_LAST_NAME + " - " + c.NAME + " ";


            _request.Header = string.Format("{0} - {1}", cln, _sdg.SDG_USER.U_PATHOLAB_NUMBER);
            _request.Status = _sdg.STATUS;
            _request.Results = GetResults(_sdg.SDG_ID);
            _request.PontoNum = _hpvTest.PontoNum;
            _request.AliquotId = _hpvTest.aliquotId;


            return _request;
        }

        public List<WResult> GetResults(long sdgId)
        {

            _hpvResults = (from rl in _dal.FindBy<RESULT>
                        (x => x.TEST.ALIQUOT.SAMPLE.SDG_ID == sdgId)
                           where rl.TEST.NAME == "HPV"
                           select rl).ToList();
            var res = _hpvResults.Select(x => new WResult { Name = x.NAME, Value = x.FORMATTED_RESULT }).ToList();


            return res;

        }

        internal string SaveReq(Request hpv_request)
        {
            try
            {

                foreach (WResult wResult in hpv_request.Results)
                {
                    RESULT r = this._hpvResults.FirstOrDefault(x => x.NAME == wResult.Name);
                    r.FORMATTED_RESULT = wResult.Value;

                    //Set original result
                    if (wResult.Value == "True")
                        r.ORIGINAL_RESULT = "T";
                    else if (wResult.Value == "False")
                        r.ORIGINAL_RESULT = "F";
                    else
                        r.ORIGINAL_RESULT = wResult.Value;





                    r.STATUS = "C";
                }
                _hpvResults.First().TEST.STATUS = "C";
                _hpvResults.First().TEST.ALIQUOT.ALIQUOT_USER.U_EXTERNAL_LAB_NUM = hpv_request.PontoNum;




                _dal.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }

        internal string CheckIdentity(string un, string pass)
        {
            var q = (from item in _dal.GetPhraseEntries("HPV Ponto Operators")
                     where item.PHRASE_NAME == un && item.PHRASE_DESCRIPTION == pass
                     select item).FirstOrDefault();
            IsManager = q != null && q.PHRASE_INFO == "A";
            return q == null ? "שם משתמש או סיסמא אינם נכונים" : "";

        }

        internal string Authorize()
        {

            FireEventXmlHandler fe = new FireEventXmlHandler(_sp);
            fe.CreateFireEventXml("ALIQUOT", _request.AliquotId, "Approve");
            var b = fe.ProcssXml();
            return b ? "" : fe.ErrorResponse;

        }

        #endregion

        public void CloseSite()
        {
            if (_dal != null) _dal.Close();
            if (_ntlsSite != null) _ntlsSite.CloseWindow();
        }


    }


}



