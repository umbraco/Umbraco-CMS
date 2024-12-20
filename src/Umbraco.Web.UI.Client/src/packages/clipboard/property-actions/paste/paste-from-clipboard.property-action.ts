import { UMB_CLIPBOARD_CONTEXT } from '../../context/clipboard.context-token.js';
import type { MetaPropertyActionPasteFromClipboardKind } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbPropertyActionBase, type UmbPropertyActionArgs } from '@umbraco-cms/backoffice/property-action';

export class UmbPasteFromClipboardPropertyAction extends UmbPropertyActionBase<MetaPropertyActionPasteFromClipboardKind> {
	#init: Promise<unknown>;
	#propertyContext?: typeof UMB_PROPERTY_CONTEXT.TYPE;
	#clipboardContext?: typeof UMB_CLIPBOARD_CONTEXT.TYPE;

	constructor(host: UmbControllerHost, args: UmbPropertyActionArgs<MetaPropertyActionPasteFromClipboardKind>) {
		super(host, args);

		this.#init = Promise.all([
			this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
				this.#propertyContext = context;
			}).asPromise(),

			this.consumeContext(UMB_CLIPBOARD_CONTEXT, (context) => {
				this.#clipboardContext = context;
			}).asPromise(),
		]);
	}

	override async execute() {
		await this.#init;
		if (!this.#clipboardContext) throw new Error('Clipboard context not found');
		if (!this.#propertyContext) throw new Error('Property context not found');

		const propertyEditorManifest = this.#propertyContext.getEditorManifest();

		if (!propertyEditorManifest) {
			throw new Error('Property editor manifest not found');
		}

		const propertyValues = await this.#clipboardContext.pickForProperty({
			propertyEditorUiAlias: propertyEditorManifest.alias,
			multiple: false,
		});

		const propertyValue = propertyValues[0];
		this.#propertyContext?.setValue(propertyValue);
	}
}
export { UmbPasteFromClipboardPropertyAction as api };
