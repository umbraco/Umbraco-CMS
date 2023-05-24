import type { UmbExtensionRegistry } from './registry/extension.registry.js';
import { ManifestBase } from './types.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export type UmbEntryPointOnInit = (
	host: UmbControllerHostElement,
	extensionRegistry: UmbExtensionRegistry<ManifestBase>
) => void;

/**
 * Interface containing supported life-cycle functions for ESModule entry points
 */
export interface UmbEntryPointModule {
	onInit: UmbEntryPointOnInit;
}
