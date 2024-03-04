import { UmbUserActionConditionBase } from '../../../conditions/user-allow-action-base.condition.js';
import { UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export class UmbUserAllowResendInviteActionCondition extends UmbUserActionConditionBase {
	async _onUserDataChange() {
		if (!this.userUnique) {
			this.permitted = false;
			return;
		}

		this.permitted = this.userState === UserStateModel.INVITED;
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'User Allow Resend Invite Action Condition',
	alias: 'Umb.Condition.User.AllowResendInviteAction',
	api: UmbUserAllowResendInviteActionCondition,
};
