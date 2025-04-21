import type { UmbSectionUserPermissionConditionConfig } from './types.js';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

// Do not export - for internal use only
type UmbOnChangeCallbackType = (permitted: boolean) => void;

export class UmbSectionUserPermissionCondition extends UmbControllerBase implements UmbExtensionCondition {
	config: UmbSectionUserPermissionConditionConfig;
	permitted = false;
	#onChange: UmbOnChangeCallbackType;

	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbSectionUserPermissionConditionConfig, UmbOnChangeCallbackType>,
	) {
		super(host);
		this.config = args.config;
		this.#onChange = args.onChange;

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context?.currentUser,
				(currentUser) => {
					const allowedSections = currentUser?.allowedSections || [];
					this.permitted = allowedSections.includes(this.config.match);
					this.#onChange(this.permitted);
				},
				'umbSectionUserPermissionConditionObserver',
			);
		});
	}
}
