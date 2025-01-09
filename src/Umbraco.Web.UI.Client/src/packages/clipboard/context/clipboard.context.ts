import { UMB_CLIPBOARD_ENTRY_PICKER_MODAL } from '../clipboard-entry/picker-modal/index.js';
import {
	UmbClipboardCopyTranslatorValueResolver,
	UmbClipboardEntryDetailRepository,
	UmbClipboardPasteTranslatorValueResolver,
	type UmbClipboardEntryDetailModel,
	type UmbClipboardEntryValuesType,
} from '../clipboard-entry/index.js';
import type { ManifestClipboardPasteTranslator } from '../translator/types.js';
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

	async writeForProperty(args: {
		name: string;
		icon?: string;
		propertyValue: any;
		propertyEditorUiAlias: string;
	}): Promise<void> {
		const copyValueResolver = new UmbClipboardCopyTranslatorValueResolver(this);
		const values = await copyValueResolver.resolve(args.propertyValue, args.propertyEditorUiAlias);

		const entryPreset: Partial<UmbClipboardEntryDetailModel> = {
			name: args.name,
			values,
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

		const pasteTranslatorManifests = this.#getPasteTranslatorManifestsForPropertyEditorUi(args.propertyEditorUiAlias);

		const modal = this.#modalManagerContext?.open(this, UMB_CLIPBOARD_ENTRY_PICKER_MODAL, {
			data: {
				filter: (clipboardEntryDetail) =>
					this.#hasSupportedPasteTranslator(pasteTranslatorManifests, clipboardEntryDetail.values),
			},
		});

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

	async #resolveEntry(clipboardEntryUnique: string, propertyEditorUiManifest: ManifestPropertyEditorUi): Promise<any> {
		if (!clipboardEntryUnique) {
			throw new Error('Unique id is required');
		}

		if (!propertyEditorUiManifest.alias) {
			throw new Error('Property Editor UI alias is required');
		}

		if (!propertyEditorUiManifest.meta.propertyEditorSchemaAlias) {
			throw new Error('Property Editor UI Schema alias is required');
		}

		const { data: entry } = await this.#clipboardDetailRepository.requestByUnique(clipboardEntryUnique);

		if (!entry) {
			throw new Error(`Could not find clipboard entry with unique id: ${clipboardEntryUnique}`);
		}

		let propertyValue = undefined;

		const valueResolver = new UmbClipboardPasteTranslatorValueResolver(this);
		propertyValue = await valueResolver.resolve(entry.values, propertyEditorUiManifest.alias);

		const cloner = new UmbPropertyValueCloneController(this);
		const clonedValue = await cloner.clone({
			editorAlias: propertyEditorUiManifest.meta.propertyEditorSchemaAlias,
			value: propertyValue,
		});

		propertyValue = clonedValue.value;

		return propertyValue;
	}

	#getPasteTranslatorManifestsForPropertyEditorUi(propertyEditorUiAlias: string) {
		return umbExtensionsRegistry.getByTypeAndFilter(
			'clipboardPasteTranslator',
			(manifest) => manifest.toPropertyEditorUi === propertyEditorUiAlias,
		);
	}

	#hasSupportedPasteTranslator(
		manifests: Array<ManifestClipboardPasteTranslator>,
		clipboardEntryValues: UmbClipboardEntryValuesType,
	): boolean {
		const entryValueTypes = clipboardEntryValues.map((x) => x.type);

		const supportedManifests = manifests.filter((manifest) => {
			const canTranslateValue = entryValueTypes.includes(manifest.fromClipboardEntryValueType);
			return canTranslateValue;
		});

		return supportedManifests.length > 0;
	}
}

export { UmbClipboardContext as api };
