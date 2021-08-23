# Kit de démarrage FranceConnect - Fournisseur de service (FS)

Ce projet exemple propose un canevas de site web en [ASP.NET Core](https://dotnet.microsoft.com/learn/aspnet/what-is-aspnet-core) qui s’appuie sur FranceConnect pour identifier les internautes qui le souhaitent. La version utilisée est ASP.NET Core 3.1 (LTS), ainsi que .NET 5.0 sur la branche concernée. Vous pouvez trouver sur ce répertoire **[la documentation associée](/Documentation)**.

Le canevas ainsi proposé peut être executé localement en suivant **[le tutoriel de démarrage rapide](/Source/README.md)** fourni dans ce répertoire, ou être testé directement à l'adresse <https://aka.ms/FranceConnect-FS>, sur laquelle le code de ce répertoire GitHub est continuellement déployé. Utilisez un des fournisseurs d'identité de démonstration lors de la connexion.

La plateforme [FranceConnect](https://franceconnect.gouv.fr/) est un système d’identification visant à faciliter l’accès des usagers aux services numériques de l’administration en ligne, en évitant à tout un chacun de devoir créer un nouveau compte lors de l’accès à un nouveau service et donc à se remémorer différents mots de passe pour l’ensemble des services accédés.

Pour cela, le système FranceConnect permet à chaque usager de disposer d’un mécanisme d’identification reconnu par les téléservices de l’administration au travers du bouton FranceConnect. Lors de l’accès à un nouveau service, et au-delà de la possibilité toujours proposée de s’inscrire vis-à-vis d’une autorité administrative que ne connaîtrait pas encore l’usager, le bouton permet de sélectionner une identité compatible dont l’usager disposerait déjà (impôts, assurance maladie, identité numérique la poste, etc.) et de l’utiliser dans ce contexte.

FranceConnect est porté par la direction interministérielle du numérique, ou DINUM, qui accompagne les ministères dans leur transformation numérique, conseille le gouvernement et développe des services et ressources partagées comme ici ce système d’identification et d’authentification en ligne de l’État, ou encore le réseau interministériel de l’État, data.gouv.fr ou api.gouv.fr.

La documentation d'implémentation du service FranceConnect est disponible sur leur [portail partenaires](https://partenaires.franceconnect.gouv.fr/fcp/fournisseur-service)
