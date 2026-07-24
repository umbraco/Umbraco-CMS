import type { UmbHookEntry, UmbHookMethod } from './types.js';

export class UmbHookController<T> {
	#entries: Array<UmbHookEntry<T>> = [];

	add(method: UmbHookMethod<T>, weight: number = 0): void {
		this.#entries.push({ method, weight });
	}

	remove(method: UmbHookMethod<T>): void {
		const index = this.#entries.findIndex((entry) => entry.method === method);
		if (index !== -1) {
			this.#entries.splice(index, 1);
		}
	}

	async execute(data: T): Promise<T> {
		const sortedEntries = [...this.#entries].sort((a, b) => a.weight - b.weight);

		const result = await sortedEntries.reduce(async (prev, entry) => {
			const current = await prev;
			return entry.method(current);
		}, Promise.resolve(data));

		return result;
	}

	destroy(): void {
		this.#entries = [];
	}
}
