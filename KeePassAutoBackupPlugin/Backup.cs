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

using System;
using System.IO;
using System.Linq;

namespace KeePassAutoBackupPlugin
{
    internal class Backup
    {
        internal void CreateBackup(string database)
        {
            if (string.IsNullOrWhiteSpace(Settings.BackupPath))
                throw new Exception($"Invalid value for BackupPath \"{Settings.BackupPath}\".");


            if (!Directory.Exists(Settings.BackupPath))
                Directory.CreateDirectory(Settings.BackupPath);

            
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
            string targetFilename = Path.GetFileNameWithoutExtension(database) + "_" + timestamp + Path.GetExtension(database);
            string target = Path.Combine(Settings.BackupPath, targetFilename);

            File.Copy(database, target, true);

            Notifications.SendNotificationInfo("Backup created", $"Backup created successfully: {target}");
        }

        internal void CleanBackups(string database)
        {
            if (string.IsNullOrWhiteSpace(Settings.BackupPath))
                throw new Exception($"Invalid value for BackupPath \"{Settings.BackupPath}\".");


            string pattern = Path.GetFileNameWithoutExtension(database) + "_????????_????.kdbx";
            var backupedFiles = Directory.GetFiles(Settings.BackupPath, pattern).ToList();

            for (int i = 0; i < backupedFiles.Count; i++)
            {
                if (backupedFiles.Count - i <= Settings.BackupsPreserve) break;
                File.Delete(backupedFiles[i]);
            }
        }
    }
}
