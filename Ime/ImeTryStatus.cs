using System;
using System.Collections.Generic;
using System.Text;

namespace Ime.Core;

public enum ImeTryStatus
{
    Success = 0,
    NoForeground = 1,
    NoImeContext = 2,
    AlreadyHiragana = 3,
    FailedToSet = 4,
    LanguageSwitchRequested = 5
}
