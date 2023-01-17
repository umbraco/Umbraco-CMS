import { UmbContextAlias } from '../context-alias';

export const umbContextRequestEventType = 'umb:context-request';

export type UmbContextCallback<T> = (instance: T) => void;

/**
 * @export
 * @interface UmbContextRequestEvent
 */
export interface UmbContextRequestEvent<T = unknown> extends Event {
	readonly contextAlias: string | UmbContextAlias<T>;
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
		public readonly contextAlias: string | UmbContextAlias<T>,
		public readonly callback: UmbContextCallback<T>
	) {
		super(umbContextRequestEventType, { bubbles: true, composed: true, cancelable: true });
	}
}

export const isUmbContextRequestEvent = (event: Event): event is UmbContextRequestEventImplementation => {
	return event.type === umbContextRequestEventType;
};
