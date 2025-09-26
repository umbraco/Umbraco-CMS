import type { UmbPickerContext } from '../picker.context.js';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';
import type { ManifestModal, UmbPickerModalData } from '@umbraco-cms/backoffice/modal';
import { UMB_PICKER_INPUT_CONTEXT } from '@umbraco-cms/backoffice/picker-input';

export abstract class UmbPickerModalBaseElement<
	ItemType = UmbEntityModel,
	ModalDataType extends UmbPickerModalData<ItemType> = UmbPickerModalData<ItemType>,
	ModalValueType = unknown,
	ModalManifestType extends ManifestModal = ManifestModal,
> extends UmbModalBaseElement<ModalDataType, ModalValueType, ModalManifestType> {
	protected abstract _pickerContext: UmbPickerContext;

	#pickerInputContext?: typeof UMB_PICKER_INPUT_CONTEXT.TYPE;

	constructor() {
		super();
		this.consumeContext(UMB_PICKER_INPUT_CONTEXT, (pickerInputContext) => {
			this.#pickerInputContext = pickerInputContext;
			this.#observeMemoriesFromInputContext();
		});
	}

	override connectedCallback(): void {
		super.connectedCallback();
		this.#observeMemoriesFromPicker();
	}

	#observeMemoriesFromPicker() {
		this.observe(this._pickerContext.interactionMemory.memories, (memories) => {
			this.#setMemoriesOnInputContext(memories);
		});
	}

	#getInteractionMemoryUnique() {
		// TODO: consider appending with a unique when we have that implemented.
		return `UmbPickerModal`;
	}

	#observeMemoriesFromInputContext() {
		this.observe(
			this.#pickerInputContext?.interactionMemory.memory(this.#getInteractionMemoryUnique()),
			(memory) => {
				memory?.memories?.forEach((memory) => this._pickerContext.interactionMemory.setMemory(memory));
			},
			'umbModalInteractionMemoryObserver',
		);
	}

	#setMemoriesOnInputContext(pickerMemories: Array<UmbInteractionMemoryModel>) {
		if (pickerMemories?.length > 0) {
			const pickerModalMemory: UmbInteractionMemoryModel = {
				unique: this.#getInteractionMemoryUnique(),
				memories: pickerMemories,
			};

			this.#pickerInputContext?.interactionMemory.setMemory(pickerModalMemory);
		} else {
			this.#pickerInputContext?.interactionMemory.deleteMemory(this.#getInteractionMemoryUnique());
		}
	}
}
