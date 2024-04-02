using Amazon.S3.Model;
using System.Collections.Generic;

namespace EcoPlanet.Models
{
    public class TrashpediaViewModel
    {
        public List<Trashpedia> TrashpediaList { get; set; }

        public List<S3Object> ImageList { get; set; }
    }
}