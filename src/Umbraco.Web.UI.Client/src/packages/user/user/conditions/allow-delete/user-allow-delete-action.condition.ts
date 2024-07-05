import { UmbUserActionConditionBase } from './user-allow-action-base.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export class UmbUserAllowDeleteActionCondition extends UmbUserActionConditionBase {
	async _onUserDataChange() {
		// don't allow the current user to delete themselves
		if (!this.userUnique || (await this.isCurrentUser())) {
			this.permitted = false;
		} else {
			this.permitted = true;
		}
	}
}
