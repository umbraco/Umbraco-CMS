import type { UmbUserStateEnum } from '../types.js';
import { UMB_USER_WORKSPACE_CONTEXT } from '../workspace/user-workspace.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { isCurrentUser, isCurrentUserAnAdmin } from '@umbraco-cms/backoffice/current-user';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export abstract class UmbUserActionConditionBase
	extends UmbConditionBase<UmbConditionConfigBase>
	implements UmbExtensionCondition
{
	protected userUnique?: string;
	protected userState?: UmbUserStateEnum | null;

	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);

		this.consumeContext(UMB_USER_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context.unique,
				(unique) => {
					this.userUnique = unique;
					this._onUserDataChange();
				},
				'umbUserUnique',
			);
			this.observe(
				context.state,
				(state) => {
					this.userState = state;
					// TODO: Investigate if we can remove this observation and just use the unique change to trigger the state change. [NL]
					// Can user state change over time? if not then this observation is not needed and then we just need to retrieve the state when the unique has changed. [NL]
					// These two could also be combined via the observeMultiple method, that could prevent triggering onUserDataChanged twice. [NL]
					this._onUserDataChange();
				},
				'umbUserState',
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
	 * Check if the current user is an admin
	 * @protected
	 */
	protected async isCurrentUserAdmin() {
		return isCurrentUserAnAdmin(this._host);
	}

	/**
	 * Called when the user data changes
	 * @protected
	 * @memberof UmbUserActionConditionBase
	 */
	protected abstract _onUserDataChange(): Promise<void>;
}
