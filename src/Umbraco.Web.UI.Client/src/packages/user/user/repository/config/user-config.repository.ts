import type { UmbUserConfigurationModel } from '../../types.js';
import { UmbUserConfigServerDataSource } from './user-config.server.data-source.js';
import { UMB_USER_CONFIG_STORE_CONTEXT } from './user-config.store.token.js';
import { UmbConfigRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbUserConfigRepository extends UmbConfigRepositoryBase<UmbUserConfigurationModel> {
	readonly #dataSource = new UmbUserConfigServerDataSource(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_USER_CONFIG_STORE_CONTEXT);
	}

	protected override async _requestConfig() {
		const { data } = await this.#dataSource.getUserConfig();
		return data;
	}
}

export default UmbUserConfigRepository;
