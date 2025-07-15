/**
 * @description
 * This class provides a Least Recently Used (LRU) cache implementation.
 * It is designed to store key-value pairs and automatically remove the least recently used items when the cache exceeds a maximum size.
 */
export class UmbLruCache<K, V> {
	#cache = new Map<K, V>();

	#maxSize: number;

	constructor(maxSize: number) {
		this.#maxSize = maxSize;
	}

	get(key: K): V | undefined {
		if (!this.#cache.has(key)) return undefined;
		const value = this.#cache.get(key)!;
		this.#cache.delete(key);
		this.#cache.set(key, value);
		return value;
	}

	set(key: K, value: V): void {
		if (this.#cache.has(key)) {
			this.#cache.delete(key);
		} else if (this.#cache.size >= this.#maxSize) {
			const oldestKey = this.#cache.keys().next().value;
			if (oldestKey) {
				this.#cache.delete(oldestKey);
			}
		}
		this.#cache.set(key, value);
	}

	has(key: K): boolean {
		return this.#cache.has(key);
	}
}

export default UmbLruCache;
