import type { UmbElementOrElementFolderUserPermissionConditionConfig } from './types.js';
import { UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS } from './constants.js';
import { UmbElementUserPermissionCondition } from './element-user-permission.condition.js';
import { UmbElementFolderUserPermissionCondition } from '../../folder/user-permissions/conditions/element-folder-user-permission.condition.js';
import { UMB_ELEMENT_FOLDER_USER_PERMISSION_CONDITION_ALIAS } from '../../folder/user-permissions/conditions/constants.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/** Permits when the user has the configured element OR element folder (container) permissions. */
export class UmbElementOrElementFolderUserPermissionCondition
	extends UmbConditionBase<UmbElementOrElementFolderUserPermissionConditionConfig>
	implements UmbExtensionCondition
{
	#elementCondition: UmbElementUserPermissionCondition | undefined;
	#folderCondition: UmbElementFolderUserPermissionCondition | undefined;

	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbElementOrElementFolderUserPermissionConditionConfig>,
	) {
		super(host, args);

		if (this.config.element) {
			this.#elementCondition = new UmbElementUserPermissionCondition(this, {
				host: this,
				config: { alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS, ...this.config.element },
				onChange: () => this.#evaluate(),
			});
		}

		if (this.config.folder) {
			this.#folderCondition = new UmbElementFolderUserPermissionCondition(this, {
				host: this,
				config: { alias: UMB_ELEMENT_FOLDER_USER_PERMISSION_CONDITION_ALIAS, ...this.config.folder },
				onChange: () => this.#evaluate(),
			});
		}
	}

	#evaluate() {
		this.permitted = (this.#elementCondition?.permitted ?? false) || (this.#folderCondition?.permitted ?? false);
	}
}

export { UmbElementOrElementFolderUserPermissionCondition as api };
