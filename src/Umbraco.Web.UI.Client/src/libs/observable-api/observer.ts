import { Observable, Subscription, lastValueFrom } from '@umbraco-cms/backoffice/external/rxjs';

export type ObserverCallbackStack<T> = {
	next: (_value: T) => void;
	error?: (_value: unknown) => void;
	complete?: () => void;
};

export type ObserverCallback<T> = ((_value: T) => void) | ObserverCallbackStack<T>;

export class UmbObserver<T> {
	#source!: Observable<T>;
	#callback!: ObserverCallback<T>;
	#subscription!: Subscription;

	constructor(source: Observable<T>, callback: ObserverCallback<T>) {
		this.#source = source;
		this.#subscription = source.subscribe(callback);
	}

	public async asPromise() {
		return await lastValueFrom(this.#source);
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
