import { UmbUserActionConditionBase } from './user-allow-action-base.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbUserAllowMfaActionCondition extends UmbUserActionConditionBase {
	async _onUserDataChange() {
		// Check if there are any MFA providers available
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
