import { UmbUserActionConditionBase } from './user-allow-action-base.condition.js';
import { UserStateModel } from '@umbraco-cms/backoffice/backend-api';
import {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbUserAllowEnableActionCondition extends UmbUserActionConditionBase {
	constructor(args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(args);
	}

	async onUserDataChange() {
		// don't allow the current user to enable themselves
		if (!this.userData || !this.userData.id || (await this.isCurrentUser())) {
			this.permitted = false;
			super.onUserDataChange();
			return;
		}

		this.permitted = this.userData?.state === UserStateModel.DISABLED;
		super.onUserDataChange();
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'User Allow Enable Action Condition',
	alias: 'Umb.Condition.User.AllowEnableAction',
	api: UmbUserAllowEnableActionCondition,
};
