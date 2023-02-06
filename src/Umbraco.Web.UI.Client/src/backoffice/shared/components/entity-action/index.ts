import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { UmbObserverController } from '@umbraco-cms/observable-api';
import { createExtensionClass } from 'libs/extensions-api/create-extension-class.function';

export interface UmbEntityAction<T> {
	unique: string;
	repository: T;
	execute(): Promise<void>;
}

export class UmbEntityActionBase<T> {
	host: UmbControllerHostInterface;
	unique: string;
	repository?: T;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		this.host = host;
		this.unique = unique;

		// TODO: unsure a method can't be called before everything is initialized
		new UmbObserverController(
			this.host,
			umbExtensionsRegistry.getByTypeAndAlias('repository', repositoryAlias),
			async (repositoryManifest) => {
				if (!repositoryManifest) return;

				try {
					const result = await createExtensionClass<T>(repositoryManifest, [this.host]);
					this.repository = result;
				} catch (error) {
					throw new Error('Could not create repository with alias: ' + repositoryAlias + '');
				}
			}
		);
	}
}
