using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fiskalizacija1.Interfaces;

public interface IGreska
{
	public GreskaType[] Greske { get; set; }
}
