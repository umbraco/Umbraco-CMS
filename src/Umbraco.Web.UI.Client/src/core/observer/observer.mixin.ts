import { Observable, Subscription } from 'rxjs';
import type { HTMLElementConstructor } from '../models';

export declare class UmbObserverMixinInterface {
	observe(source: Observable<any>, callback: (value: any) => void): any;
}

export const UmbObserverMixin = <T extends HTMLElementConstructor>(superClass: T) => {
	class UmbObserverMixinClass extends superClass {
		_subscriptions: Map<Observable<unknown>, Subscription> = new Map();

		observe(source: Observable<any>, callback: (_value: any) => void): void {
			if (this._subscriptions.has(source)) {
				const subscription = this._subscriptions.get(source);
				subscription?.unsubscribe();
			}

			const subscription = source.subscribe((value) => callback(value));
			this._subscriptions.set(source, subscription);
		}

		disconnectedCallback() {
			super.disconnectedCallback?.();
			this._subscriptions.forEach((subscription) => subscription.unsubscribe());
		}
	}

	return UmbObserverMixinClass as unknown as HTMLElementConstructor<UmbObserverMixinInterface> & T;
};

declare global {
	interface HTMLElement {
		connectedCallback(): void;
		disconnectedCallback(): void;
	}
}
