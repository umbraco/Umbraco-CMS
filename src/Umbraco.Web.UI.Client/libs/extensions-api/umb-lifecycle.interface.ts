import type { UmbExtensionRegistry } from './registry/extension.registry';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export type UmbEntrypointOnInit = (host: UmbControllerHostElement, extensionRegistry: UmbExtensionRegistry) => void;

/**
 * Interface containing supported life-cycle functions for ESModule entrypoints
 */
export interface UmbEntrypointModule {
	onInit: UmbEntrypointOnInit;
}
