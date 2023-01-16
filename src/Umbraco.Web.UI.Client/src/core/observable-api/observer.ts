import { Observable, Subscription } from 'rxjs';

export class UmbObserver<T> {

	#subscription!: Subscription;

	constructor(source: Observable<T>, callback: (_value: T) => void) {

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
