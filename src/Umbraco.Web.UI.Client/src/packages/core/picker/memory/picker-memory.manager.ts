import type { UmbPickerMemory } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export class UmbPickerMemoryManager extends UmbControllerBase {
	#memories = new UmbArrayState<UmbPickerMemory>([], (x) => x.unique);

	constructor(host: UmbControllerHost) {
		super(host);
	}

	get(unique: string): UmbPickerMemory | undefined {
		return this.#memories.getValue().find((x) => x.unique === unique);
	}

	getAll() {
		return this.#memories.getValue();
	}

	append<MemoryType>(memory: UmbPickerMemory<MemoryType>) {
		this.#memories.appendOne(memory);
	}

	delete(unique: string) {
		this.#memories.remove([unique]);
	}
}
