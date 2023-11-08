import { UmbUserActionConditionBase } from './user-allow-action-base.condition.js';
import { UserStateModel } from '@umbraco-cms/backoffice/backend-api';
import {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbUserAllowResendInviteActionCondition extends UmbUserActionConditionBase {
	constructor(args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(args);
	}

	async onUserDataChange() {
		if (!this.userData || !this.userData.id) {
			this.permitted = false;
			super.onUserDataChange();
			return;
		}

		this.permitted = this.userData?.state === UserStateModel.INVITED;
		super.onUserDataChange();
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'User Allow Resend Invite Action Condition',
	alias: 'Umb.Condition.User.AllowResendInviteAction',
	api: UmbUserAllowResendInviteActionCondition,
};
