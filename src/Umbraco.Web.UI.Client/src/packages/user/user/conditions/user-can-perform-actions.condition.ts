import { UmbUserActionConditionBase } from './user-allow-action-base.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

/**
 * Condition that checks if the current user is allowed to perform actions on the selected user.
 * The logic is generally laid out so that admins can perform actions on any user, while users can only perform actions on other users.
 */
export class UmbUserCanPerformActionsCondition extends UmbUserActionConditionBase {
	async _onUserDataChange() {
		// Must have selected a user
		if (this.userUnique === undefined) {
			this.permitted = false;
			return;
		}

		// If the user is the current user, they can perform actions
		if (await this.isCurrentUser()) {
			this.permitted = true;
			return;
		}

		// If the current user is an admin, they can perform actions on any user
		if (await this.isCurrentUserAdmin()) {
			this.permitted = true;
			return;
		}

		// Otherwise, the current user can only perform actions on other users
		if (this.userAdmin) {
			this.permitted = false;
			return;
		}

		// The current user seems to be able to perform actions on the selected user
		this.permitted = true;
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'User Can Perform Actions Condition',
	alias: 'Umb.Condition.User.CanPerformActions',
	api: UmbUserCanPerformActionsCondition,
};
