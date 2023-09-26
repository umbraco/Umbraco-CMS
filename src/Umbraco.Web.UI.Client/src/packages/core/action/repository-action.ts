import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

export class UmbActionBase<RepositoryType> {
	host: UmbControllerHostElement;
	repository?: RepositoryType;

	constructor(host: UmbControllerHostElement, repositoryAlias: string) {
		this.host = host;

		// TODO: unsure a method can't be called before everything is initialized
		new UmbObserverController(
			this.host,
			umbExtensionsRegistry.getByTypeAndAlias('repository', repositoryAlias),
			async (repositoryManifest) => {
				if (!repositoryManifest) return;

				try {
					const result = await createExtensionApi<RepositoryType>(repositoryManifest, [this.host]);
					this.repository = result;
				} catch (error) {
					throw new Error('Could not create repository with alias: ' + repositoryAlias + '');
				}
			}
		);
	}
}
