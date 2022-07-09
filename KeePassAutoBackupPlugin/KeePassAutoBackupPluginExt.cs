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

using KeePass.Forms;
using KeePass.Plugins;
using System;
using System.Collections.Generic;
using System.IO;

namespace KeePassAutoBackupPlugin
{
    public sealed class KeePassAutoBackupPluginExt : Plugin
    {
        private IPluginHost m_host = null;

        // Key = full path to database; Value = database has been modified and backup pending?
        private Dictionary<string, bool> _Databases = new Dictionary<string, bool>();

        public override bool Initialize(IPluginHost host)
        {
            m_host = host;

            Settings.LoadSettings();

            m_host.MainWindow.FileOpened += DatabaseOpened;
            m_host.MainWindow.FileSaving += DatabaseSaving;
            m_host.MainWindow.FileSaved += DatabaseSaved;
            m_host.MainWindow.FileClosed += DatabaseClosed;

            return true;
        }

        public override void Terminate()
        {
            m_host.MainWindow.FileOpened -= DatabaseOpened;
            m_host.MainWindow.FileSaving -= DatabaseSaving;
            m_host.MainWindow.FileSaved -= DatabaseSaved;
            m_host.MainWindow.FileClosed -= DatabaseClosed;
        }

        #region KeePass Events

        private void DatabaseOpened(object sender, FileOpenedEventArgs e)
        {
            _Databases.Add(GetFullDatabasePath(e), false);
        }

        private void DatabaseSaving(object sender, FileSavingEventArgs e)
        {
            bool databaseHasChanged = e.Database.Modified;

            if (Settings.BackupOnDatabaseExit == true && Settings.BackupOnDatabaseChange == false)
            {
                if (databaseHasChanged == true)
                    _Databases[e.Database.IOConnectionInfo.Path] = databaseHasChanged;
            }
            else
                _Databases[e.Database.IOConnectionInfo.Path] = databaseHasChanged;
        }

        private void DatabaseSaved(object sender, FileSavedEventArgs e)
        {
            if (Settings.BackupOnDatabaseChange == false) return;

            string database = e.Database.IOConnectionInfo.Path;
            BackupDatabase(database);
        }

        private void DatabaseClosed(object sender, FileClosedEventArgs e)
        {
            string database = e.IOConnectionInfo.Path;
            if (Settings.BackupOnDatabaseExit == true)
                BackupDatabase(database);

            _Databases.Remove(database);
        }

        #endregion

        private string GetFullDatabasePath(FileOpenedEventArgs e)
        {
            return new FileInfo(e.Database.IOConnectionInfo.Path).FullName;
        }

        private void BackupDatabase(string database)
        {
            var backup = new Backup();

            foreach (KeyValuePair<string, bool> item in _Databases)
            {
                if (item.Key != database) continue;
                if (Settings.BackupOnlyWhenDatabaseHasChanged == true && item.Value == false) continue;


                if (Settings.BackupInSourceDir == true)
                    Settings.BackupPath = Path.GetDirectoryName(item.Key);

                try
                {
                    backup.CreateBackup(item.Key);
                    backup.CleanBackups(item.Key);

                    _Databases[item.Key] = false;
                }
                catch (Exception ex)
                {
                    string message = $"Error creating backup of \"{item.Key}\":" + Environment.NewLine + ex.ToString();
                    Notifications.SendNotificationError("Backup failed", message);
                    Notifications.ShowError(message);
                }

                break;
            }
        }
    }
}
