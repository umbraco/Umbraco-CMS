import type { UmbExtensionRegistry } from '../registry/extension.registry.js';
import type { ManifestBase } from '../types/index.js';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';

export type UmbEntryPointOnInit = (host: UmbElement, extensionRegistry: UmbExtensionRegistry<ManifestBase>) => void;

/**
 * Interface containing supported life-cycle functions for ESModule entry points
 */
export interface UmbEntryPointModule {
	/**
	 * Function that will be called when the backoffice element is initialized after authentication.
	 */
	onInit: UmbEntryPointOnInit;
}
