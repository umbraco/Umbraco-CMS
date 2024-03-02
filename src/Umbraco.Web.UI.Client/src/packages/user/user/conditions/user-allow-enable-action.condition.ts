import { UmbUserStateEnum } from '../types.js';
import { UmbUserActionConditionBase } from './user-allow-action-base.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export class UmbUserAllowEnableActionCondition extends UmbUserActionConditionBase {
	async onUserDataChange() {
		// don't allow the current user to enable themselves
		if (!this.userUnique || (await this.isCurrentUser())) {
			this.permitted = false;
			super.onUserDataChange();
			return;
		}

		this.permitted = this.userState === UmbUserStateEnum.DISABLED;
		super.onUserDataChange();
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'User Allow Enable Action Condition',
	alias: 'Umb.Condition.User.AllowEnableAction',
	api: UmbUserAllowEnableActionCondition,
};
