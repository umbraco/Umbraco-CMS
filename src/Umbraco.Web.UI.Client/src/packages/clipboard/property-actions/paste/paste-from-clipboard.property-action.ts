import { UMB_CLIPBOARD_ENTRY_PICKER_MODAL } from '../../clipboard-entry/index.js';
import type { UmbClipboardPasteResolver } from '../../resolver/types.js';
import type { MetaPropertyActionPasteFromClipboardKind } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbPropertyActionBase, type UmbPropertyActionArgs } from '@umbraco-cms/backoffice/property-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbPasteFromClipboardPropertyAction extends UmbPropertyActionBase<MetaPropertyActionPasteFromClipboardKind> {
	#init: Promise<unknown>;
	#propertyContext?: typeof UMB_PROPERTY_CONTEXT.TYPE;
	#modalManagerContext?: typeof UMB_MODAL_MANAGER_CONTEXT.TYPE;

	#pasteResolverAlias?: string;
	#pasteResolver?: UmbClipboardPasteResolver;

	constructor(host: UmbControllerHost, args: UmbPropertyActionArgs<MetaPropertyActionPasteFromClipboardKind>) {
		super(host, args);

		this.#pasteResolverAlias = args.meta.clipboardPasteResolverAlias;

		this.#init = Promise.all([
			this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
				this.#propertyContext = context;
			}).asPromise(),

			this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (context) => {
				this.#modalManagerContext = context;
			}).asPromise(),
		]);

		if (this.#pasteResolverAlias) {
			new UmbExtensionApiInitializer(
				this,
				umbExtensionsRegistry,
				this.#pasteResolverAlias,
				[this],
				(permitted, ctrl) => {
					this.#pasteResolver = permitted ? (ctrl.api as UmbClipboardPasteResolver) : undefined;
				},
			);
		}
	}

	override async execute() {
		await this.#init;

		if (!this.#pasteResolver) {
			throw new Error('No paste resolver was found');
		}

		const entryTypes = await this.#pasteResolver.getAcceptedTypes();

		const modalContext = this.#modalManagerContext?.open(this, UMB_CLIPBOARD_ENTRY_PICKER_MODAL, {
			data: {
				entryTypes,
			},
		});

		const value = await modalContext?.onSubmit();
		const selectedUnique = value?.selection?.[0];

		if (!selectedUnique) {
			throw new Error('No clipboard entry was returned');
		}

		const entry = await this.#pasteResolver.resolve(selectedUnique);

		if (!entry) {
			throw new Error('No clipboard entry was resolved');
		}

		this.#propertyContext?.setValue(entry.value);
	}
}
export { UmbPasteFromClipboardPropertyAction as api };
