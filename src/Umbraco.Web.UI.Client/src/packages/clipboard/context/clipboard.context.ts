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
	#init?: Promise<unknown>;

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
	 * Write to the clipboard
	 * @param {Partial<UmbClipboardEntryDetailModel>} entryPreset - The preset for the clipboard entry
	 * @returns {Promise<void>}
	 * @memberof UmbClipboardContext
	 */
	async write(entryPreset: Partial<UmbClipboardEntryDetailModel>): Promise<void> {
		if (!entryPreset) throw new Error('Entry preset is required');

		const { data: scaffoldData } = await this.#clipboardDetailRepository.createScaffold(entryPreset);
		if (!scaffoldData) return;

		await this.#clipboardDetailRepository.create(scaffoldData);
	}

	/**
	 * Read a clipboard entry for a property. The entry will be translated to the property editor value
	 * @param {string} clipboardEntryUnique - The unique id of the clipboard entry
	 * @param {string} propertyEditorUiAlias - The alias of the property editor to match
	 * @returns { Promise<unknown> } - Returns the resolved property value
	 */
	async readForProperty(clipboardEntryUnique: string, propertyEditorUiAlias: string): Promise<unknown> {
		if (!clipboardEntryUnique) throw new Error('The Clipboard Entry unique is required');
		if (!propertyEditorUiAlias) throw new Error('Property Editor UI alias is required');
		const manifest = await this.#findPropertyEditorUiManifest(propertyEditorUiAlias);
		return this.#resolveEntry(clipboardEntryUnique, manifest);
	}

	/**
	 * Read multiple clipboard entries for a property. The entries will be translated to the property editor values
	 * @param {Array<string>} clipboardEntryUniques - The unique ids of the clipboard entries
	 * @param {string} propertyEditorUiAlias - The alias of the property editor to match
	 * @returns { Promise<Array<unknown>> } - Returns an array of resolved property values
	 */
	async readMultipleForProperty(
		clipboardEntryUniques: Array<string>,
		propertyEditorUiAlias: string,
	): Promise<Array<unknown>> {
		if (!clipboardEntryUniques || !clipboardEntryUniques.length) {
			throw new Error('Clipboard entry uniques are required');
		}

		const promises = Promise.allSettled(
			clipboardEntryUniques.map((unique) => this.readForProperty(unique, propertyEditorUiAlias)),
		);

		const readResult = await promises;

		// TODO:show message if some entries are not fulfilled
		const fulfilledResult = readResult.filter((result) => result.status === 'fulfilled' && result.value) as Array<
			PromiseFulfilledResult<unknown>
		>;
		const propertyValues = fulfilledResult.map((result) => result.value);

		if (!propertyValues.length) {
			throw new Error('Failed to read clipboard entries');
		}

		return propertyValues;
	}

	/**
	 * Write a clipboard entry for a property. The property value will be translated to the clipboard entry values
	 * @param args - Arguments for writing a clipboard entry
	 * @param {string} args.name - The name of the clipboard entry
	 * @param {string} args.icon - The icon of the clipboard entry
	 * @param {any} args.propertyValue - The property value to write
	 * @param {string} args.propertyEditorUiAlias - The alias of the property editor to match
	 * @returns { Promise<void> }
	 */
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

		await this.write(entryPreset);
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

		const pasteTranslatorManifests = this.getPasteTranslatorManifestsForPropertyEditorUi(args.propertyEditorUiAlias);

		const modal = this.#modalManagerContext?.open(this, UMB_CLIPBOARD_ENTRY_PICKER_MODAL, {
			data: {
				filter: (clipboardEntryDetail) =>
					this.hasSupportedPasteTranslator(pasteTranslatorManifests, clipboardEntryDetail.values),
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

	async #resolveEntry(
		clipboardEntryUnique: string,
		propertyEditorUiManifest: ManifestPropertyEditorUi,
	): Promise<unknown> {
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

		const valueResolver = new UmbClipboardPasteTranslatorValueResolver(this);
		const propertyValue = await valueResolver.resolve(entry.values, propertyEditorUiManifest.alias);

		const cloner = new UmbPropertyValueCloneController(this);
		const clonedValue = await cloner.clone({
			editorAlias: propertyEditorUiManifest.meta.propertyEditorSchemaAlias,
			alias: propertyEditorUiManifest.alias,
			value: propertyValue,
		});

		return clonedValue.value;
	}

	/**
	 * Get all clipboard paste translators for a property editor ui
	 * @param {string} propertyEditorUiAlias - The alias of the property editor to match
	 * @returns {Array<ManifestClipboardPasteTranslator>} - Returns an array of clipboard paste translators
	 */
	getPasteTranslatorManifestsForPropertyEditorUi(propertyEditorUiAlias: string) {
		return umbExtensionsRegistry.getByTypeAndFilter(
			'clipboardPasteTranslator',
			(manifest) => manifest.toPropertyEditorUi === propertyEditorUiAlias,
		);
	}

	/**
	 * Check if the clipboard entry values has supported paste translator
	 * @param {Array<ManifestClipboardPasteTranslator>} manifests - The paste translator manifests
	 * @param {UmbClipboardEntryValuesType} clipboardEntryValues - The clipboard entry values
	 * @returns {boolean} - Returns true if the clipboard entry values has supported paste translator
	 */
	hasSupportedPasteTranslator(
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
