import { UMB_CURRENT_USER_CONTEXT } from '../../current-user.context.token.js';
import type { UmbCurrentUserModel } from '../../types.js';
import type { UmbCurrentUserGroupConditionConfig } from './types.js';
import { stringOrStringArrayIntersects } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbCurrentUserGroupCondition
	extends UmbConditionBase<UmbCurrentUserGroupConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbCurrentUserGroupConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(context.currentUser, this.observeCurrentUser, 'umbCurrentUserGroupConditionObserver');
		});
	}

	private observeCurrentUser = async (currentUser: UmbCurrentUserModel) => {
		const { grant, deny } = this.config;

		if (typeof grant === 'undefined' && typeof deny === 'undefined') {
			console.warn('[UmbCurrentUserGroupCondition] No grant or deny specified');
			this.permitted = false;
			return;
		}

		if (grant) {
			if (stringOrStringArrayIntersects(grant, currentUser.userGroupIds)) {
				this.permitted = true;
				return;
			}

			this.permitted = false;
			return;
		}

		if (deny) {
			if (stringOrStringArrayIntersects(deny, currentUser.userGroupIds)) {
				this.permitted = false;
				return;
			}
		}

		this.permitted = true;
	};
}

export { UmbCurrentUserGroupCondition as api };
