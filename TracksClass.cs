using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Patrol_service
{
    public class TracksClass{
        public TracksClass() { }

        //Структура контрольной точки
        public class Point2f{
            public int x, y;
            public int[] probabilities;
        }

        //контрольные точки
        public List<Point2f> control_points = new List<Point2f>();

        //id контрольной точки, определяющей возврат в гараж
        public int point_back_id = 3;
    }
}
