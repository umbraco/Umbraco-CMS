import { umbPermissionRegistry } from '../registry/permission.registry.js';
import type { UmbPermissionConditionConfig } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

/**
 * Permits the extension while the permission registry permits the configured verbs.
 */
export class UmbPermissionCondition
	extends UmbConditionBase<UmbPermissionConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbPermissionConditionConfig>) {
		super(host, args);

		this.observe(
			umbPermissionRegistry.permissionResult(this, this.config),
			(result) => {
				this.permitted = result.permitted === true;
			},
			null,
		);
	}
}

export { UmbPermissionCondition as api };
