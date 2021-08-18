cd PXWeb\

copy C:\git_adm.ssb.no\configRepos\px-web-intern\localhost\konfigfiler\databases.config .
copy C:\git_adm.ssb.no\configRepos\px-web-intern\localhost\konfigfiler\setting.config .
copy C:\git_adm.ssb.no\configRepos\px-web-intern\localhost\konfigfiler\SqlDb.config .
copy C:\git_adm.ssb.no\configRepos\px-web-intern\localhost\konfigfiler\Web.config .

cd Resources\PX\Databases\
mkdir utv_24v_intern
copy C:\git_adm.ssb.no\configRepos\px-web-intern\localhost\MetaId\database.config utv_24v_intern\.
copy C:\git_adm.ssb.no\configRepos\px-web-intern\localhost\MetaId\metadata.config utv_24v_intern\.


echo "Done."
pause


