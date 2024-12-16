import { UMB_CLIPBOARD_ENTRY_PICKER_MODAL, UmbClipboardEntryDetailRepository } from '../../clipboard-entry/index.js';
import type { MetaPropertyActionPasteFromClipboardKind } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbPropertyActionBase, type UmbPropertyActionArgs } from '@umbraco-cms/backoffice/property-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbColorPickerPasteFromClipboardPropertyAction extends UmbPropertyActionBase<MetaPropertyActionPasteFromClipboardKind> {
	#propertyContext?: typeof UMB_PROPERTY_CONTEXT.TYPE;
	#modalManagerContext?: typeof UMB_MODAL_MANAGER_CONTEXT.TYPE;
	#clipboardEntryDetailRepository = new UmbClipboardEntryDetailRepository(this);
	#init?: Promise<unknown>;
	#entryType?: string;

	constructor(host: UmbControllerHost, args: UmbPropertyActionArgs<MetaPropertyActionPasteFromClipboardKind>) {
		super(host, args);

		if (!args.meta?.entry?.type) {
			throw new Error('The "entry.type" meta property is required');
		}

		this.#init = Promise.all([
			this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
				this.#propertyContext = context;
			}).asPromise(),

			this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (context) => {
				this.#modalManagerContext = context;
			}).asPromise(),
		]);
	}

	override async execute() {
		await this.#init;

		try {
			const modalContext = this.#modalManagerContext?.open(this, UMB_CLIPBOARD_ENTRY_PICKER_MODAL);
			const value = await modalContext?.onSubmit();
			const clipboardEntryUnique = value?.selection?.[0];
			console.log(this.#entryType);
			if (clipboardEntryUnique) {
				const { data: clipboardEntry, error } =
					await this.#clipboardEntryDetailRepository.requestByUnique(clipboardEntryUnique);

				const clipboardEntryData = clipboardEntry?.data[0];

				if (clipboardEntryData) {
					this.#propertyContext?.setValue(clipboardEntryData);
				}
			}
		} catch (error) {}
	}
}
export { UmbColorPickerPasteFromClipboardPropertyAction as api };
