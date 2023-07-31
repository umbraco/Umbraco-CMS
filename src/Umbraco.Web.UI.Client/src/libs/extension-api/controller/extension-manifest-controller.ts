import type { ManifestCondition, ManifestWithDynamicConditions } from '../types.js';
import type { UmbExtensionRegistry } from '../registry/extension.registry.js';
import { UmbBaseExtensionController } from './base-extension-controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbExtensionManifestController<
	ManifestType extends ManifestWithDynamicConditions = ManifestWithDynamicConditions
> extends UmbBaseExtensionController<ManifestType> {
	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestCondition>,
		alias: string,
		onPermissionChanged: (isPermitted: boolean, controller: UmbExtensionManifestController<ManifestType>) => void
	) {
		super(host, extensionRegistry, alias, onPermissionChanged);
		this._init();
	}

	protected async _conditionsAreGood() {
		return true;
	}

	protected async _conditionsAreBad() {
		// Destroy the element/class.
	}
}
