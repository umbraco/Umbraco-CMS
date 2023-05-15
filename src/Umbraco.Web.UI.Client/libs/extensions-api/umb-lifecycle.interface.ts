import type { UmbExtensionRegistry } from './registry/extension.registry';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export type UmbEntryPointOnInit = (host: UmbControllerHostElement, extensionRegistry: UmbExtensionRegistry) => void;

/**
 * Interface containing supported life-cycle functions for ESModule entry points
 */
export interface UmbEntryPointModule {
	onInit: UmbEntryPointOnInit;
}
