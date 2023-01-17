import { ContextToken } from '../context-token';

export const umbContextProvideEventType = 'umb:context-provide';

/**
 * @export
 * @interface UmbContextProvideEvent
 */
export interface UmbContextProvideEvent extends Event {
	readonly contextAlias: string | ContextToken;
}

/**
 * @export
 * @class UmbContextProvideEventImplementation
 * @extends {Event}
 * @implements {UmbContextProvideEvent}
 */
export class UmbContextProvideEventImplementation extends Event implements UmbContextProvideEvent {
	public constructor(public readonly contextAlias: string | ContextToken) {
		super(umbContextProvideEventType, { bubbles: true, composed: true });
	}
}

export const isUmbContextProvideEventType = (event: Event): event is UmbContextProvideEventImplementation => {
	return event.type === umbContextProvideEventType;
};
