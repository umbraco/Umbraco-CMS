import { UmbUserActionConditionBase } from '../../../conditions/user-allow-action-base.condition.js';
import { UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbUserAllowResendInviteActionCondition extends UmbUserActionConditionBase {
	async _onUserDataChange() {
		if (!this.userUnique) {
			this.permitted = false;
			return;
		}

		this.permitted = this.userState === UserStateModel.INVITED;
	}
}

export { UmbUserAllowResendInviteActionCondition as api };
