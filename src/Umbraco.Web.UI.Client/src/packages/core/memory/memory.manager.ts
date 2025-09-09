import type { UmbMemoryModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbMemoryManager extends UmbControllerBase {
	#memories = new Map<string, UmbMemoryModel>();

	get(unique: string): UmbMemoryModel | undefined {
		return this.#memories.get(unique);
	}

	getAll() {
		return Array.from(this.#memories.values());
	}

	set(memory: UmbMemoryModel) {
		this.#memories.set(memory.unique, memory);
	}

	delete(unique: string) {
		this.#memories.delete(unique);
	}
}
