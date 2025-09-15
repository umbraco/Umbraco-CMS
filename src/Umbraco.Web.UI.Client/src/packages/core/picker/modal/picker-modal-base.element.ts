import type { UmbPickerContext } from '../picker.context.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';
import {
	UmbModalBaseElement,
	type ManifestModal,
	type UmbModalContext,
	type UmbPickerModalData,
} from '@umbraco-cms/backoffice/modal';

export abstract class UmbPickerModalBaseElement<
	ItemType = UmbEntityModel,
	ModalDataType extends UmbPickerModalData<ItemType> = UmbPickerModalData<ItemType>,
	ModalValueType = unknown,
	ModalManifestType extends ManifestModal = ManifestModal,
> extends UmbModalBaseElement<ModalDataType, ModalValueType, ModalManifestType> {
	protected abstract _pickerContext: UmbPickerContext;

	@property({ attribute: false })
	public override set modalContext(context: UmbModalContext<ModalDataType, ModalValueType> | undefined) {
		super.modalContext = context;
		this.#observeModalInteractionMemories();
	}
	public override get modalContext(): UmbModalContext<ModalDataType, ModalValueType> | undefined {
		return super.modalContext;
	}

	override connectedCallback(): void {
		super.connectedCallback();
		// We need to observe the picker memories to be able to update the modal memory.
		// We observe the memories to support close with esc key or clicking outside the modal.
		this.#observePickerModalInteractionMemories();
	}

	#observePickerModalInteractionMemories() {
		this.observe(this._pickerContext.interactionMemory.memories, (memories) => {
			this.#setTreeItemPickerModalMemory(memories);
		});
	}

	#getInteractionMemoryUnique() {
		// TODO: consider appending with a picker unique when we have that implemented.
		return `UmbPickerModalMemory`;
	}

	#observeModalInteractionMemories() {
		this.observe(
			this.modalContext?.interactionMemory.memory(this.#getInteractionMemoryUnique()),
			(memory) => {
				memory?.memories?.forEach((memory) => this._pickerContext.interactionMemory.setMemory(memory));
			},
			'umbModalInteractionMemoryObserver',
		);
	}

	#setTreeItemPickerModalMemory(pickerMemories: Array<UmbInteractionMemoryModel>) {
		if (pickerMemories?.length > 0) {
			const pickerModalMemory: UmbInteractionMemoryModel = {
				unique: this.#getInteractionMemoryUnique(),
				memories: pickerMemories,
			};

			this.modalContext?.interactionMemory.setMemory(pickerModalMemory);
		} else {
			this.modalContext?.interactionMemory.deleteMemory(this.#getInteractionMemoryUnique());
		}
	}
}
