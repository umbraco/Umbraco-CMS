import { GLOBAL_ROUTER_EVENTS_TARGET } from '../config.js';
import type { EventListenerSubscription, GlobalRouterEvent, IRoutingInfo } from '../model.js';

/**
 * Dispatches a did change route event.
 * @template D
 * @param {HTMLElement} $elem - The element to dispatch the event on.
 * @param {IRoutingInfo<D>} detail - The routing info to dispatch.
 */
export function dispatchRouteChangeEvent<D = any>($elem: HTMLElement, detail: IRoutingInfo<D>) {
	$elem.dispatchEvent(new CustomEvent('changestate', { detail }));
}

/**
 * Dispatches an event on the window object.
 * @template D
 * @param {GlobalRouterEvent} name - The name of the event to dispatch.
 * @param {IRoutingInfo<D>} [detail] - The routing info to dispatch.
 */
export function dispatchGlobalRouterEvent<D = any>(name: GlobalRouterEvent, detail?: IRoutingInfo<D>) {
	GLOBAL_ROUTER_EVENTS_TARGET.dispatchEvent(new CustomEvent(name, { detail }));
}

/**
 * Adds an event listener (or more) to an element and returns a function to unsubscribe.
 * @template {Event} T
 * @template {string} eventType
 * @param {EventTarget} $elem - The element to add the listener(s) to.
 * @param {eventType[] | eventType} type - The event type(s) to listen for.
 * @param {(e: T) => void} listener - The listener callback.
 * @param {boolean | AddEventListenerOptions} [options] - The event listener options.
 * @returns {EventListenerSubscription} A function to unsubscribe the listener(s).
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
 * @param {EventListenerSubscription[]} listeners - The listeners to remove.
 */
export function removeListeners(listeners: EventListenerSubscription[]) {
	listeners.forEach((unsub) => unsub());
}
