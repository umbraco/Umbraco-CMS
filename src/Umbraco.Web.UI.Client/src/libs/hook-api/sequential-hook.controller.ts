import type { UmbSequentialHookEntry, UmbSequentialHookMethod } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

type UmbSequentialHookEntryWithOrder<ValueType, MetaType extends Record<string, unknown>> = UmbSequentialHookEntry<
	ValueType,
	MetaType
> & {
	order: number;
};

/**
 * A hook whose methods run one after the other, each receiving the value returned by the previous one.
 * Use it when the methods transform a value and the order matters.
 * @template ValueType - The type of the value passed through the methods.
 * @template MetaType - The type of the meta passed to every method.
 */
export class UmbSequentialHookController<
	ValueType,
	MetaType extends Record<string, unknown> = Record<string, unknown>,
> extends UmbControllerBase {
	// Always sorted: highest weight first, and in the order of adding for the same weight.
	#entries: Array<UmbSequentialHookEntryWithOrder<ValueType, MetaType>> = [];
	#nextOrder = 0;

	/**
	 * Adds a method to the hook. Methods run in order of weight, highest first.
	 * A method can also be added while the hook is executing, it then runs if it comes after the method that is
	 * currently running, and is skipped by that execution if it comes before.
	 * @param {UmbSequentialHookMethod} method - The method to run.
	 * @param {number} [weight] - Without a weight the method comes after all methods added so far.
	 */
	add(method: UmbSequentialHookMethod<ValueType, MetaType>, weight?: number): void {
		const lowestWeight = this.#entries[this.#entries.length - 1]?.weight;
		const resolvedWeight = weight ?? (lowestWeight === undefined ? 0 : lowestWeight - 1);
		const entry = { method, weight: resolvedWeight, order: this.#nextOrder++ };

		const index = this.#entries.findIndex((existing) => existing.weight < resolvedWeight);
		this.#entries.splice(index === -1 ? this.#entries.length : index, 0, entry);
	}

	/**
	 * Removes a method from the hook. Executions that are already running skip it if it has not run yet.
	 * @param {UmbSequentialHookMethod} method - The method to remove.
	 */
	remove(method: UmbSequentialHookMethod<ValueType, MetaType>): void {
		const index = this.#entries.findIndex((entry) => entry.method === method);
		if (index !== -1) {
			this.#entries.splice(index, 1);
		}
	}

	/**
	 * Passes the value through all methods in order of weight, highest first.
	 * @param {ValueType | Promise<ValueType>} data - The initial value.
	 * @param {MetaType} meta - Passed to every method.
	 * @returns {Promise<ValueType>} The value returned by the last method, or the initial value when there are no methods.
	 */
	async execute(data: ValueType | Promise<ValueType>, meta: MetaType): Promise<ValueType> {
		let result = await data;
		let previous: UmbSequentialHookEntryWithOrder<ValueType, MetaType> | undefined;

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

	override destroy(): void {
		super.destroy();
		this.#entries = [];
	}
}
