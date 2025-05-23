/* eslint-disable local-rules/umb-class-prefix */
/*
 * Copyright 2017 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
 * in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the
 * License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing permissions and
 * limitations under the License.
 */

/**
 * A subset of the `Storage` interface which we need for the backends to work.
 *
 * Essentially removes the indexable properties and readonly properties from
 * `Storage` in lib.dom.d.ts. This is so that a custom type can extend it for
 * testing.
 */
export interface UnderlyingStorage {
	readonly length: number;
	clear(): void;
	getItem(key: string): string | null;
	removeItem(key: string): void;
	setItem(key: string, data: string): void;
}

/**
 * Asynchronous storage APIs. All methods return a `Promise`.
 * All methods take the `DOMString`
 * IDL type (as it is the lowest common denominator).
 */
export abstract class StorageBackend {
	/**
	 * When passed a key `name`, will return that key's value.
	 */
	public abstract getItem(name: string): Promise<string | null>;

	/**
	 * When passed a key `name`, will remove that key from the storage.
	 */
	public abstract removeItem(name: string): Promise<void>;

	/**
	 * When invoked, will empty all keys out of the storage.
	 */
	public abstract clear(): Promise<void>;

	/**
	 * The setItem() method of the `StorageBackend` interface,
	 * when passed a key name and value, will add that key to the storage,
	 * or update that key's value if it already exists.
	 */
	public abstract setItem(name: string, value: string): Promise<void>;
}

/**
 * A `StorageBackend` backed by `localstorage`.
 */
export class LocalStorageBackend extends StorageBackend {
	private storage: UnderlyingStorage;
	constructor(storage?: UnderlyingStorage) {
		super();
		this.storage = storage || window.localStorage;
	}

	public getItem(name: string): Promise<string | null> {
		return new Promise<string | null>((resolve, reject) => {
			const value = this.storage.getItem(name);
			if (value) {
				resolve(value);
			} else {
				resolve(null);
			}
		});
	}

	public removeItem(name: string): Promise<void> {
		return new Promise<void>((resolve, reject) => {
			this.storage.removeItem(name);
			resolve();
		});
	}

	public clear(): Promise<void> {
		return new Promise<void>((resolve, reject) => {
			this.storage.clear();
			resolve();
		});
	}

	public setItem(name: string, value: string): Promise<void> {
		return new Promise<void>((resolve, reject) => {
			this.storage.setItem(name, value);
			resolve();
		});
	}
}
