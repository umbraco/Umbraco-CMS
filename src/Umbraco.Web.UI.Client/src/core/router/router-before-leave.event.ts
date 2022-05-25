import { UmbRouteLocation } from './router';

export const umbRouterBeforeLeaveEventType = 'umb:router-before-leave';

/**
 * @export
 * @interface UmbRouterBeforeLeave
 */
 export interface UmbRouterBeforeLeave {
  readonly to: UmbRouteLocation;
}

/**
 * @export
 * @class UmbRouterBeforeLeaveEvent
 * @extends {Event}
 * @implements {UmbRouterBeforeLeave}
 */
export class UmbRouterBeforeLeaveEvent extends Event implements UmbRouterBeforeLeave {
  public constructor (
    public readonly to: UmbRouteLocation,
  ) {
    super(umbRouterBeforeLeaveEventType, {bubbles: true, composed: true, cancelable: true });
  }
}

export const isUmbRouterBeforeLeaveEvent = (event: Event): event is UmbRouterBeforeLeaveEvent => {
  return event.type === umbRouterBeforeLeaveEventType;
}