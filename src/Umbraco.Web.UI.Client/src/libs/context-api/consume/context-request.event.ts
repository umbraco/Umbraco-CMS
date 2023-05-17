import { UmbContextToken } from '../token/context-token';

export const umbContextRequestEventType = 'umb:context-request';
export const umbDebugContextEventType = 'umb:debug-contexts';

export type UmbContextCallback<T> = (instance: T) => void;

/**
 * @export
 * @interface UmbContextRequestEvent
 */
export interface UmbContextRequestEvent<T = unknown> extends Event {
	readonly contextAlias: string | UmbContextToken<T>;
	readonly callback: UmbContextCallback<T>;
}

/**
 * @export
 * @class UmbContextRequestEventImplementation
 * @extends {Event}
 * @implements {UmbContextRequestEvent}
 */
export class UmbContextRequestEventImplementation<T = unknown> extends Event implements UmbContextRequestEvent<T> {
	public constructor(
		public readonly contextAlias: string | UmbContextToken<T>,
		public readonly callback: UmbContextCallback<T>
	) {
		super(umbContextRequestEventType, { bubbles: true, composed: true, cancelable: true });
	}
}

export const isUmbContextRequestEvent = (event: Event): event is UmbContextRequestEventImplementation => {
	return event.type === umbContextRequestEventType;
};

export class UmbContextDebugRequest extends Event {
	public constructor(public readonly callback: any) {
		super(umbDebugContextEventType, { bubbles: true, composed: true, cancelable: false });
	}
}
