import UmbCurrentUserConfigRepository from '../../repository/config/current-user-config.repository.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbCurrentUserAllowChangePasswordActionCondition extends UmbConditionBase<UmbConditionConfigBase> {
	#configRepository = new UmbCurrentUserConfigRepository(this._host);

	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	constructor(host: UmbControllerHost, args: any) {
		super(host, args);
		this.#init();
	}

	async #init() {
		await this.#configRepository.initialized;
		this.observe(
			this.#configRepository.part('allowChangePassword'),
			(isAllowed) => {
				this.permitted = isAllowed;
			},
			'_userAllowChangePasswordActionCondition',
		);
	}
}

export { UmbCurrentUserAllowChangePasswordActionCondition as api };
