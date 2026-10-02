import { UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_CONDITION_ALIAS } from '../conditions/constants.js';
import {
	UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_READ,
	UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_WRITE,
} from '../constants.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbVariantPropertyGuardManager } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';

export class UmbPropertyValueUserPermissionWorkspaceContextBase extends UmbControllerBase {
	#conditions = new Map<UmbVariantPropertyGuardManager, Array<Promise<UmbApi>>>();

	protected _setPermissions(
		properties: Array<UmbPropertyTypeModel>,
		propertyViewGuard: UmbVariantPropertyGuardManager,
		propertyWriteGuard: UmbVariantPropertyGuardManager,
	) {
		this.#destroyConditions(propertyViewGuard);
		this.#destroyConditions(propertyWriteGuard);

		properties.forEach((property) => {
			this.#setPermissionForProperty({
				verb: UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_READ,
				stateManager: propertyViewGuard,
				property,
			});

			this.#setPermissionForProperty({
				verb: UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_WRITE,
				stateManager: propertyWriteGuard,
				property,
			});
		});
	}

	#setPermissionForProperty(args: {
		verb: string;
		stateManager: UmbVariantPropertyGuardManager;
		property: UmbPropertyTypeModel;
	}) {
		// TODO: Oh, this results in quite a few Context Consumptions. Lets try not to use a condition in this case. [NL]
		const condition = createExtensionApiByAlias(this, UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_CONDITION_ALIAS, [
			{
				config: {
					allOf: [args.verb],
					match: {
						propertyType: {
							unique: args.property.unique,
						},
					},
				},
				onChange: (permitted: boolean) => {
					const unique = 'UMB_PROPERTY_' + args.property.unique;

					if (permitted) {
						args.stateManager.addRule({
							unique,
							propertyType: {
								unique: args.property.unique,
							},
						});
					} else {
						args.stateManager.removeRule(unique);
					}
				},
			},
		]);

		const conditions = this.#conditions.get(args.stateManager) ?? [];
		conditions.push(condition);
		this.#conditions.set(args.stateManager, conditions);
	}

	#destroyConditions(stateManager: UmbVariantPropertyGuardManager) {
		const conditions = this.#conditions.get(stateManager);
		if (!conditions) return;
		this.#conditions.delete(stateManager);
		conditions.forEach((condition) => condition.then((api) => api.destroy()).catch(() => undefined));
	}

	override destroy(): void {
		super.destroy();
		// Conditions should have been destroyed at this point as we share the same host.
		this.#conditions.clear();
	}
}

export { UmbPropertyValueUserPermissionWorkspaceContextBase as api };
