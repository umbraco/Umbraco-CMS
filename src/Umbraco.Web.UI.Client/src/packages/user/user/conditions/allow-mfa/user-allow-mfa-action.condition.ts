import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbConditionBase, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbUserAllowMfaActionCondition extends UmbConditionBase<never> {
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	constructor(host: UmbControllerHost, args: any) {
		super(host, args);

		// Check if there are any MFA providers available
		this.observe(
			umbExtensionsRegistry.byType('mfaLoginProvider'),
			(exts) => (this.permitted = exts.length > 0),
			'_userAllowMfaActionConditionProviders',
		);
	}
}

export { UmbUserAllowMfaActionCondition as api };
