using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Soundscripter
{
    [BsonIgnoreExtraElements]
    public class SamplesCollection 
    {
        public List<Sample> samples { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string transcriptId { get; set; }
    }
}