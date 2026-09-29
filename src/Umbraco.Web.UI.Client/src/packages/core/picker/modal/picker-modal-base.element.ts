import type { UmbPickerContext } from '../picker.context.js';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalInteractionMemoryController } from '@umbraco-cms/backoffice/interaction-memory';
import type { ManifestModal, UmbPickerModalData } from '@umbraco-cms/backoffice/modal';

export abstract class UmbPickerModalBaseElement<
	ItemType = UmbEntityModel,
	ModalDataType extends UmbPickerModalData<ItemType> = UmbPickerModalData<ItemType>,
	ModalValueType = unknown,
	ModalManifestType extends ManifestModal = ManifestModal,
> extends UmbModalBaseElement<ModalDataType, ModalValueType, ModalManifestType> {
	protected abstract _pickerContext: UmbPickerContext;

	constructor() {
		super();
		// `_pickerContext` is a subclass field and does not exist yet, hence the accessors — the controller
		// resolves both on connect.
		new UmbModalInteractionMemoryController(this, {
			memory: () => this._pickerContext.interactionMemory,
			unique: () => this.#getInteractionMemoryUnique(),
		});
	}

	/**
	 * Keyed by modal alias, so different picker modals — and the same modal reached through different
	 * openers — share or isolate their remembered state as appropriate.
	 * @returns {(string | undefined)} The interaction-memory key, or undefined when there is no alias to
	 * key by, in which case nothing is bridged.
	 */
	#getInteractionMemoryUnique(): string | undefined {
		const alias = this.manifest?.alias;
		return alias ? `UmbPickerModal:${alias}` : undefined;
	}
}
