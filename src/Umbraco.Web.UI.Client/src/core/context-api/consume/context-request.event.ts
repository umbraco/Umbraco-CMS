import { ContextToken } from '../injectionToken';

export const umbContextRequestEventType = 'umb:context-request';

export type UmbContextCallback<T> = (instance: T) => void;

/**
 * @export
 * @interface UmbContextRequestEvent
 */
export interface UmbContextRequestEvent<T> extends Event {
	readonly contextAlias: string | ContextToken<T>;
	readonly callback: UmbContextCallback<T>;
}

/**
 * @export
 * @class UmbContextRequestEventImplementation
 * @extends {Event}
 * @implements {UmbContextRequestEvent}
 */
export class UmbContextRequestEventImplementation<T> extends Event implements UmbContextRequestEvent<T> {
	public constructor(
		public readonly contextAlias: string | ContextToken<T>,
		public readonly callback: UmbContextCallback<T>
	) {
		super(umbContextRequestEventType, { bubbles: true, composed: true, cancelable: true });
	}
}

export const isUmbContextRequestEvent = (event: Event): event is UmbContextRequestEventImplementation<Event> => {
	return event.type === umbContextRequestEventType;
};
