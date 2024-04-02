import { UmbUserActionConditionBase } from './user-allow-action-base.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbUserAllowMfaActionCondition extends UmbUserActionConditionBase {
	async _onUserDataChange() {
		const isCurrentUser = await this.isCurrentUser();
		let isAdmin = false;

		// If it's not the current user being clicked, we need to check if the current logged-in user is an admin
		if (!isCurrentUser) {
			isAdmin = await this.isCurrentUserAdmin();
		}

		const isCurrentUserOrAdmin = isCurrentUser || isAdmin;

		this.observe(
			umbExtensionsRegistry.byType('mfaLoginProvider'),
			(exts) => (this.permitted = isCurrentUserOrAdmin && exts.length > 0),
			'_hasProviders',
		);
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'User Allow Mfa Action Condition',
	alias: 'Umb.Condition.User.AllowMfaAction',
	api: UmbUserAllowMfaActionCondition,
};
