import { UmbUserConfigRepository } from '../../repository/config/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbConditionBase, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

export class UmbUserAllowMfaActionCondition extends UmbConditionBase<never> {
	#configRepository = new UmbUserConfigRepository(this._host);

	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	constructor(host: UmbControllerHost, args: any) {
		super(host, args);
		this.#init();
	}

	async #init() {
		await this.#configRepository.initialized;
		this.observe(
			observeMultiple([
				this.#configRepository.part('allowTwoFactor'),
				umbExtensionsRegistry.byType('mfaLoginProvider'),
			]),
			([allowTwoFactor, exts]) => {
				this.permitted = allowTwoFactor && exts.length > 0;
			},
			'_userAllowMfaActionCondition',
		);
	}
}

export { UmbUserAllowMfaActionCondition as api };
