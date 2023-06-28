export interface UmbController {
	get controllerAlias(): string | symbol | undefined;
	hostConnected(): void;
	hostDisconnected(): void;
	destroy(): void;
}
