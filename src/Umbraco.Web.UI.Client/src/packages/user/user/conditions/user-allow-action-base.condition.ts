import type { UmbUserStateEnum } from '../types.js';
import { UMB_USER_WORKSPACE_CONTEXT } from '../workspace/user-workspace.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { isCurrentUser } from '@umbraco-cms/backoffice/current-user';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

export abstract class UmbUserActionConditionBase
	extends UmbConditionBase<UmbConditionConfigBase>
	implements UmbExtensionCondition
{
	/**
	 * The unique identifier of the user being edited
	 */
	protected userUnique?: string;

	/**
	 * The state of the user being edited
	 */
	protected userState?: UmbUserStateEnum | null;

	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);

		this.consumeContext(UMB_USER_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				observeMultiple([context.unique, context.state]),
				([unique, state]) => {
					this.userUnique = unique;
					this.userState = state;
					this._onUserDataChange();
				},
				'_umbActiveUser',
			);
		});
	}

	/**
	 * Check if the current user is the same as the user being edited
	 * @protected
	 * @return {Promise<boolean>}
	 * @memberof UmbUserActionConditionBase
	 */
	protected async isCurrentUser() {
		return this.userUnique ? isCurrentUser(this._host, this.userUnique) : false;
	}

	/**
	 * Called when the user data changes
	 * @protected
	 * @memberof UmbUserActionConditionBase
	 */
	protected abstract _onUserDataChange(): Promise<void>;
}
