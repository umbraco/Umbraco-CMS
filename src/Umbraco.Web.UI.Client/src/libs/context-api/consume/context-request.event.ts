export const UMB_CONTEXT_REQUEST_EVENT_TYPE = 'umb:context-request';
/**
 * @deprecated use UMB_CONTEXT_REQUEST_EVENT_TYPE
 * This will be removed in Umbraco 17
 */
export const UMB_CONTENT_REQUEST_EVENT_TYPE = UMB_CONTEXT_REQUEST_EVENT_TYPE;
export const UMB_DEBUG_CONTEXT_EVENT_TYPE = 'umb:debug-contexts';

export type UmbContextCallback<T> = (instance: T) => void;

/**
 * @interface UmbContextRequestEvent
 */
export interface UmbContextRequestEvent<ResultType = unknown> extends Event {
	readonly contextAlias: string;
	readonly apiAlias: string;
	readonly callback: (context: ResultType) => boolean;
	readonly stopAtContextMatch: boolean;
	clone(): UmbContextRequestEvent<ResultType>;
}

/**
 * @class UmbContextRequestEventImplementation
 * @augments {Event}
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
		public readonly stopAtContextMatch: boolean = true,
	) {
		super(UMB_CONTEXT_REQUEST_EVENT_TYPE, { bubbles: true, composed: true, cancelable: true });
	}

	clone() {
		return new UmbContextRequestEventImplementation(
			this.contextAlias,
			this.apiAlias,
			this.callback,
			this.stopAtContextMatch,
		);
	}
}

export class UmbContextDebugRequest extends Event {
	public constructor(public readonly callback: any) {
		super(UMB_DEBUG_CONTEXT_EVENT_TYPE, { bubbles: true, composed: true, cancelable: false });
	}
}
