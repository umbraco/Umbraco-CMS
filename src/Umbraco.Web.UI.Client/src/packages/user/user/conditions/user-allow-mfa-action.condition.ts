import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbUserAllowMfaActionCondition extends UmbConditionBase<never> {
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	constructor(host: UmbControllerHost, args: any) {
		super(host, args);

		// Check if there are any MFA providers available
		this.permitted = false;
		this.observe(
			umbExtensionsRegistry.byType('mfaLoginProvider'),
			(exts) => (this.permitted = exts.length > 0),
			'_userAllowMfaActionConditionProviders',
		);
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'User Allow Mfa Action Condition',
	alias: 'Umb.Condition.User.AllowMfaAction',
	api: UmbUserAllowMfaActionCondition,
};
