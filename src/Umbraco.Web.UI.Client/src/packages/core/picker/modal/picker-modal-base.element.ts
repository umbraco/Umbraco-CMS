import type { UmbPickerContext } from '../picker.context.js';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import {
	UMB_PICKER_INTERACTION_MEMORY_CONTEXT,
	type UmbInteractionMemoryModel,
} from '@umbraco-cms/backoffice/interaction-memory';
import type { ManifestModal, UmbPickerModalData } from '@umbraco-cms/backoffice/modal';

/**
 * Builds the interaction-memory key a picker modal bridges its state under, keyed by modal alias so
 * different picker modals — and the same modal reached through different openers — can share or
 * isolate their remembered state as appropriate.
 * @param {(string | undefined)} alias - The picker modal's alias.
 * @returns {string} The interaction-memory key.
 */
export function umbPickerModalMemoryUnique(alias: string | undefined): string {
	return `UmbPickerModal:${alias ?? ''}`;
}

export abstract class UmbPickerModalBaseElement<
	ItemType = UmbEntityModel,
	ModalDataType extends UmbPickerModalData<ItemType> = UmbPickerModalData<ItemType>,
	ModalValueType = unknown,
	ModalManifestType extends ManifestModal = ManifestModal,
> extends UmbModalBaseElement<ModalDataType, ModalValueType, ModalManifestType> {
	protected abstract _pickerContext: UmbPickerContext;

	#memoryScope?: typeof UMB_PICKER_INTERACTION_MEMORY_CONTEXT.TYPE;

	constructor() {
		super();
		this.consumeContext(UMB_PICKER_INTERACTION_MEMORY_CONTEXT, (memoryScope) => {
			this.#memoryScope = memoryScope;
			this.#observeMemoriesFromScope();
		});
	}

	override connectedCallback(): void {
		super.connectedCallback();
		this.#observeMemoriesFromPicker();
	}

	#observeMemoriesFromPicker() {
		this.observe(this._pickerContext.interactionMemory.memories, (memories) => {
			this.#setMemoriesOnScope(memories);
		});
	}

	#getInteractionMemoryUnique() {
		return umbPickerModalMemoryUnique(this.manifest?.alias);
	}

	#observeMemoriesFromScope() {
		if (!this.#memoryScope) return;
		this.observe(
			this.#memoryScope.memory(this.#getInteractionMemoryUnique()),
			(memory) => {
				this._pickerContext.interactionMemory.setMemories(memory?.memories ?? []);
			},
			'umbModalInteractionMemoryObserver',
		);
	}

	#setMemoriesOnScope(pickerMemories: Array<UmbInteractionMemoryModel>) {
		if (pickerMemories?.length > 0) {
			const pickerModalMemory: UmbInteractionMemoryModel = {
				unique: this.#getInteractionMemoryUnique(),
				memories: pickerMemories,
			};

			this.#memoryScope?.setMemory(pickerModalMemory);
		} else {
			this.#memoryScope?.deleteMemory(this.#getInteractionMemoryUnique());
		}
	}
}
