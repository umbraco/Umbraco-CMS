import type { UmbHookEntry, UmbHookMethod } from './types.js';

export class UmbHookController<ValueType, MetaType extends Record<string, unknown> = Record<string, unknown>> {
	#entries: Array<UmbHookEntry<ValueType, MetaType>> = [];

	add(method: UmbHookMethod<ValueType, MetaType>, weight: number = 0): void {
		this.#entries.push({ method, weight });
	}

	remove(method: UmbHookMethod<ValueType, MetaType>): void {
		const index = this.#entries.findIndex((entry) => entry.method === method);
		if (index !== -1) {
			this.#entries.splice(index, 1);
		}
	}

	async execute(data: ValueType | Promise<ValueType>, meta: MetaType): Promise<ValueType> {
		const sortedEntries = [...this.#entries].sort((a, b) => a.weight - b.weight);

		const result = await sortedEntries.reduce(async (prev: ValueType | Promise<ValueType>, entry) => {
			const prevResolved = await prev;
			return entry.method(prevResolved, meta);
		}, Promise.resolve(data));

		return result;
	}

	destroy(): void {
		this.#entries = [];
	}
}
