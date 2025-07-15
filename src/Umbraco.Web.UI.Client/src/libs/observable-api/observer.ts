import type { Observable, Subscription } from '@umbraco-cms/backoffice/external/rxjs';
export type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export type ObserverCallback<T> = (value: T) => void;

export class UmbObserver<T> {
	#source!: Observable<T>;
	#callback?: ObserverCallback<T>;
	#subscription!: Subscription;

	constructor(source: Observable<T>, callback?: ObserverCallback<T>) {
		this.#source = source;
		if (callback) {
			this.#callback = callback;
			this.#subscription = source.subscribe(callback);
		}
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
						if (!this.#callback) {
							this.destroy();
						}
					}
					resolve(value as Exclude<T, undefined>);
				}
			});
			initialCallback = false;
			if (wantedToClose) {
				subscription.unsubscribe();
				if (!this.#callback) {
					this.destroy();
				}
			}
		});
	}

	hostConnected() {
		// Notice: This will not re-subscribe if this controller was destroyed. Only if the subscription was closed.
		if (this.#subscription?.closed && this.#callback) {
			this.#subscription = this.#source.subscribe(this.#callback);
		}
	}

	hostDisconnected() {
		// No cause then it cant re-connect, if the same element just was moved in DOM. [NL]
		// I do not agree with my self anymore ^^. I think we should unsubscribe here, to help garbage collector and prevent unforeseen side effects of observations continuing while element are out of the DOM. [NL]
		this.#subscription?.unsubscribe();
	}

	destroy(): void {
		if (this.#subscription) {
			this.#subscription.unsubscribe();
			(this.#callback as unknown) = undefined;
			(this.#subscription as unknown) = undefined;
		}
		(this.#source as unknown) = undefined;
	}
}
