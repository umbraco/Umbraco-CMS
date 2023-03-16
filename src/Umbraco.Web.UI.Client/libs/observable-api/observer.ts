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
		this.#subscription.unsubscribe();
	}

	destroy(): void {
		this.#subscription.unsubscribe();
	}
}
