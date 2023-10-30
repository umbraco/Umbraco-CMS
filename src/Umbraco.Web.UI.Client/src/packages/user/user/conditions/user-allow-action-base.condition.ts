import { UmbUserDetail } from '../types.js';
import { UmbUserWorkspaceContext } from '../workspace/user-workspace.context.js';
import { UmbBaseController } from '@umbraco-cms/backoffice/controller-api';
import { isCurrentUser } from '@umbraco-cms/backoffice/current-user';
import {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

export class UmbUserActionConditionBase extends UmbBaseController implements UmbExtensionCondition {
	config: UmbConditionConfigBase;
	permitted = false;
	#onChange: () => void;
	protected userData?: UmbUserDetail;

	constructor(args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
			const userContext = context as UmbUserWorkspaceContext;
			this.observe(userContext.data, (data) => {
				this.userData = data;
				this.onUserDataChange();
			}, 'umbUserDataActionConditionObserver');
		});
	}

	/**
	 * Check if the current user is the same as the user being edited
	 * @protected
	 * @return {Promise<boolean>}
	 * @memberof UmbUserActionConditionBase
	 */
	protected async isCurrentUser() {
		return this.userData?.id ? isCurrentUser(this._host, this.userData.id) : false;
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
