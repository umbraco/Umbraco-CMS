import type { UmbExtensionRegistry } from '../registry/extension.registry.js';
import type { ManifestBase } from '../types/index.js';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';

export type UmbEntryPointOnInit = (host: UmbElement, extensionRegistry: UmbExtensionRegistry<ManifestBase>) => void;

export type UmbEntryPointOnUnload = (host: UmbElement, extensionRegistry: UmbExtensionRegistry<ManifestBase>) => void;

/**
 * Interface containing supported life-cycle functions for ESModule entry points
 */
export interface UmbEntryPointModule {
	/**
	 * Function that will be called when the host element is initialized and/or the extension is loaded for the first time.
	 * @optional
	 */
	onInit: UmbEntryPointOnInit;

	/**
	 * Function that will be called when the extension is unregistered.
	 * @remark This does not mean the host element is destroyed, only that the extension is no longer available. You should listen to the host element's `destroy` event if you need to clean up after the host element.
	 * @optional
	 */
	onUnload: UmbEntryPointOnUnload;
}
