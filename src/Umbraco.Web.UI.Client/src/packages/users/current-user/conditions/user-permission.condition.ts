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

		console.log('HELLO FROM MY CONDITION');
		console.log('GET CURRENT USER CONTEXT');
		this.permitted = true;

		/*
		this.consumeContext(UMB_SECTION_CONTEXT_TOKEN, (context) => {
			this.observe(context.alias, (sectionAlias) => {
				this.permitted = sectionAlias === this.config.match;
				this.#onChange();
			});
		});
    */
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
