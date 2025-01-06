import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbConditionBase, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { UmbUserConfigRepository } from '../../repository/config/index.js';

export class UmbUserAllowMfaActionCondition extends UmbConditionBase<never> {
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	constructor(host: UmbControllerHost, args: any) {
		super(host, args);

		const repository = new UmbUserConfigRepository(host);

		this.observe(repository.initialized, () => {
			this.observe(
				observeMultiple([repository.part('allowTwoFactor'), umbExtensionsRegistry.byType('mfaLoginProvider')]),
				([allowTwoFactor, exts]) => {
					this.permitted = allowTwoFactor && exts.length > 0;
				},
				'_userAllowMfaActionCondition',
			);
		});
	}
}

export { UmbUserAllowMfaActionCondition as api };
