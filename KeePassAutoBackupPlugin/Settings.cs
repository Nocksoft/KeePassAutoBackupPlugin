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

using Nocksoft.IO.ConfigFiles;
using System;
using System.IO;
using System.Reflection;

namespace KeePassAutoBackupPlugin
{
    internal static class Settings
    {
        internal static string BackupPath { get; set; }
        internal static bool BackupInSourceDir { get; private set; }
        internal static string[] BackupExclusions { get; set; }
        internal static int BackupsPreserve { get; private set; }

        internal static bool BackupOnDatabaseExit { get; private set; }
        internal static bool BackupOnDatabaseChange { get; private set; }
        internal static bool BackupOnlyWhenDatabaseHasChanged { get; private set; }

        internal static void LoadSettings()
        {
            string assembly = Assembly.GetExecutingAssembly().Location;
            var ini = new INIFile(Path.Combine(Path.GetDirectoryName(assembly), "KeePassAutoBackupPlugin.dll.ini"));
            string valueStr;

            /* BackupPath */
            BackupPath = ini.GetValue("config", "BackupPath");
            if (!string.IsNullOrWhiteSpace(BackupPath))
            {
                BackupPath = ReplaceAppData(BackupPath);
                BackupInSourceDir = false;
            }
            else
                BackupInSourceDir = true;

            /* BackupExclusions */
            valueStr = ini.GetValue("config", "BackupExclusions");
            if (!string.IsNullOrWhiteSpace(valueStr))
            {
                BackupExclusions = valueStr.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
                BackupExclusions = null;

            /* BackupsPreserve */
            int valueInt;
            valueStr = ini.GetValue("config", "BackupsPreserve");
            if (int.TryParse(valueStr, out valueInt))
            {
                BackupsPreserve = valueInt;
            }
            else
            {
                throw new Exception(string.Format("Ungüliger Wert für BackupsPreserve \"{0}\".", valueStr));
            }


            /* BackupOnDatabaseExit */
            valueStr = ini.GetValue("config", "BackupOnDatabaseExit");
            if (valueStr == "true" || valueStr == "yes" || valueStr == "1") BackupOnDatabaseExit = true;
            else BackupOnDatabaseExit = false;

            /* BackupOnDatabaseChange */
            valueStr = ini.GetValue("config", "BackupOnDatabaseChange");
            if (valueStr == "true" || valueStr == "yes" || valueStr == "1") BackupOnDatabaseChange = true;
            else BackupOnDatabaseChange = false;

            /* BackupOnlyWhenDatabaseHasChanged */
            valueStr = ini.GetValue("config", "BackupOnlyWhenDatabaseHasChanged");
            if (valueStr == "true" || valueStr == "yes" || valueStr == "1") BackupOnlyWhenDatabaseHasChanged = true;
            else BackupOnlyWhenDatabaseHasChanged = false;
        }

        private static string ReplaceAppData(string path)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (path.Contains("%AppData%"))
                path = path.Replace("%AppData%", appData);
            return path;
        }
    }
}
