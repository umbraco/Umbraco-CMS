import { UMB_CLIPBOARD_ENTRY_PICKER_MODAL } from '../clipboard-entry/picker-modal/index.js';
import {
	UmbClipboardCopyTranslatorValueResolver,
	UmbClipboardEntryDetailRepository,
	UmbClipboardPasteTranslatorResolver,
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
import { name } from '../umbraco-package.js';

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

	async write(entryPreset: Partial<UmbClipboardEntryDetailModel>): Promise<void> {
		if (!entryPreset) throw new Error('Entry is required');

		const { data: scaffoldData } = await this.#clipboardDetailRepository.createScaffold(entryPreset);
		if (!scaffoldData) return;

		await this.#clipboardDetailRepository.create(scaffoldData);
	}

	/**
	 * Read a clipboard entry for a property. The entry will be translated to the property editor value
	 * @param {string} clipboardEntryUnique - The unique id of the clipboard entry
	 * @param {string} propertyEditorUiAlias - The alias of the property editor to match
	 * @returns { Promise<UmbClipboardEntryDetailModel | undefined> } - Returns a clipboard entry matching the unique id
	 */
	async readForProperty(
		clipboardEntryUnique: string,
		propertyEditorUiAlias: string,
	): Promise<UmbClipboardEntryDetailModel | undefined> {
		if (!clipboardEntryUnique) throw new Error('The Clipboard Entry unique is required');
		if (!propertyEditorUiAlias) throw new Error('Property Editor UI alias is required');
		const manifest = await this.#findPropertyEditorUiManifest(propertyEditorUiAlias);
		return this.#resolveEntry(clipboardEntryUnique, manifest);
	}

	async writeFromProperty(args: {
		name: string;
		icon?: string;
		propertyValue: any;
		propertyEditorUiAlias: string;
	}): Promise<void> {
		const copyResolver = new UmbClipboardCopyTranslatorValueResolver(this);
		const values = await copyResolver.resolve(args.propertyValue, args.propertyEditorUiAlias);

		const entryPreset = {
			name: args.name,
			value: values,
			icon: args.icon,
		};

		const { data: scaffoldData } = await this.#clipboardDetailRepository.createScaffold(entryPreset);
		if (!scaffoldData) return;

		await this.#clipboardDetailRepository.create(scaffoldData);
	}

	/**
	 * Pick a clipboard entry for a property. The entry will be translated to the property editor value
	 * @param args - Arguments for picking a clipboard entry
	 * @param {boolean} args.multiple - Allow multiple clipboard entries to be picked
	 * @param {string} args.propertyEditorUiAlias - The alias of the property editor to match
	 * @returns { Promise<Array<any>> } - Returns an array of property values matching the property editor alias
	 */
	async pickForProperty(args: { multiple: boolean; propertyEditorUiAlias: string }): Promise<Array<any>> {
		await this.#init;
		const modal = this.#modalManagerContext?.open(this, UMB_CLIPBOARD_ENTRY_PICKER_MODAL);
		const result = await modal?.onSubmit();

		if (!result?.selection.length) {
			throw new Error('No clipboard entry selected');
		}

		const propertyEditorUiManifest = await this.#findPropertyEditorUiManifest(args.propertyEditorUiAlias);

		let propertyValues: Array<any> = [];

		if (args.multiple) {
			throw new Error('Multiple clipboard entries not supported');
		} else {
			const selected = result?.selection[0];

			if (!selected) {
				throw new Error('No clipboard entry selected');
			}

			const propertyValue = await this.#resolveEntry(selected, propertyEditorUiManifest);
			propertyValues = [propertyValue];
		}

		return propertyValues;
	}

	async #findPropertyEditorUiManifest(alias: string): Promise<ManifestPropertyEditorUi> {
		const manifest = umbExtensionsRegistry.getByAlias<ManifestPropertyEditorUi>(alias);

		if (!manifest) {
			throw new Error(`Could not find property editor with alias: ${alias}`);
		}

		if (manifest.type !== 'propertyEditorUi') {
			throw new Error(`Alias ${alias} is not a property editor ui`);
		}

		return manifest;
	}

	async #resolveEntry(unique: string, manifest: ManifestPropertyEditorUi): Promise<any> {
		if (!unique) {
			throw new Error('Unique id is required');
		}

		if (!manifest.alias) {
			throw new Error('Property Editor UI alias is required');
		}

		if (!manifest.meta.propertyEditorSchemaAlias) {
			throw new Error('Property Editor UI Schema alias is required');
		}

		const { data: entry } = await this.#clipboardDetailRepository.requestByUnique(unique);

		if (!entry) {
			throw new Error(`Could not find clipboard entry with unique id: ${unique}`);
		}

		let propertyValue = undefined;

		const translator = new UmbClipboardPasteTranslatorResolver(this);
		propertyValue = await translator.translate(entry, manifest.alias);

		const cloner = new UmbPropertyValueCloneController(this);
		const clonedValue = await cloner.clone({
			editorAlias: manifest.meta.propertyEditorSchemaAlias,
			value: propertyValue,
		});

		propertyValue = clonedValue.value;

		return propertyValue;
	}
}

export { UmbClipboardContext as api };
