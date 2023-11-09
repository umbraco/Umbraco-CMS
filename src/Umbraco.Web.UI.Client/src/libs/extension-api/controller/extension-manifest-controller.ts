import type { ManifestCondition, ManifestWithDynamicConditions } from '../types.js';
import type { UmbExtensionRegistry } from '../registry/extension.registry.js';
import { UmbBaseExtensionController } from './base-extension-controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * This Controller manages a single Extension and its Manifest.
 * When the extension is permitted to be used, the manifest is available for the consumer.
 *
 * @example
* ```ts
* const controller = new UmbExtensionManifestController(host, extensionRegistry, alias, (permitted, ctrl) => { console.log("Extension is permitted and this is the manifest: ", ctrl.manifest) }));
* ```
 * @export
 * @class UmbExtensionManifestController
 */
export class UmbExtensionManifestController<
	ManifestType extends ManifestWithDynamicConditions = ManifestWithDynamicConditions,
	ControllerType extends UmbBaseExtensionController<ManifestType, any> = any
> extends UmbBaseExtensionController<ManifestType, ControllerType> {
	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbExtensionRegistry<ManifestCondition>,
		alias: string,
		onPermissionChanged: (isPermitted: boolean, controller: ControllerType) => void
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
