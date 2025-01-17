import { UMB_CURRENT_USER_CONTEXT } from '../../current-user.context.token.js';
import type { UmbCurrentUserModel } from '../../types.js';
import type { UmbCurrentUserGroupIdConditionConfig } from './types.js';
import { stringOrStringArrayIntersects } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbCurrentUserGroupCondition
	extends UmbConditionBase<UmbCurrentUserGroupIdConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbCurrentUserGroupIdConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(context.currentUser, this.observeCurrentUser, 'umbCurrentUserGroupConditionObserver');
		});
	}

	private observeCurrentUser = async (currentUser: UmbCurrentUserModel) => {
		const { match, oneOf, allOf, noneOf } = this.config;

		if (match) {
			if (currentUser.userGroupIds.includes(match)) {
				this.permitted = true;
				return;
			}

			this.permitted = false;
			return;
		}

		if (oneOf) {
			if (stringOrStringArrayIntersects(oneOf, currentUser.userGroupIds)) {
				this.permitted = true;
				return;
			}

			this.permitted = false;
			return;
		}

		if (allOf) {
			if (allOf.every((group) => currentUser.userGroupIds.includes(group))) {
				this.permitted = true;
				return;
			}

			this.permitted = false;
			return;
		}

		if (noneOf) {
			if (noneOf.some((group) => currentUser.userGroupIds.includes(group))) {
				this.permitted = false;
				return;
			}
		}

		this.permitted = true;
	};
}

export { UmbCurrentUserGroupCondition as api };
