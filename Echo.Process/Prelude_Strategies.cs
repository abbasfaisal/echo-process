﻿using LanguageExt;
using LanguageExt.UnitsOfMeasure;
using static Echo.Strategy;

namespace Echo
{
    public static partial class Process
    {
        /// <summary>
        /// Default Process strategy if one isn't provided on spawn
        /// </summary>
        public static readonly State<StrategyContext, Unit> DefaultStrategy =
            OneForOne(Always(Directive.Restart));
    }
}
