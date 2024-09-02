import { UmbUserModelKind } from '../../utils/index.js';
import { UmbUserActionConditionBase } from '../user-allow-action-base.condition.js';

export class UmbUserAllowChangePasswordActionCondition extends UmbUserActionConditionBase {
	async _onUserDataChange() {
		// don't allow the current user to delete themselves
		if (this.userKind === UmbUserModelKind.DEFAULT) {
			this.permitted = true;
		} else {
			this.permitted = false;
		}
	}
}

export { UmbUserAllowChangePasswordActionCondition as api };
