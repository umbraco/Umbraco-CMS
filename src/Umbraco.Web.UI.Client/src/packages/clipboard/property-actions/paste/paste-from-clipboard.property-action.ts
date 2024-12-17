import { UMB_CLIPBOARD_ENTRY_PICKER_MODAL, UmbClipboardEntryDetailRepository } from '../../clipboard-entry/index.js';
import type { MetaPropertyActionPasteFromClipboardKind } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbPropertyActionBase, type UmbPropertyActionArgs } from '@umbraco-cms/backoffice/property-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbColorPickerPasteFromClipboardPropertyAction extends UmbPropertyActionBase<MetaPropertyActionPasteFromClipboardKind> {
	#detailRepository = new UmbClipboardEntryDetailRepository(this);
	#init: Promise<unknown>;
	#entryType: string;
	#propertyContext?: typeof UMB_PROPERTY_CONTEXT.TYPE;
	#modalManagerContext?: typeof UMB_MODAL_MANAGER_CONTEXT.TYPE;

	constructor(host: UmbControllerHost, args: UmbPropertyActionArgs<MetaPropertyActionPasteFromClipboardKind>) {
		super(host, args);

		if (!args.meta?.entry?.type) {
			throw new Error('The "entry.type" meta property is required');
		}

		this.#entryType = args.meta.entry.type;

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

		const modalContext = this.#modalManagerContext?.open(this, UMB_CLIPBOARD_ENTRY_PICKER_MODAL, {
			data: {
				entry: {
					type: this.#entryType,
				},
			},
		});

		const value = await modalContext?.onSubmit();
		const clipboardEntryUnique = value?.selection?.[0];

		if (clipboardEntryUnique) {
			const { data: entry } = await this.#detailRepository.requestByUnique(clipboardEntryUnique);

			const entryValue = entry?.value;

			if (entryValue) {
				this.#propertyContext?.setValue(entryValue);
			}
		}
	}
}
export { UmbColorPickerPasteFromClipboardPropertyAction as api };
