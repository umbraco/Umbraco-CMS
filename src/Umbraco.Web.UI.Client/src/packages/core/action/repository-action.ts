import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { type UmbApi, UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbActionBase<RepositoryType> extends UmbControllerBase implements UmbApi {
	repository?: RepositoryType;

	constructor(host: UmbControllerHost, repositoryAlias: string) {
		super(host);

		new UmbExtensionApiInitializer(this, umbExtensionsRegistry, repositoryAlias, [this._host], (permitted, ctrl) => {
			this.repository = permitted ? (ctrl.api as RepositoryType) : undefined;
		});
	}
}
