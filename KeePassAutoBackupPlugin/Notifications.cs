/* KeePassAutoBackupPlugin
 * Copyright (C) 2022 Rafael Nockmann @ Nocksoft
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * Dieses Programm ist Freie Software: Sie können es unter den Bedingungen
 * der GNU General Public License, wie von der Free Software Foundation,
 * Version 3 der Lizenz oder (nach Ihrer Wahl) jeder neueren
 * veröffentlichten Version, weiter verteilen und/oder modifizieren.
 * 
 * Dieses Programm wird in der Hoffnung bereitgestellt, dass es nützlich sein wird, jedoch
 * OHNE JEDE GEWÄHR,; sogar ohne die implizite
 * Gewähr der MARKTFÄHIGKEIT oder EIGNUNG FÜR EINEN BESTIMMTEN ZWECK.
 * Siehe die GNU General Public License für weitere Einzelheiten.
 * 
 * Sie sollten eine Kopie der GNU General Public License zusammen mit diesem
 * Programm erhalten haben. Wenn nicht, siehe <https://www.gnu.org/licenses/>.
 */

using KeePassLib.Utility;
using System.Drawing;
using System.Windows.Forms;

namespace KeePassAutoBackupPlugin
{
    internal static class Notifications
    {
        #region Notification Area

        private static void SendNotification(NotifyIcon notify, string title, string message)
        {
            /* For this, the references System.Drawing and System.Windows.Forms must be added to the project. */
            notify.Visible = true;
            notify.BalloonTipTitle = title;
            notify.BalloonTipText = message;
            notify.ShowBalloonTip(1000);
        }

        internal static void SendNotificationInfo(string title, string message)
        {
            var notify = new NotifyIcon();
            notify.Icon = SystemIcons.Information;
            notify.BalloonTipIcon = ToolTipIcon.Info;

            SendNotification(notify, title, message);
        }

        internal static void SendNotificationError(string title, string message)
        {
            var notify = new NotifyIcon();
            notify.Icon = SystemIcons.Error;
            notify.BalloonTipIcon = ToolTipIcon.Error;

            SendNotification(notify, title, message);
        }

        #endregion

        #region MessageBox

        internal static void ShowError(string message)
        {
            MessageService.ShowFatal(message);
        }

        #endregion
    }
}
