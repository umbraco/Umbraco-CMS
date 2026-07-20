import type { UmbCurrentUserConfigurationModel } from '../../user/types.js';
import { UmbCurrentUserConfigServerDataSource } from './current-user-config.server.data-source.js';
import { UMB_CURRENT_USER_CONFIG_STORE_CONTEXT } from './current-user-config.store.token.js';
import { UmbConfigRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbCurrentUserConfigRepository extends UmbConfigRepositoryBase<UmbCurrentUserConfigurationModel> {
	#dataSource = new UmbCurrentUserConfigServerDataSource(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_CURRENT_USER_CONFIG_STORE_CONTEXT);
	}

	protected override async _requestConfig() {
		const { data } = await this.#dataSource.getCurrentUserConfig();
		return data;
	}
}

export default UmbCurrentUserConfigRepository;
