import { GLOBAL_ROUTER_EVENTS_TARGET } from '../config.js';
import type { EventListenerSubscription, GlobalRouterEvent, IRoutingInfo } from '../model.js';

/**
 * Dispatches a did change route event.
 * @param $elem
 * @param {IRoute} detail
 */
export function dispatchRouteChangeEvent<D = any>($elem: HTMLElement, detail: IRoutingInfo<D>) {
	$elem.dispatchEvent(new CustomEvent('changestate', { detail }));
}

/**
 * Dispatches an event on the window object.
 * @param name
 * @param detail
 */
export function dispatchGlobalRouterEvent<D = any>(name: GlobalRouterEvent, detail?: IRoutingInfo<D>) {
	GLOBAL_ROUTER_EVENTS_TARGET.dispatchEvent(new CustomEvent(name, { detail }));
}

/**
 * Adds an event listener (or more) to an element and returns a function to unsubscribe.
 * @param $elem
 * @param type
 * @param listener
 * @param options
 */
export function addListener<T extends Event, eventType extends string>(
	$elem: EventTarget,
	type: eventType[] | eventType,
	listener: (e: T) => void,
	options?: boolean | AddEventListenerOptions,
): EventListenerSubscription {
	const types = Array.isArray(type) ? type : [type];
	types.forEach((t) => $elem.addEventListener(t, listener as EventListenerOrEventListenerObject, options));
	return () =>
		types.forEach((t) => $elem.removeEventListener(t, listener as EventListenerOrEventListenerObject, options));
}

/**
 * Removes the event listeners in the array.
 * @param listeners
 */
export function removeListeners(listeners: EventListenerSubscription[]) {
	listeners.forEach((unsub) => unsub());
}
