cd PXWeb\

copy C:\git_adm.ssb.no\configRepos\px-web-ekstern\localhost\konfigfiler\databases.config .
copy C:\git_adm.ssb.no\configRepos\px-web-ekstern\localhost\konfigfiler\setting.config .
copy C:\git_adm.ssb.no\configRepos\px-web-ekstern\localhost\konfigfiler\SqlDb.config .
copy C:\git_adm.ssb.no\configRepos\px-web-ekstern\localhost\konfigfiler\Web.config .


mkdir PXWebIndeksering\
cd PXWebIndeksering\
mkdir utv_24v_ekstern
copy C:\git_adm.ssb.no\configRepos\px-web-ekstern\localhost\konfigfiler\database.config utv_24v_ekstern\.
copy C:\git_adm.ssb.no\configRepos\px-web-ekstern\localhost\konfigfiler\metadata.config utv_24v_ekstern\.
echo "Done."
pause


