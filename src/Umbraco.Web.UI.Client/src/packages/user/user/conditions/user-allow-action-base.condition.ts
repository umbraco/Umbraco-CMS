import type { UmbUserStateEnum } from '../types.js';
import { UMB_USER_WORKSPACE_CONTEXT } from '../workspace/user-workspace.context.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { isCurrentUser } from '@umbraco-cms/backoffice/current-user';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbUserActionConditionBase extends UmbControllerBase implements UmbExtensionCondition {
	config: UmbConditionConfigBase;
	permitted = false;
	#onChange: () => void;
	protected userUnique?: string;
	protected userState?: UmbUserStateEnum | null;

	constructor(args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;

		this.consumeContext(UMB_USER_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context.unique,
				(unique) => {
					this.userUnique = unique;
					this.onUserDataChange();
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
					this.onUserDataChange();
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
	 * Called when the user data changes
	 * @protected
	 * @memberof UmbUserActionConditionBase
	 */
	protected async onUserDataChange() {
		this.#onChange();
	}
}
