namespace omg_app.Models
{
    using Microsoft.ProjectOxford.Face.Contract;

    public class FaceAttributesModel
    {
        public string ImageUrl { get; set; }

        public FaceAttributes FaceAttributes{ get; set; }
    }
}
