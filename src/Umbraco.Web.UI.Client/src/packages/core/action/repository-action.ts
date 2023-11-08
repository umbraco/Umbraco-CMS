import { UmbBaseController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { type ExtensionApi, createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbActionBase<RepositoryType> extends UmbBaseController implements ExtensionApi {
	repository?: RepositoryType;

	constructor(host: UmbControllerHostElement, repositoryAlias: string) {
		super(host);

		// TODO: Could be replaced with ExtensionApiController?
		this.observe(
			umbExtensionsRegistry.getByTypeAndAlias('repository', repositoryAlias),
			async (repositoryManifest) => {
				if (!repositoryManifest) return;

				try {
					const result = await createExtensionApi(repositoryManifest, [this._host]);
					this.repository = result as RepositoryType;
				} catch (error) {
					throw new Error('Could not create repository with alias: ' + repositoryAlias + '');
				}
			}
		);
	}
}
