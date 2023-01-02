import { Observable, Subscription } from 'rxjs';

export class UmbObserver<Y = any> {

	#subscription!: Subscription;
	
	constructor(source: Observable<any>, callback: (_value: Y) => void) {

		// TODO: can be transferred to something using alias?
		/*
		if (this._subscriptions.has(source)) {
			const subscription = this._subscriptions.get(source);
			subscription?.unsubscribe();
		}
		*/

		this.#subscription = source.subscribe((value) => callback(value));
	}

	hostDisconnected() {
		this.#subscription.unsubscribe();
	}
};