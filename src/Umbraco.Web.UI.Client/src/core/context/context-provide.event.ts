export const umbContextProvideEventType = 'umb:context-provide';

/**
 * @export
 * @interface UmbContextProvideEvent
 */
export interface UmbContextProvideEvent extends Event {
  readonly contextAlias: string;
}

/**
 * @export
 * @class UmbContextProvideEventImplementation
 * @extends {Event}
 * @implements {UmbContextProvideEvent}
 */
export class UmbContextProvideEventImplementation extends Event implements UmbContextProvideEvent {
  public constructor(public readonly contextAlias: string) {
    super(umbContextProvideEventType, { bubbles: true, composed: true });
  }
}

export const isUmbContextProvideEvent = (event: Event): event is UmbContextProvideEventImplementation => {
  return event.type === umbContextProvideEventType;
};
