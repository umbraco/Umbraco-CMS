import type { UmbControllerHostInterface } from "@umbraco-cms/controller";
import type { UmbExtensionRegistry } from "./registry/extension.registry";

/**
 * Interface containing supported life-cycle functions for ESModule entrypoints
 */
export interface UmbEntrypointModule {
	onInit: (host: UmbControllerHostInterface, extensionRegistry: UmbExtensionRegistry) => void
}
