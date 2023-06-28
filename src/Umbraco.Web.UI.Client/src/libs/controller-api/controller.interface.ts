export interface UmbController {
	get controllerAlias(): string | undefined;
	hostConnected(): void;
	hostDisconnected(): void;
	destroy(): void;
}
