import type { UmbPickerContext } from '../picker.context.js';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UMB_INTERACTION_MEMORY_SCOPE_CONTEXT } from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';
import type { ManifestModal, UmbPickerModalData } from '@umbraco-cms/backoffice/modal';
import type { PropertyValues } from '@umbraco-cms/backoffice/external/lit';

export abstract class UmbPickerModalBaseElement<
	ItemType = UmbEntityModel,
	ModalDataType extends UmbPickerModalData<ItemType> = UmbPickerModalData<ItemType>,
	ModalValueType = unknown,
	ModalManifestType extends ManifestModal = ManifestModal,
> extends UmbModalBaseElement<ModalDataType, ModalValueType, ModalManifestType> {
	protected abstract _pickerContext: UmbPickerContext;

	#memoryScope?: typeof UMB_INTERACTION_MEMORY_SCOPE_CONTEXT.TYPE;

	constructor() {
		super();
		this.consumeContext(UMB_INTERACTION_MEMORY_SCOPE_CONTEXT, (memoryScope) => {
			this.#memoryScope = memoryScope;
			this.#observeMemoriesFromScope();
		});
	}

	override connectedCallback(): void {
		super.connectedCallback();
		this.#observeMemoriesFromPicker();
	}

	protected override updated(changedProperties: PropertyValues): void {
		super.updated(changedProperties);
		// The modal manifest is assigned to this element before it is connected, so this should
		// already be in place by the time the scope context resolves. Re-run on the off chance the
		// two race the other way, so the memory is never bridged under an empty/placeholder alias.
		if (changedProperties.has('manifest')) {
			this.#observeMemoriesFromScope();
		}
	}

	#observeMemoriesFromPicker() {
		this.observe(this._pickerContext.interactionMemory.memories, (memories) => {
			this.#setMemoriesOnScope(memories);
		});
	}

	#getInteractionMemoryUnique() {
		const alias = this.manifest?.alias ?? this.modalContext?.alias;
		return `UmbPickerModal:${alias ?? ''}`;
	}

	#observeMemoriesFromScope() {
		if (!this.#memoryScope) return;
		this.observe(
			this.#memoryScope.interactionMemory.memory(this.#getInteractionMemoryUnique()),
			(memory) => {
				memory?.memories?.forEach((memory) => this._pickerContext.interactionMemory.setMemory(memory));
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

			this.#memoryScope?.interactionMemory.setMemory(pickerModalMemory);
		} else {
			this.#memoryScope?.interactionMemory.deleteMemory(this.#getInteractionMemoryUnique());
		}
	}
}
