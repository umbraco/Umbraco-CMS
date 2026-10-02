import type { UmbHookEntry, UmbHookMethod } from './types.js';

type UmbHookEntryWithOrder<ValueType, MetaType extends Record<string, unknown>> = UmbHookEntry<ValueType, MetaType> & {
	order: number;
};

export class UmbSequentialHookController<
	ValueType,
	MetaType extends Record<string, unknown> = Record<string, unknown>,
> {
	// Always sorted: highest weight first, and in the order of adding for the same weight.
	#entries: Array<UmbHookEntryWithOrder<ValueType, MetaType>> = [];
	#nextOrder = 0;

	/**
	 * Adds a method to the hook. Methods run in order of weight, highest first.
	 * A method can also be added while the hook is executing, it then runs if it comes after the method that is
	 * currently running, and is skipped by that execution if it comes before.
	 * @param {UmbHookMethod} method - The method to run.
	 * @param {number} [weight] - Without a weight the method comes after all methods added so far.
	 */
	add(method: UmbHookMethod<ValueType, MetaType>, weight?: number): void {
		const lowestWeight = this.#entries[this.#entries.length - 1]?.weight;
		const resolvedWeight = weight ?? (lowestWeight === undefined ? 0 : lowestWeight - 1);
		const entry = { method, weight: resolvedWeight, order: this.#nextOrder++ };

		const index = this.#entries.findIndex((existing) => existing.weight < resolvedWeight);
		this.#entries.splice(index === -1 ? this.#entries.length : index, 0, entry);
	}

	remove(method: UmbHookMethod<ValueType, MetaType>): void {
		const index = this.#entries.findIndex((entry) => entry.method === method);
		if (index !== -1) {
			this.#entries.splice(index, 1);
		}
	}

	async execute(data: ValueType | Promise<ValueType>, meta: MetaType): Promise<ValueType> {
		let result = await data;
		let previous: UmbHookEntryWithOrder<ValueType, MetaType> | undefined;

		// Each step takes the entry that follows the one that just ran, from the current list, so methods added meanwhile are included.
		while (true) {
			const last = previous;
			const next = this.#entries.find(
				(entry) => !last || entry.weight < last.weight || (entry.weight === last.weight && entry.order > last.order),
			);
			if (!next) return result;

			previous = next;
			result = await next.method(result, meta);
		}
	}

	destroy(): void {
		this.#entries = [];
	}
}
