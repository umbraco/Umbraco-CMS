import { Observable, Subscription } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbObserver<T> {
	#source!: Observable<T>;
	#callback!: (_value: T) => void;
	#subscription!: Subscription;

	constructor(source: Observable<T>, callback: (_value: T) => void) {
		this.#source = source;
		this.#subscription = source.subscribe(callback);
	}

	/**
	 * provides a promise which is resolved ones the observer got a value that is not undefined.
	 * Notice this promise will resolve immediately if the Observable holds an empty array or empty string.
	 *
	 */
	public asPromise() {
		// Notice, we do not want to store and reuse the Promise, cause this promise guarantees that the value is not undefined when resolved. and reusing the promise would not ensure that.
		return new Promise<Exclude<T, undefined>>((resolve) => {
			let initialCallback = true;
			let wantedToClose = false;
			const subscription = this.#source.subscribe((value) => {
				if (value !== undefined) {
					if (initialCallback) {
						wantedToClose = true;
					} else {
						subscription.unsubscribe();
					}
					resolve(value as Exclude<T, undefined>);
				}
			});
			initialCallback = false;
			if (wantedToClose) {
				subscription.unsubscribe();
			}
		});
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
		if (this.#subscription) {
			this.#subscription.unsubscribe();
			(this.#source as any) = undefined;
			(this.#callback as any) = undefined;
			(this.#subscription as any) = undefined;
		}
	}
}
