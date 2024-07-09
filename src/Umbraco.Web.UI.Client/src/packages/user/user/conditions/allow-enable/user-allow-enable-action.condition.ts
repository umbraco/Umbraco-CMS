import { UmbUserStateEnum } from '../../types.js';
import { UmbUserActionConditionBase } from '../user-allow-action-base.condition.js';

export class UmbUserAllowEnableActionCondition extends UmbUserActionConditionBase {
	async _onUserDataChange() {
		// don't allow the current user to enable themselves
		if (!this.userUnique || (await this.isCurrentUser())) {
			this.permitted = false;
			return;
		}

		this.permitted = this.userState === UmbUserStateEnum.DISABLED;
	}
}

export { UmbUserAllowEnableActionCondition as api };
