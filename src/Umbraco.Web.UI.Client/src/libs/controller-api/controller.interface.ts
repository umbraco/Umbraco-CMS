export interface UmbController {
	get unique(): string | undefined;
	hostConnected(): void;
	hostDisconnected(): void;
	destroy(): void;
}
