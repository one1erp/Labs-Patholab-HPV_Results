using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Patholab_DAL_V1;


namespace ReciveShipmentWpf
{
    public class ListDatas
    {
        private DataLayer dal;

        public ListDatas(DataLayer dal)
        {

            this.dal = dal;
            this.Clinics = dal.GetAll<U_CLINIC>().ToList();
            Customers = dal.GetAll<U_CUSTOMER>().ToList();
            Operators = dal.GetAll<OPERATOR>().ToList();
        }

        public List<U_CLINIC> Clinics { get; set; }
        public List<U_CUSTOMER> Customers { get; set; }

        public List<OPERATOR> Operators { get; set; }
    }
}
