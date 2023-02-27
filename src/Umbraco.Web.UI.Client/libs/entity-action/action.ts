import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { umbExtensionsRegistry, createExtensionClass } from '@umbraco-cms/extensions-api';
import { UmbObserverController } from '@umbraco-cms/observable-api';

export interface UmbAction<RepositoryType> {
	host: UmbControllerHostInterface;
	repository: RepositoryType;
	execute(): Promise<void>;
}

export class UmbActionBase<RepositoryType> {
	host: UmbControllerHostInterface;
	repository?: RepositoryType;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string) {
		this.host = host;

		// TODO: unsure a method can't be called before everything is initialized
		new UmbObserverController(
			this.host,
			umbExtensionsRegistry.getByTypeAndAlias('repository', repositoryAlias),
			async (repositoryManifest) => {
				if (!repositoryManifest) return;

				try {
					const result = await createExtensionClass<RepositoryType>(repositoryManifest, [this.host]);
					this.repository = result;
				} catch (error) {
					throw new Error('Could not create repository with alias: ' + repositoryAlias + '');
				}
			}
		);
	}
}
