import UmbUserConfigRepository from '../../repository/config/user-config.repository.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbUserAllowChangePasswordActionCondition extends UmbConditionBase<UmbConditionConfigBase> {
	constructor(host: UmbControllerHost, args: any) {
		super(host, args);

		const configRepository = new UmbUserConfigRepository(host);

		this.observe(configRepository.initialized, () => {
			this.observe(
				configRepository.part('allowChangePassword'),
				(isAllowed) => {
					this.permitted = isAllowed;
				},
				'_userAllowChangePasswordActionCondition',
			);
		});
	}
}

export { UmbUserAllowChangePasswordActionCondition as api };
