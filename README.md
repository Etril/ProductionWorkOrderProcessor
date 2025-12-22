Ce projet est un worker .NET dédié au traitement d'ordres de travail, avec un focus "Clean Architecture", sur la testabilité et la possibilité d'étendre le projet facilement. 

** Points clés: **

-Clean Architecture, avec séparation Domain/Application Infrastrructure                                                         

-Traitement des commandes idempotent, evitant les ordres doublons.

-Retry policy avec delais et nombres d'essais configurables facilement. 

-DLQ pour les ordres irrécupérables. 

-Persistence via SQLite et EFCore.

-Tests unitaires et d'intégration, pipeline CI via GitHub Actions. 

-Infrastructure extensible et amovible. 

** Apports du projet: ** 

-Design de systems Back-end en dehors du CRUD classique. 

-Traitement de messages, commandes ou ordres de travail. 

-Stratégies de gestion des échecs, comme un service externe indisponible. 




