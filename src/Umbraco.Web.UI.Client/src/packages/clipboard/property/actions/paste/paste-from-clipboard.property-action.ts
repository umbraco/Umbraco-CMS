import { UmbClipboardEntryItemRepository } from '../../../clipboard-entry/index.js';
import { UMB_CLIPBOARD_PROPERTY_CONTEXT } from '../../context/clipboard.property-context-token.js';
import type { MetaPropertyActionPasteFromClipboardKind } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { escapeHTML } from '@umbraco-cms/backoffice/utils';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbPropertyActionBase, type UmbPropertyActionArgs } from '@umbraco-cms/backoffice/property-action';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbPasteFromClipboardPropertyAction extends UmbPropertyActionBase<MetaPropertyActionPasteFromClipboardKind> {
	#init: Promise<unknown>;
	#propertyContext?: typeof UMB_PROPERTY_CONTEXT.TYPE;
	#clipboardContext?: typeof UMB_CLIPBOARD_PROPERTY_CONTEXT.TYPE;
	readonly #localize = new UmbLocalizationController(this);

	constructor(host: UmbControllerHost, args: UmbPropertyActionArgs<MetaPropertyActionPasteFromClipboardKind>) {
		super(host, args);

		this.#init = Promise.all([
			this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
				this.#propertyContext = context;
			}).asPromise({ preventTimeout: true }),

			this.consumeContext(UMB_CLIPBOARD_PROPERTY_CONTEXT, (context) => {
				this.#clipboardContext = context;
			}).asPromise({ preventTimeout: true }),
		]);
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected async _pickerFilter(value: any, config: any) {
		return true;
	}

	override async execute() {
		await this.#init;
		if (!this.#clipboardContext) throw new Error('Clipboard context not found');
		if (!this.#propertyContext) throw new Error('Property context not found');

		const propertyEditorManifest = this.#propertyContext.getEditorManifest();

		if (!propertyEditorManifest) {
			throw new Error('Property editor manifest not found');
		}

		const result = await this.#clipboardContext.pick({
			propertyEditorUiAlias: propertyEditorManifest.alias,
			multiple: false,
			filter: this._pickerFilter,
		});

		const selectedUnique = result.selection[0];
		const propertyValue = result.propertyValues[0];

		if (!selectedUnique) {
			throw new Error('No clipboard entry selected');
		}

		if (!propertyValue) {
			throw new Error('No property value found');
		}

		const hasCurrentPropertyValue = this.#propertyContext.getValue();

		if (hasCurrentPropertyValue) {
			const clipboardEntryItemRepository = new UmbClipboardEntryItemRepository(this);
			const { data } = await clipboardEntryItemRepository.requestItems([selectedUnique]);

			if (!data || data.length === 0) {
				throw new Error('Clipboard entry not found');
			}

			const item = data[0];

			await umbConfirmModal(this, {
				headline: '#clipboard_confirmPasteHeadline',
				content: this.#localize.term('clipboard_confirmPasteOverwriteMessage', escapeHTML(item.name ?? '')),
				confirmLabel: '#defaultdialogs_paste',
			});
		}

		this.#propertyContext?.setValue(propertyValue);
	}
}
export { UmbPasteFromClipboardPropertyAction as api };
