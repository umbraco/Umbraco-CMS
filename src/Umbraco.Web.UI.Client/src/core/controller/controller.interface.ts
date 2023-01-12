export interface UmbControllerInterface {
	get unique(): string | undefined;
	hostConnected(): void;
	hostDisconnected(): void;
	destroy(): void;
}
