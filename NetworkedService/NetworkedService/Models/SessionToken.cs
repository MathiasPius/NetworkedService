using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService.Models
{
    public class SessionToken : IEquatable<SessionToken>
    {
        public Guid Token;

        public SessionToken(Guid guid)
        {
            Token = guid;
        }

        public bool Equals(SessionToken other)
        {
            return Token.Equals(other.Token);
        }

        public static SessionToken NewToken()
        {
            return new SessionToken(Guid.NewGuid());
        }
    }

    public class Session
    {
        public SessionToken Token { get; set; }
        public byte[] Message { get; set; }
    }
}
