import { ContextToken } from '@umbraco-cms/context-api';

export interface UmbControllerInterface<T> {
	get unique(): string | ContextToken<T>;
	hostConnected(): void;
	hostDisconnected(): void;
	destroy(): void;
}
