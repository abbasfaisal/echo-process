﻿using LanguageExt;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static LanguageExt.Prelude;
using static Echo.Process;

namespace Echo
{
    /// <summary>
    /// <para>
    ///     Process: Ask functions
    /// </para>
    /// <para>
    ///     'ask' is a request/response system for processes.  You can ask a process a question (a message) and it
    ///     can reply using the 'Process.reply' function.  It doesn't have to and 'ask' will timeout after 
    ///     ActorConfig.Default.Timeout seconds. 
    /// </para>
    /// <para>
    ///     'ask' is blocking, because mostly it will be called from within a process and processes shouldn't 
    ///     perform asynchronous operations.
    /// </para>
    /// </summary>
    public static partial class Process
    {
        /// <summary>
        /// Ask a process for a reply
        /// </summary>
        /// <param name="pid">Process to ask</param>
        /// <param name="message">Message to send</param>
        /// <param name="sender">Sender process</param>
        /// <returns>The response to the request</returns>
        public static T ask<T>(ProcessId pid, object message, ProcessId sender) =>
            ActorContext.System(pid).Ask<T>(pid, message, sender);

        /// <summary>
        /// Ask a process for a reply
        /// </summary>
        /// <param name="pid">Process to ask</param>
        /// <param name="message">Message to send</param>
        /// <returns>The response to the request</returns>
        public static T ask<T>(ProcessId pid, object message) =>
            ask<T>(pid, message, Self);

        /// <summary>
        /// Asynchronous ask - must be used outside of a Process
        /// </summary>
        /// <typeparam name="R">Type of the return value</typeparam>
        /// <param name="pid">Process to ask</param>
        /// <param name="message">Message to send</param>
        /// <param name="sender">Sender process</param>
        /// <returns>A promise to return a response to the request</returns>
        public static Task<R> askAsync<R>(ProcessId pid, object message, ProcessId sender) =>
            InMessageLoop
                ? raiseDontUseInMessageLoopException<Task<R>>(nameof(observeState))
                : Task.Run(() => ask<R>(pid, message, sender));

        /// <summary>
        /// Ask a process for a reply (if the process is running).  If the process isn't running
        /// then None is returned
        /// </summary>
        /// <param name="pid">Process to ask</param>
        /// <param name="message">Message to send</param>
        /// <param name="sender">Sender process</param>
        /// <returns>The response to the request or None if the process isn't running</returns>
        public static Option<T> askIfAlive<T>(ProcessId pid, object message, ProcessId sender) =>
            ping(pid)
                ? Optional(ActorContext.System(pid).Ask<T>(pid, message, sender))
                : None;

        /// <summary>
        /// Ask a process for a reply (if the process is running).  If the process isn't running
        /// then None is returned
        /// </summary>
        /// <param name="pid">Process to ask</param>
        /// <param name="message">Message to send</param>
        /// <returns>The response to the request or None if the process isn't running</returns>
        public static Option<T> askIfAlive<T>(ProcessId pid, object message) =>
            ping(pid)
                ? Optional(ask<T>(pid, message, Self))
                : None;

        /// <summary>
        /// Asynchronous ask - must be used outside of a Process
        /// </summary>
        /// <typeparam name="R">Type of the return value</typeparam>
        /// <param name="pid">Process to ask</param>
        /// <param name="message">Message to send</param>
        /// <returns>A promise to return a response to the request</returns>
        public static Task<R> askAsync<R>(ProcessId pid, object message) =>
            askAsync<R>(pid, message, Self);

        /// <summary>
        /// Ask children the same message
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns></returns>
        public static IEnumerable<T> askChildren<T>(object message, int take = Int32.MaxValue) =>
            ActorContext.System(default(SystemName)).AskMany<T>(Children.Values.ToSeq(), message, take);

        /// <summary>
        /// Ask parent process for a reply
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>The response to the request</returns>
        public static T askParent<T>(object message) =>
            ask<T>(Parent, message);

        /// <summary>
        /// Ask a named child process for a reply
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="name">Name of the child process</param>
        public static T askChild<T>(ProcessName name, object message) =>
            ask<T>(Self.Child(name), message);

        /// <summary>
        /// Ask a child process (found by index) for a reply
        /// </summary>
        /// <remarks>
        /// Because of the potential changeable nature of child nodes, this will
        /// take the index and mod it by the number of children.  We expect this 
        /// call will mostly be used for load balancing, and round-robin type 
        /// behaviour, so feel that's acceptable.  
        /// </remarks>
        /// <param name="message">Message to send</param>
        /// <param name="index">Index of the child process (see remarks)</param>
        public static T askChild<T>(int index, object message) =>
            ask<T>(child(index), message);
    }
}
