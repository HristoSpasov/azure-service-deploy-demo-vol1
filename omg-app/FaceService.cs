namespace omg_app
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ProjectOxford.Face;
    using Microsoft.ProjectOxford.Face.Contract;
    using Models;

    public class FaceService
    {
        private readonly IFaceServiceClient faceServiceClient;

        public FaceService(FaceServiceStorageInfo info)
        {
            this.faceServiceClient = new FaceServiceClient(info.SubscriptionKey, info.ApiRoot);
        }

        public async Task<FaceAttributes[]> DetectFaceAttributesAsync(string url)
        {

            var faces = await this.faceServiceClient.DetectAsync(url, true,
                true,
                new FaceAttributeType[] {
                    FaceAttributeType.Gender,
                    FaceAttributeType.Age,
                    FaceAttributeType.Emotion,
                    FaceAttributeType.Smile,
                });

            var faceAttributes = faces.Select(face => face.FaceAttributes).ToArray();
            return faceAttributes;
        }
    }
}
