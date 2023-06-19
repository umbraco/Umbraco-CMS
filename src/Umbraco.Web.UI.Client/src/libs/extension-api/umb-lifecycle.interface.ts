import type { UmbExtensionRegistry } from './registry/extension.registry.js';
import { ManifestBase } from './types.js';
import type { UmbElementMixinInterface } from '@umbraco-cms/backoffice/element-api';

export type UmbEntryPointOnInit = (
	host: UmbElementMixinInterface,
	extensionRegistry: UmbExtensionRegistry<ManifestBase>
) => void;

/**
 * Interface containing supported life-cycle functions for ESModule entry points
 */
export interface UmbEntryPointModule {
	onInit: UmbEntryPointOnInit;
}
