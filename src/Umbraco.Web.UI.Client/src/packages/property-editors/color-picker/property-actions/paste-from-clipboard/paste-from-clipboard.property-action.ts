import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbPropertyActionBase, type UmbPropertyActionArgs } from '@umbraco-cms/backoffice/property-action';
import { UMB_CLIPBOARD_CONTEXT } from '@umbraco-cms/backoffice/clipboard';

export class UmbColorPickerPasteFromClipboardPropertyAction extends UmbPropertyActionBase {
	#clipboardContext?: typeof UMB_CLIPBOARD_CONTEXT.TYPE;
	#propertyContext?: typeof UMB_PROPERTY_CONTEXT.TYPE;
	#init?: Promise<unknown>;

	constructor(host: UmbControllerHost, args: UmbPropertyActionArgs<never>) {
		super(host, args);

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
			const value = await this.#clipboardContext?.pick();
			debugger;
		} catch (error) {}
	}
}
export { UmbColorPickerPasteFromClipboardPropertyAction as api };
