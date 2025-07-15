import { UmbUserStateEnum } from '../../types.js';
import { UmbUserActionConditionBase } from '../user-allow-action-base.condition.js';

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

export { UmbUserAllowUnlockActionCondition as api };
