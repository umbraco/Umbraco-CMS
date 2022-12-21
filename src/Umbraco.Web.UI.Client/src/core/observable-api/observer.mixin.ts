import { Observable, Subscription } from 'rxjs';
import type { HTMLElementConstructor } from '../models';

export declare class UmbObserverMixinInterface {
	observe<Y = any>(source: Observable<any>, callback: (_value: Y) => void): () => void;
}

export const UmbObserverMixin = <T extends HTMLElementConstructor>(superClass: T) => {
	class UmbObserverMixinClass extends superClass {
		_subscriptions: Map<Observable<any>, Subscription> = new Map();

		observe<Y = any>(source: Observable<any>, callback: (_value: Y) => void): ()=>void {
			if (this._subscriptions.has(source)) {
				const subscription = this._subscriptions.get(source);
				subscription?.unsubscribe();
			}

			const subscription = source.subscribe((value) => callback(value));
			this._subscriptions.set(source, subscription);

			return subscription.unsubscribe;
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
