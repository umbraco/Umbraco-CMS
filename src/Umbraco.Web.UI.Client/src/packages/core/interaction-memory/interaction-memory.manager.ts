import type { UmbInteractionMemoryModel } from './types.js';
import { UmbArrayState, type Observable } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbInteractionMemoryManager extends UmbControllerBase {
	#memories = new UmbArrayState<UmbInteractionMemoryModel>([], (x) => x.unique);
	memories = this.#memories.asObservable();

	memory(unique: string): Observable<UmbInteractionMemoryModel | undefined> {
		return this.#memories.asObservablePart((items) => items.find((item) => item.unique === unique));
	}

	getMemory(unique: string): UmbInteractionMemoryModel | undefined {
		return this.#memories.getValue().find((item) => item.unique === unique);
	}

	setMemory(memory: UmbInteractionMemoryModel) {
		this.#memories.appendOne(memory);
	}

	deleteMemory(unique: string) {
		this.#memories.removeOne(unique);
	}

	getAllMemories() {
		return this.#memories.getValue();
	}

	clear() {
		this.#memories.clear();
	}
}
