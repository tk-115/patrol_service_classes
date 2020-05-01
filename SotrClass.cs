using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Patrol_service
{
    public class SotrClass
    {
        //звание, фио, имя файла подписи в data/signatures/
        public string rank, FIO, pidpfilename;
        //list выписаных протоколов в котором значение - id протокола
        public List<int> proto = new List<int>();
    }
}
