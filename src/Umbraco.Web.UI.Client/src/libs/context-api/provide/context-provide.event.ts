import type { UmbContextToken } from '../token/context-token.js';

export const UMB_CONTEXT_PROVIDE_EVENT_TYPE = 'umb:context-provide';

/**
 * @interface UmbContextProvideEvent
 */
export interface UmbContextProvideEvent extends Event {
	readonly contextAlias: string | UmbContextToken;
}

/**
 * @class UmbContextProvideEventImplementation
 * @augments {Event}
 * @implements {UmbContextProvideEvent}
 */
export class UmbContextProvideEventImplementation extends Event implements UmbContextProvideEvent {
	public constructor(public readonly contextAlias: string | UmbContextToken) {
		super(UMB_CONTEXT_PROVIDE_EVENT_TYPE, { bubbles: true, composed: true });
	}
}

export const isUmbContextProvideEventType = (event: Event): event is UmbContextProvideEventImplementation => {
	return event.type === UMB_CONTEXT_PROVIDE_EVENT_TYPE;
};

export const UMB_CONTEXT_UNPROVIDED_EVENT_TYPE = 'umb:context-unprovided';

/**
 * @interface UmbContextProvideEvent
 */
export interface UmbContextUnprovidedEvent extends Event {
	readonly contextAlias: string | UmbContextToken;
	readonly instance: unknown;
}

/**
 * @class UmbContextUnprovidedEventImplementation
 * @augments {Event}
 * @implements {UmbContextUnprovidedEvent}
 */
export class UmbContextUnprovidedEventImplementation extends Event implements UmbContextUnprovidedEvent {
	public constructor(
		public readonly contextAlias: string | UmbContextToken,
		public readonly instance: unknown,
	) {
		super(UMB_CONTEXT_UNPROVIDED_EVENT_TYPE, { bubbles: true, composed: true });
	}
}

export const isUmbContextUnprovidedEventType = (event: Event): event is UmbContextUnprovidedEventImplementation => {
	return event.type === UMB_CONTEXT_UNPROVIDED_EVENT_TYPE;
};
