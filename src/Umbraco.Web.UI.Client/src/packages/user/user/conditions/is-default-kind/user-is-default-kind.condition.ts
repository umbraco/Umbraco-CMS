import { UmbUserKind } from '../../utils/index.js';
import { UmbUserActionConditionBase } from '../user-allow-action-base.condition.js';

export class UmbUserIsDefaultKindCondition extends UmbUserActionConditionBase {
	async _onUserDataChange() {
		// don't allow the current user to delete themselves
		if (this.userKind === UmbUserKind.DEFAULT) {
			this.permitted = true;
		} else {
			this.permitted = false;
		}
	}
}

export { UmbUserIsDefaultKindCondition as api };
