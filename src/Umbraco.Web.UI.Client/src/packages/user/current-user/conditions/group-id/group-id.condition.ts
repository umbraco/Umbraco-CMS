import { UMB_CURRENT_USER_CONTEXT } from '../../current-user.context.token.js';
import type { UmbCurrentUserModel } from '../../types.js';
import type { UmbCurrentUserGroupIdConditionConfig } from './types.js';
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
		// Idea: This part could be refactored to become a shared util, to align these matching feature across conditions. [NL]
		// Notice doing so it would be interesting to invistigate if it makes sense to combine some of these properties, to enable more specific matching. (But maybe it is only relevant for the combination of match + oneOf) [NL]
		const { match, oneOf, allOf, noneOf } = this.config;

		if (match) {
			this.permitted = currentUser.userGroupUniques.includes(match);
			return;
		}

		if (oneOf) {
			this.permitted = oneOf.some((v) => currentUser.userGroupUniques.includes(v));
			return;
		}

		if (allOf) {
			this.permitted = allOf.every((v) => currentUser.userGroupUniques.includes(v));
			return;
		}

		if (noneOf) {
			if (noneOf.some((v) => currentUser.userGroupUniques.includes(v))) {
				this.permitted = false;
				return;
			}
		}

		this.permitted = true;
	};
}

export { UmbCurrentUserGroupCondition as api };
