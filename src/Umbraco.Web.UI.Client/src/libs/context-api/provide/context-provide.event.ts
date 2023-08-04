import { UmbContextToken } from '../token/context-token.js';

export const umbContextProvideEventType = 'umb:context-provide';

/**
 * @export
 * @interface UmbContextProvideEvent
 */
export interface UmbContextProvideEvent extends Event {
	readonly contextAlias: string | UmbContextToken;
}

/**
 * @export
 * @class UmbContextProvideEventImplementation
 * @extends {Event}
 * @implements {UmbContextProvideEvent}
 */
export class UmbContextProvideEventImplementation extends Event implements UmbContextProvideEvent {
	public constructor(public readonly contextAlias: string | UmbContextToken) {
		super(umbContextProvideEventType, { bubbles: true, composed: true });
	}
}

export const isUmbContextProvideEventType = (event: Event): event is UmbContextProvideEventImplementation => {
	return event.type === umbContextProvideEventType;
};

export const umbContextUnprovidedEventType = 'umb:context-unprovided';

/**
 * @export
 * @interface UmbContextProvideEvent
 */
export interface UmbContextUnprovidedEvent extends Event {
	readonly contextAlias: string | UmbContextToken;
	readonly instance: unknown;
}

/**
 * @export
 * @class UmbContextUnprovidedEventImplementation
 * @extends {Event}
 * @implements {UmbContextUnprovidedEvent}
 */
export class UmbContextUnprovidedEventImplementation extends Event implements UmbContextUnprovidedEvent {
	public constructor(public readonly contextAlias: string | UmbContextToken, public readonly instance: unknown) {
		super(umbContextUnprovidedEventType, { bubbles: true, composed: true });
	}
}

export const isUmbContextUnprovidedEventType = (event: Event): event is UmbContextUnprovidedEventImplementation => {
	return event.type === umbContextUnprovidedEventType;
};
