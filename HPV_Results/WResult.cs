using System.Collections.Generic;

namespace HPV_Results
{
    public class WResult
    {
        public string Status;
        public long ResultId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Request
    {
        public string PontoNum { get; set; }
        public long AliquotId { get; set; }
        public string Status { get; set; }
        public string Header { get; set; }
        public List<WResult> Results { get; set; }




    }
}