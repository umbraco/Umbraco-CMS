import { Observable, Subscription } from 'rxjs';

export class UmbObserver<T = any> {

	#subscription!: Subscription;
	
	constructor(source: Observable<any>, callback: (_value: T) => void) {

		// TODO: can be transferred to something using alias?
		/*
		if (this._subscriptions.has(source)) {
			const subscription = this._subscriptions.get(source);
			subscription?.unsubscribe();
		}
		*/

		this.#subscription = source.subscribe((value) => callback(value));
	}

	// Notice controller class implements empty hostConnected().

	hostDisconnected() {
		this.#subscription.unsubscribe();
	}

	destroy(): void {
		this.#subscription.unsubscribe();
	}

};