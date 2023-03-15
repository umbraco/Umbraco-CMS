import { Observable, Subscription } from 'rxjs';

export class UmbObserver<T> {
	#source!: Observable<T>;
	#callback!: (_value: T) => void;
	#subscription!: Subscription;

	constructor(source: Observable<T>, callback: (_value: T) => void) {
		this.#source = source;
		this.#subscription = source.subscribe(callback);
	}

	hostConnected() {
		if (this.#subscription.closed) {
			this.#subscription = this.#source.subscribe(this.#callback);
		}
	}

	hostDisconnected() {
		// No cause then it cant re-connect, if the same element just was moved in DOM.
		//this.#subscription.unsubscribe();
	}

	destroy(): void {
		this.#subscription.unsubscribe();
	}
}
