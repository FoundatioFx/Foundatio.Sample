using System;
using Foundatio.Utility;

namespace Samples.Core.Models {
    public class EntityChanged {
        public EntityChanged() {
            Data = new DataDictionary();
        }
        
        public string Id { get; set; }
        public ChangeType ChangeType { get; set; }
        public DataDictionary Data { get; set; }
    }

    public enum ChangeType : byte {
        Added = 0,
        Saved = 1,
        Removed = 2
    }
}