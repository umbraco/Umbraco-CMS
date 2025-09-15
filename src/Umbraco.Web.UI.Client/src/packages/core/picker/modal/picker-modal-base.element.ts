import type { UmbPickerContext } from '../picker.context.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';
import {
	UmbModalBaseElement,
	type ManifestModal,
	type UmbModalContext,
	type UmbModalRejectReason,
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
		this.#observeInteractionMemories();
	}
	public override get modalContext(): UmbModalContext<ModalDataType, ModalValueType> | undefined {
		return super.modalContext;
	}

	protected override _submitModal() {
		this.#setTreeItemPickerModalMemory();
		super._submitModal();
	}

	protected override _rejectModal(reason?: UmbModalRejectReason) {
		this.#setTreeItemPickerModalMemory();
		super._rejectModal(reason);
	}

	#getInteractionMemoryUnique() {
		return 'UmbPickerModalMemory';
	}

	#observeInteractionMemories() {
		this.observe(
			this.modalContext?.interactionMemory.memory(this.#getInteractionMemoryUnique()),
			(memory) => {
				memory?.memories?.forEach((memory) => this._pickerContext.interactionMemory.setMemory(memory));
			},
			'umbModalInteractionMemoryObserver',
		);
	}

	#setTreeItemPickerModalMemory() {
		// Get all memories from the picker context and set them as on combined memory for the picker modal
		const pickerMemories = this._pickerContext.interactionMemory.getAllMemories();
		if (pickerMemories?.length === 0) return;

		const pickerModalMemory: UmbInteractionMemoryModel = {
			unique: this.#getInteractionMemoryUnique(),
			memories: pickerMemories,
		};

		this.modalContext?.interactionMemory.setMemory(pickerModalMemory);
	}
}
