using Amazon.S3.Model;
using System.Collections.Generic;

namespace EcoPlanet.Models
{
    public class GoodsViewModel
    {
        public List<Goods> GoodsList { get; set; }
        public List<S3Object> ImageList { get; set; }
    }
}
