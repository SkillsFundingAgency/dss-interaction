using System.ComponentModel;

namespace NCS.DSS.Interaction.ReferenceData
{
    public enum Channel
    {
        [Description("Face to face")]
        FaceToFace = 1,
        Telephone = 2,
        Webchat = 3,
        Videochat = 4,
        Email = 5,
        [Description("Social media")]
        SocialMedia = 6,
        SMS = 7,
        Post = 8,
        [Description("Co-browse")]
        Cobrowse = 9,
        Other = 99
    }
}
