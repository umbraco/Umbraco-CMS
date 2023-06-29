import type { UmbExtensionRegistry } from './registry/extension.registry.js';
import { ManifestBase } from './types.js';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';

export type UmbEntryPointOnInit = (host: UmbElement, extensionRegistry: UmbExtensionRegistry<ManifestBase>) => void;

/**
 * Interface containing supported life-cycle functions for ESModule entry points
 */
export interface UmbEntryPointModule {
	onInit: UmbEntryPointOnInit;
}
