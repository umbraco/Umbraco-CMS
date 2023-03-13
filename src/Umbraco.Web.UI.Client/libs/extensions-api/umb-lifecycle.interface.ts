import type { UmbExtensionRegistry } from "./registry/extension.registry";
import type { UmbControllerHostInterface } from "@umbraco-cms/controller";

/**
 * Interface containing supported life-cycle functions for ESModule entrypoints
 */
export interface UmbEntrypointModule {
	onInit: (host: UmbControllerHostInterface, extensionRegistry: UmbExtensionRegistry) => void
}
