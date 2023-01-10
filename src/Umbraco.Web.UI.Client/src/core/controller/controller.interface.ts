export interface UmbControllerInterface {
	hostConnected(): void;
	hostDisconnected(): void;
	destroy(): void;
}
