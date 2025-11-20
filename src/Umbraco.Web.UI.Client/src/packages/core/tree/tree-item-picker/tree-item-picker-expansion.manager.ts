import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbEntityExpansionManager } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityExpansionModel } from '@umbraco-cms/backoffice/utils';
import type {
	UmbInteractionMemoryManager,
	UmbInteractionMemoryModel,
} from '@umbraco-cms/backoffice/interaction-memory';

export interface UmbTreeItemPickerExpansionManagerArgs {
	interactionMemoryManager?: UmbInteractionMemoryManager;
}

export class UmbTreeItemPickerExpansionManager extends UmbControllerBase {
	#manager = new UmbEntityExpansionManager(this);
	public readonly expansion = this.#manager.expansion;

	#interactionMemoryManager?: UmbInteractionMemoryManager;
	#interactionMemoryUnique: string = 'UmbTreeItemPickerExpansion';
	#muteMemoryObservation = false;

	constructor(host: UmbControllerHost, args?: UmbTreeItemPickerExpansionManagerArgs) {
		super(host);
		this.#interactionMemoryManager = args?.interactionMemoryManager;

		if (this.#interactionMemoryManager) {
			this.#observeInteractionMemory();
		}
	}

	/**
	 * Sets the full expansion state
	 * @param {UmbEntityExpansionModel} expansion - The full expansion state to set
	 * @memberof UmbTreeItemPickerExpansionManager
	 */
	setExpansion(expansion: UmbEntityExpansionModel): void {
		this.#manager.setExpansion(expansion);

		// Store the latest expansion state in interaction memory
		if (expansion.length > 0) {
			this.#setExpansionMemory();
		} else {
			this.#removeExpansionMemory();
		}
	}

	/**
	 * Gets the current expansion state
	 * @returns {UmbEntityExpansionModel} The full expansion state
	 * @memberof UmbTreeItemPickerExpansionManager
	 */
	getExpansion(): UmbEntityExpansionModel {
		return this.#manager.getExpansion();
	}

	#observeInteractionMemory() {
		this.observe(this.#interactionMemoryManager?.memory(this.#interactionMemoryUnique), (memory) => {
			if (this.#muteMemoryObservation) return;

			if (memory) {
				this.#applyExpansionInteractionMemory(memory);
			}
		});
	}

	#setExpansionMemory() {
		if (!this.#interactionMemoryManager) return;

		// Add a memory entry with the latest expansion state
		const memory: UmbInteractionMemoryModel = {
			unique: this.#interactionMemoryUnique,
			value: {
				expansion: this.getExpansion(),
			},
		};

		this.#muteMemoryObservation = true;
		this.#interactionMemoryManager?.setMemory(memory);
		this.#muteMemoryObservation = false;
	}

	#removeExpansionMemory() {
		if (!this.#interactionMemoryManager) return;
		this.#interactionMemoryManager.deleteMemory(this.#interactionMemoryUnique);
	}

	#applyExpansionInteractionMemory(memory: UmbInteractionMemoryModel) {
		const memoryExpansion = memory?.value?.expansion as UmbEntityExpansionModel | undefined;

		if (memoryExpansion) {
			this.#manager.setExpansion(memoryExpansion);
		}
	}
}
