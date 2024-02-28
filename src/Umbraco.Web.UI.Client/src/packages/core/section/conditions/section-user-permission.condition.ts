import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbSectionUserPermissionCondition extends UmbBaseController implements UmbExtensionCondition {
	config: UmbSectionUserPermissionConditionConfig;
	permitted = false;
	#onChange: () => void;

	constructor(args: UmbConditionControllerArguments<UmbSectionUserPermissionConditionConfig>) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context.currentUser,
				(currentUser) => {
					const sectionAccess = currentUser?.allowedSections || [];
					this.permitted = sectionAccess.includes(this.config.match);
					this.#onChange();
				},
				'umbSectionUserPermissionConditionObserver',
			);
		});
	}
}

export type UmbSectionUserPermissionConditionConfig = UmbConditionConfigBase<'Umb.Condition.SectionUserPermission'> & {
	/**
	 *
	 *
	 * @example
	 * "Umb.Section.Content"
	 */
	match: string;
};
