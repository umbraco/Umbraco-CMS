import { UmbUserStateEnum } from '../../types.js';
import { UmbUserActionConditionBase } from '../user-allow-action-base.condition.js';

export class UmbUserAllowDisableActionCondition extends UmbUserActionConditionBase {
	async _onUserDataChange() {
		// don't allow the current user to disable themselves
		if (!this.userUnique || (await this.isCurrentUser())) {
			this.permitted = false;
			return;
		}

		this.permitted = this.userState !== UmbUserStateEnum.DISABLED;
	}
}

export { UmbUserAllowDisableActionCondition as api };
