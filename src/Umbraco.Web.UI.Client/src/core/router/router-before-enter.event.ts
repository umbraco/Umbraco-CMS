import { UmbRouteLocation } from './router';

export const umbRouterBeforeEnterEventType = 'umb:router-before-enter';

/**
 * @export
 * @interface UmbRouterBeforeEnter
 */
 export interface UmbRouterBeforeEnter {
  readonly to: UmbRouteLocation;
}

/**
 * @export
 * @class UmbRouterBeforeEnterEvent
 * @extends {Event}
 * @implements {UmbRouterBeforeEnter}
 */
export class UmbRouterBeforeEnterEvent extends Event implements UmbRouterBeforeEnter {
  public constructor (
    public readonly to: UmbRouteLocation,
  ) {
    super(umbRouterBeforeEnterEventType, {bubbles: true, composed: true, cancelable: true });
  }
}

export const isUmbRouterBeforeEnterEvent = (event: Event): event is UmbRouterBeforeEnterEvent => {
  return event.type === umbRouterBeforeEnterEventType;
}