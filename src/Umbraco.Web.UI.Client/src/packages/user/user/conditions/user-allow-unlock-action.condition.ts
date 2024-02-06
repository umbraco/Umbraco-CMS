import { UmbUserActionConditionBase } from './user-allow-action-base.condition.js';
import { UserStateModel } from '@umbraco-cms/backoffice/backend-api';
import type {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbUserAllowUnlockActionCondition extends UmbUserActionConditionBase {
	constructor(args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(args);
	}

	async onUserDataChange() {
		// don't allow the current user to unlock themselves
		if (!this.userData || !this.userData.id || (await this.isCurrentUser())) {
			this.permitted = false;
			super.onUserDataChange();
			return;
		}

		this.permitted = this.userData?.state === UserStateModel.LOCKED_OUT;
		super.onUserDataChange();
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'User Allow Unlock Action Condition',
	alias: 'Umb.Condition.User.AllowUnlockAction',
	api: UmbUserAllowUnlockActionCondition,
};
