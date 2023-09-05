import { UmbContextToken } from '../token/context-token.js';

export const umbContextRequestEventType = 'umb:context-request';
export const umbDebugContextEventType = 'umb:debug-contexts';

export type UmbContextCallback<T> = (instance: T) => void;

/**
 * @export
 * @interface UmbContextRequestEvent
 */
export interface UmbContextRequestEvent<ResultType = unknown> extends Event {
	readonly contextAlias: string | UmbContextToken<unknown, ResultType>;
	readonly callback: UmbContextCallback<ResultType>;
}

/**
 * @export
 * @class UmbContextRequestEventImplementation
 * @extends {Event}
 * @implements {UmbContextRequestEvent}
 */
export class UmbContextRequestEventImplementation<ResultType = unknown> extends Event implements UmbContextRequestEvent<ResultType> {
	public constructor(
		public readonly contextAlias: string | UmbContextToken<any, ResultType>,
		public readonly callback: UmbContextCallback<ResultType>
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
