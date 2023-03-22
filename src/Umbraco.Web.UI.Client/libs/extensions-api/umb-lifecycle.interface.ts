import type { UmbExtensionRegistry } from './registry/extension.registry';
import type { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';

export type UmbEntrypointOnInit = (host: UmbControllerHostInterface, extensionRegistry: UmbExtensionRegistry) => void;

/**
 * Interface containing supported life-cycle functions for ESModule entrypoints
 */
export interface UmbEntrypointModule {
	onInit: UmbEntrypointOnInit;
}
