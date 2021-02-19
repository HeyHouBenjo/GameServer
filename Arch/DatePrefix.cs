using System;
using System.IO;
using System.Text;

namespace GameServer.Arch {
    
    public class DatePrefix : TextWriter {
        
        private readonly TextWriter _originalOut;

        public DatePrefix() {
            _originalOut = Console.Out;
        }
        
        public override Encoding Encoding => new ASCIIEncoding();

        public override void WriteLine(string value) {
            _originalOut.WriteLine($"{DateTime.Now} {value}");
        }
    }
}
