import { ContextToken } from '../context-token';

export const umbContextRequestEventType = 'umb:context-request';

export type UmbContextCallback<T = unknown> = (instance: T) => void;

/**
 * @export
 * @interface UmbContextRequestEvent
 */
export interface UmbContextRequestEvent extends Event {
	readonly contextAlias: string | ContextToken;
	readonly callback: UmbContextCallback;
}

/**
 * @export
 * @class UmbContextRequestEventImplementation
 * @extends {Event}
 * @implements {UmbContextRequestEvent}
 */
export class UmbContextRequestEventImplementation extends Event implements UmbContextRequestEvent {
	public constructor(
		public readonly contextAlias: string | ContextToken,
		public readonly callback: UmbContextCallback
	) {
		super(umbContextRequestEventType, { bubbles: true, composed: true, cancelable: true });
	}
}

export const isUmbContextRequestEvent = (event: Event): event is UmbContextRequestEventImplementation => {
	return event.type === umbContextRequestEventType;
};
