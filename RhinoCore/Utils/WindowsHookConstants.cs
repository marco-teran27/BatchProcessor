using System;

namespace BatchProcessorRhino.Utils
{
    /// <summary>
    /// Provides constant values used for Windows keyboard hook operations.
    /// <para>
    /// This static class contains constants that represent:
    /// - The hook type for keyboard events (<see cref="WH_KEYBOARD"/>).
    /// - The hook action code (<see cref="HC_ACTION"/>).
    /// - The virtual key code for the Escape key (<see cref="VK_ESCAPE"/>).
    /// - The key up flag (<see cref="KEY_UP_FLAG"/>).
    /// 
    /// These constants should be used to configure and interpret low-level keyboard hooks.
    /// </para>
    /// </summary>
    public static class WindowsHookConstants
    {
        /// <summary>
        /// Specifies a low-level keyboard hook.
        /// </summary>
        public const int WH_KEYBOARD = 2;

        /// <summary>
        /// Indicates that a hook procedure is processing a keyboard message.
        /// </summary>
        public const int HC_ACTION = 0;

        /// <summary>
        /// The virtual-key code for the Escape key.
        /// </summary>
        public const int VK_ESCAPE = 0x1B;

        /// <summary>
        /// Flag used to indicate that a key-up event has occurred.
        /// <para>
        /// The value 0x80000000 is originally a <c>uint</c>, so it is explicitly cast to <c>int</c>
        /// within an unchecked context to avoid overflow checking.
        /// </para>
        /// </summary>
        public const int KEY_UP_FLAG = unchecked((int)0x80000000);
    }
}
