export interface UmbControllerInterface {
	get unique(): string;
	hostConnected(): void;
	hostDisconnected(): void;
	destroy(): void;
}
