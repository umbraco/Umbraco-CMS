import { UmbUserDetail } from '../types.js';
import { UmbUserWorkspaceContext } from '../workspace/user-workspace.context.js';
import { UserStateModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/controller-api';
import { isCurrentUser } from '@umbraco-cms/backoffice/current-user';
import {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

export class UmbUserAllowResendInviteActionCondition extends UmbBaseController implements UmbExtensionCondition {
	config: UmbConditionConfigBase;
	permitted = false;
	#onChange: () => void;

	constructor(args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
			const userContext = context as UmbUserWorkspaceContext;
			this.observe(userContext.data, (data) => this.onUserDataChange(data));
		});
	}

	async onUserDataChange(user: UmbUserDetail | undefined) {
		if (!user || !user.id) {
			this.permitted = false;
			this.#onChange();
			return;
		}

		this.permitted = user?.state === UserStateModel.INVITED;
		this.#onChange();
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'User Allow Resend Invite Action Condition',
	alias: 'Umb.Condition.User.AllowResendInviteAction',
	api: UmbUserAllowResendInviteActionCondition,
};
