import { UMB_CLIPBOARD_ENTRY_PICKER_MODAL } from '../clipboard-entry/picker-modal/index.js';
import {
	UmbClipboardEntryDetailRepository,
	UmbPasteClipboardEntryTranslateController,
	type UmbClipboardEntryDetailModel,
} from '../clipboard-entry/index.js';
import { UMB_CLIPBOARD_CONTEXT } from './clipboard.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbPropertyValueCloneController } from '@umbraco-cms/backoffice/property';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

/**
 * Clipboard context for managing clipboard entries
 * @export
 * @class UmbClipboardContext
 * @augments {UmbContextBase<UmbClipboardContext>}
 */
export class UmbClipboardContext extends UmbContextBase<UmbClipboardContext> {
	#entries = new UmbArrayState<UmbClipboardEntryDetailModel>([], (x) => x.unique);
	#init?: Promise<unknown>;

	/**
	 * Observable that emits all entries in the clipboard
	 * @memberof UmbClipboardContext
	 */
	public readonly entries = this.#entries.asObservable();

	/**
	 * Observable that emits true if there are any entries in the clipboard
	 * @memberof UmbClipboardContext
	 */
	public hasEntries = this.#entries.asObservablePart((x) => x.length > 0);

	#modalManagerContext?: typeof UMB_MODAL_MANAGER_CONTEXT.TYPE;
	#clipboardDetailRepository = new UmbClipboardEntryDetailRepository(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_CLIPBOARD_CONTEXT);

		this.#init = Promise.all([
			this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (context) => {
				this.#modalManagerContext = context;
			}).asPromise(),
		]);
	}

	/**
	 * Create an entry in the clipboard
	 * @param {UmbClipboardEntryDetailModel} entry A clipboard entry to insert into the clipboard
	 * @memberof UmbClipboardContext
	 */
	async write(entry: UmbClipboardEntryDetailModel): Promise<void> {
		if (!entry) throw new Error('Entry is required');
		if (!entry.unique) throw new Error('Entry must have a unique property');
		this.#clipboardDetailRepository.create(entry);
	}

	async pick(args: { multiple: boolean; propertyEditorUiAlias: string }): Promise<Array<any>> {
		await this.#init;
		const modal = this.#modalManagerContext?.open(this, UMB_CLIPBOARD_ENTRY_PICKER_MODAL);
		const result = await modal?.onSubmit();

		if (!result?.selection.length) {
			throw new Error('No clipboard entry selected');
		}

		const propertyEditorUiManifest = umbExtensionsRegistry.getByAlias<ManifestPropertyEditorUi>(
			args.propertyEditorUiAlias,
		);

		if (!propertyEditorUiManifest) {
			throw new Error(`Could not find property editor with alias: ${args.propertyEditorUiAlias}`);
		}

		if (propertyEditorUiManifest.type !== 'propertyEditorUi') {
			throw new Error(`Alias ${args.propertyEditorUiAlias} is not a property editor ui`);
		}

		if (!propertyEditorUiManifest.meta.propertyEditorSchemaAlias) {
			throw new Error('Property editor does not have a schema alias');
		}

		let propertyValues: Array<any> = [];

		if (args.multiple) {
			throw new Error('Multiple clipboard entries not supported');
		} else {
			const selected = result?.selection[0];

			if (!selected) {
				throw new Error('No clipboard entry selected');
			}

			const propertyValue = await this.#resolveEntry(selected, propertyEditorUiManifest.meta.propertyEditorSchemaAlias);
			propertyValues = [propertyValue];
		}

		return propertyValues;
	}

	async #resolveEntry(unique: string, schemaAlias: string): Promise<any> {
		if (!unique) {
			throw new Error('Unique id is required');
		}

		if (!schemaAlias) {
			throw new Error('Editor alias is required');
		}

		const { data: entry } = await this.#clipboardDetailRepository.requestByUnique(unique);

		if (!entry) {
			throw new Error(`Could not find clipboard entry with unique id: ${unique}`);
		}

		let propertyValue = undefined;

		const translator = new UmbPasteClipboardEntryTranslateController(this);
		propertyValue = await translator.translate(entry);

		const cloner = new UmbPropertyValueCloneController(this);
		const clonedValue = await cloner.clone({
			editorAlias: schemaAlias,
			value: propertyValue,
		});

		propertyValue = clonedValue.value;

		return propertyValue;
	}
}

export { UmbClipboardContext as api };
