import { UmbUserActionConditionBase } from './user-allow-action-base.condition.js';
import {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbUserAllowDeleteActionCondition extends UmbUserActionConditionBase {
	constructor(args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(args);
	}

	async onUserDataChange() {
		// don't allow the current user to delete themselves
		if (!this.userData || !this.userData.id || (await this.isCurrentUser())) {
			this.permitted = false;
		} else {
			this.permitted = true;
		}
		super.onUserDataChange();
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'User Allow Delete Action Condition',
	alias: 'Umb.Condition.User.AllowDeleteAction',
	api: UmbUserAllowDeleteActionCondition,
};
