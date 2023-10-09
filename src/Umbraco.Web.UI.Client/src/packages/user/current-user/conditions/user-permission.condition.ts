import { UMB_AUTH } from '@umbraco-cms/backoffice/auth';
import { UmbBaseController } from '@umbraco-cms/backoffice/controller-api';
import {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbUserPermissionCondition extends UmbBaseController implements UmbExtensionCondition {
	config: UserPermissionConditionConfig;
	permitted = false;
	#onChange: () => void;

	constructor(args: UmbConditionControllerArguments<UserPermissionConditionConfig>) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;

		this.consumeContext(UMB_AUTH, (context) => {
			this.observe(context.currentUser, (currentUser) => {
				this.permitted = currentUser?.permissions?.includes(this.config.match) || false;
				this.#onChange();
			});
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
	class: UmbUserPermissionCondition,
};
