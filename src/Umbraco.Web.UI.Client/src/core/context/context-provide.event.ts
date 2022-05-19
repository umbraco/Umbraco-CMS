export const umbContextProvideType = 'umb:context-provide';

/**
 * @export
 * @interface UmbContextProvide
 */
export interface UmbContextProvide {
  readonly contextKey: string;
}

/**
 * @export
 * @class UmbContextProvideEvent
 * @extends {Event}
 * @implements {UmbContextProvide}
 */
export class UmbContextProvideEvent extends Event implements UmbContextProvide {
  public constructor(
    public readonly contextKey: string,
  ) {
    super(umbContextProvideType, {bubbles: true, composed: true });
  }
}

export const isUmbContextProvideEvent = (event: Event): event is UmbContextProvideEvent => {
  return event.type === umbContextProvideType;
}