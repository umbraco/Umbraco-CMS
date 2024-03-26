import { UMB_CURRENT_USER_CONTEXT } from '../../current-user/current-user.context.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbUserPermissionCondition
	extends UmbConditionBase<UserPermissionConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UserPermissionConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context.currentUser,
				(currentUser) => {
					//this.permitted = currentUser?.permissions?.includes(this.config.match) || false;
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
