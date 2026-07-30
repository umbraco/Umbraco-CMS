import { UmbClipboardEntryItemRepository } from '../../../clipboard-entry/index.js';
import { UMB_CLIPBOARD_PROPERTY_CONTEXT } from '../../context/clipboard.property-context-token.js';
import type { MetaPropertyActionPasteFromClipboardKind } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbPropertyActionBase, type UmbPropertyActionArgs } from '@umbraco-cms/backoffice/property-action';

export class UmbPasteFromClipboardPropertyAction extends UmbPropertyActionBase<MetaPropertyActionPasteFromClipboardKind> {
	#init: Promise<unknown>;
	protected _propertyContext?: typeof UMB_PROPERTY_CONTEXT.TYPE;
	#clipboardContext?: typeof UMB_CLIPBOARD_PROPERTY_CONTEXT.TYPE;

	constructor(host: UmbControllerHost, args: UmbPropertyActionArgs<MetaPropertyActionPasteFromClipboardKind>) {
		super(host, args);

		this.#init = Promise.all([
			this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
				this._propertyContext = context;
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

	/**
	 * Adjusts the translated value before it is written to the property. Property editors whose value
	 * needs context that the clipboard entry cannot carry — such as the active variant — override this.
	 * @param {*} value The translated property value.
	 * @returns {Promise<*>} The value to write to the property.
	 * @protected
	 * @memberof UmbPasteFromClipboardPropertyAction
	 */
	protected async _prepareValue(value: any): Promise<any> {
		return value;
	}

	override async execute() {
		await this.#init;
		if (!this.#clipboardContext) throw new Error('Clipboard context not found');
		if (!this._propertyContext) throw new Error('Property context not found');

		const propertyEditorManifest = this._propertyContext.getEditorManifest();

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

		const hasCurrentPropertyValue = this._propertyContext.getValue();

		if (hasCurrentPropertyValue) {
			const clipboardEntryItemRepository = new UmbClipboardEntryItemRepository(this);
			const { data } = await clipboardEntryItemRepository.requestItems([selectedUnique]);

			if (!data || data.length === 0) {
				throw new Error('Clipboard entry not found');
			}

			const item = data[0];

			// Todo: localize
			await umbConfirmModal(this, {
				headline: 'Paste from clipboard',
				content: `The property already contains a value. Paste from the property action will overwrite the current value.
				Do you want to replace the current value with ${item.name}?`,
				confirmLabel: 'Paste',
			});
		}

		this._propertyContext?.setValue(await this._prepareValue(propertyValue));
	}
}
export { UmbPasteFromClipboardPropertyAction as api };
