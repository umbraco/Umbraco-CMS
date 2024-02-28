import { UmbUserActionConditionBase } from './user-allow-action-base.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export class UmbUserAllowDeleteActionCondition extends UmbUserActionConditionBase {
	async onUserDataChange() {
		// don't allow the current user to delete themselves
		if (!this.userUnique || (await this.isCurrentUser())) {
			this.permitted = false;
		} else {
			this.permitted = true;
		}
		super.onUserDataChange();
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'User Allow Delete Action Condition',
	alias: 'Umb.Condition.User.AllowDeleteAction',
	api: UmbUserAllowDeleteActionCondition,
};
