import { UmbUserStateEnum } from '../types.js';
import { UmbUserActionConditionBase } from './user-allow-action-base.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export class UmbUserAllowUnlockActionCondition extends UmbUserActionConditionBase {
	async _onUserDataChange() {
		// don't allow the current user to unlock themselves
		if (!this.userUnique || (await this.isCurrentUser())) {
			this.permitted = false;
			return;
		}

		this.permitted = this.userState === UmbUserStateEnum.LOCKED_OUT;
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'User Allow Unlock Action Condition',
	alias: 'Umb.Condition.User.AllowUnlockAction',
	api: UmbUserAllowUnlockActionCondition,
};
