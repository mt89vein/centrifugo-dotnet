using System;

namespace Centrifugo.Sample.Models
{
    public class TestMessage
    {
        public string? Msg { get; set; }

        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Id.ToString() + ":" + Msg;
        }
    }
}