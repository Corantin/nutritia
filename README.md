Proc�dure de d�ploiement du logiciel Nutritia

1. Cr�er une base de donn�es locale
	1a. Lancer le script de cr�ation de base de donn�es (Nutritia_Creation.sql)
	1b. Lancer le script d'insertion de donn�es (Nutritia_Insertion.sql)
2. Modifier le fichier app.config
	Changer le nom de la base de donn�es dans la ligne suivante en fonction de celle choisie plus haut
	<add name="MySqlConnexion" connectionString="server=localhost;userid=root;password=;database=420-5A5-A15_Nutritia" />