import type { UmbExtensionRegistry } from './registry/extension.registry';
import { ManifestBase } from './types';
import type { UmbControllerHostElement } from 'src/libs/controller-api';

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
