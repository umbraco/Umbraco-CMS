import { UmbUserActionConditionBase } from '../user-allow-action-base.condition.js';

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

export { UmbUserAllowDeleteActionCondition as api };
