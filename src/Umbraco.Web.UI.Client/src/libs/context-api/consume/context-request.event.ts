export const UMB_CONTENT_REQUEST_EVENT_TYPE = 'umb:context-request';
export const UMB_DEBUG_CONTEXT_EVENT_TYPE = 'umb:debug-contexts';

export type UmbContextCallback<T> = (instance: T) => void;

/**
 * @export
 * @interface UmbContextRequestEvent
 */
export interface UmbContextRequestEvent<ResultType = unknown> extends Event {
	readonly contextAlias: string;
	readonly apiAlias: string;
	readonly callback: (context: ResultType) => boolean;
}

/**
 * @export
 * @class UmbContextRequestEventImplementation
 * @extends {Event}
 * @implements {UmbContextRequestEvent}
 */
export class UmbContextRequestEventImplementation<ResultType = unknown>
	extends Event
	implements UmbContextRequestEvent<ResultType>
{
	public constructor(
		public readonly contextAlias: string,
		public readonly apiAlias: string,
		public readonly callback: (context: ResultType) => boolean,
	) {
		super(UMB_CONTENT_REQUEST_EVENT_TYPE, { bubbles: true, composed: true, cancelable: true });
	}
}

export class UmbContextDebugRequest extends Event {
	public constructor(public readonly callback: any) {
		super(UMB_DEBUG_CONTEXT_EVENT_TYPE, { bubbles: true, composed: true, cancelable: false });
	}
}
