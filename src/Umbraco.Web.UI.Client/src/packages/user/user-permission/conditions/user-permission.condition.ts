import { UMB_CURRENT_USER_CONTEXT } from '../../current-user/current-user.context.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbUserPermissionCondition extends UmbControllerBase implements UmbExtensionCondition {
	config: UserPermissionConditionConfig;
	permitted = false;
	#onChange: () => void;

	constructor(args: UmbConditionControllerArguments<UserPermissionConditionConfig>) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context.currentUser,
				(currentUser) => {
					this.permitted = currentUser?.permissions?.includes(this.config.match) || false;
					this.#onChange();
				},
				'umbUserPermissionConditionObserver',
			);
		});
	}
}

export type UserPermissionConditionConfig = UmbConditionConfigBase<'Umb.Condition.UserPermission'> & {
	/**
	 *
	 *
	 * @example
	 * "Umb.UserPermission.Document.Create"
	 */
	match: string;
};

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'User Permission Condition',
	alias: 'Umb.Condition.UserPermission',
	api: UmbUserPermissionCondition,
};
