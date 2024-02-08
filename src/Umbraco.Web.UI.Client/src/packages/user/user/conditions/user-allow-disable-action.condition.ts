import { UmbUserActionConditionBase } from './user-allow-action-base.condition.js';
import { UserStateModel } from '@umbraco-cms/backoffice/backend-api';
import type {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbUserAllowDisableActionCondition extends UmbUserActionConditionBase {
	constructor(args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(args);
	}

	async onUserDataChange() {
		// don't allow the current user to disable themselves
		if (!this.userData || !this.userData.unique || (await this.isCurrentUser())) {
			this.permitted = false;
			super.onUserDataChange();
			return;
		}

		this.permitted = this.userData?.state !== UserStateModel.DISABLED;
		super.onUserDataChange();
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'User Allow Disable Action Condition',
	alias: 'Umb.Condition.User.AllowDisableAction',
	api: UmbUserAllowDisableActionCondition,
};
