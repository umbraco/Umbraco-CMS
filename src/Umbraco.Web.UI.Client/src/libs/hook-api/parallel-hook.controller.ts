import type { UmbParallelHookMethod } from './types.js';

export class UmbParallelHookController<MetaType extends Record<string, unknown> = Record<string, unknown>> {
	#methods: Array<UmbParallelHookMethod<MetaType>> = [];
	// Starts a method in an execution that is running, one for each execution.
	#executions = new Set<(method: UmbParallelHookMethod<MetaType>) => void>();

	/**
	 * Adds a method to the hook. All methods run at the same time when the hook is executed.
	 * A method added while the hook is executing is started right away, and is awaited by that execution.
	 * @param {UmbParallelHookMethod} method - The method to run.
	 */
	add(method: UmbParallelHookMethod<MetaType>): void {
		this.#methods.push(method);
		this.#executions.forEach((start) => start(method));
	}

	remove(method: UmbParallelHookMethod<MetaType>): void {
		const index = this.#methods.indexOf(method);
		if (index !== -1) {
			this.#methods.splice(index, 1);
		}
	}

	/**
	 * Runs all methods at once, and resolves when they have all finished, including methods added meanwhile.
	 * If any method fails, the first failure is thrown once all of them have finished.
	 * @param {MetaType} meta - Passed to every method.
	 */
	async execute(meta: MetaType): Promise<void> {
		const pending = new Set<Promise<void>>();
		const failures: Array<unknown> = [];

		const start = (method: UmbParallelHookMethod<MetaType>) => {
			const run = (async () => method(meta))()
				.catch((error) => {
					failures.push(error);
				})
				.finally(() => pending.delete(run));
			pending.add(run);
		};

		this.#executions.add(start);
		try {
			[...this.#methods].forEach(start);

			// Waiting for the running methods can leave new methods running, so wait until none are left.
			while (pending.size > 0) {
				await Promise.all(pending);
			}
		} finally {
			this.#executions.delete(start);
		}

		if (failures.length > 0) throw failures[0];
	}

	destroy(): void {
		this.#methods = [];
		this.#executions.clear();
	}
}
