import type { UmbControllerAlias } from './controller-alias.type.js';
export interface UmbController {
	get controllerAlias(): UmbControllerAlias;
	hostConnected(): void;
	hostDisconnected(): void;
	destroy(): void;
}
