import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbPropertyActionBase, type UmbPropertyActionArgs } from '@umbraco-cms/backoffice/property-action';
import {
	UMB_CLIPBOARD_CONTEXT,
	UMB_CLIPBOARD_ENTRY_ITEM_REPOSITORY_ALIAS,
	UMB_CLIPBOARD_ENTRY_PICKER_MODAL,
	type UmbClipboardEntryDetailModel,
} from '@umbraco-cms/backoffice/clipboard';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';

export class UmbColorPickerPasteFromClipboardPropertyAction extends UmbPropertyActionBase {
	#clipboardContext?: typeof UMB_CLIPBOARD_CONTEXT.TYPE;
	#propertyContext?: typeof UMB_PROPERTY_CONTEXT.TYPE;
	#init?: Promise<unknown>;

	#pickerInputContext = new UmbPickerInputContext<UmbClipboardEntryDetailModel>(
		this,
		UMB_CLIPBOARD_ENTRY_ITEM_REPOSITORY_ALIAS,
		UMB_CLIPBOARD_ENTRY_PICKER_MODAL,
	);

	constructor(host: UmbControllerHost, args: UmbPropertyActionArgs<never>) {
		super(host, args);

		this.#pickerInputContext.max = 1;

		this.#init = Promise.all([
			this.consumeContext(UMB_CLIPBOARD_CONTEXT, (context) => {
				this.#clipboardContext = context;
			}).asPromise(),

			this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
				this.#propertyContext = context;
			}).asPromise(),
		]);
	}

	override async execute() {
		await this.#init;

		try {
			await this.#pickerInputContext.openPicker();
			const value = this.#pickerInputContext.getSelection();
			debugger;
		} catch (error) {}
	}
}
export { UmbColorPickerPasteFromClipboardPropertyAction as api };
