import { UMB_CLIPBOARD_CONTEXT } from '../../context/index.js';
import {
	UMB_CLIPBOARD_ENTRY_PICKER_MODAL,
	type UmbClipboardEntryDetailModel,
	type UmbClipboardEntryValueModel,
	type UmbClipboardEntryValuesType,
} from '../../clipboard-entry/index.js';
import type { ManifestClipboardPastePropertyValueTranslator } from '../value-translator/types.js';
import {
	UmbClipboardCopyPropertyValueTranslatorValueResolver,
	UmbClipboardPastePropertyValueTranslatorValueResolver,
} from '../value-translator/index.js';
import { UMB_CLIPBOARD_PROPERTY_CONTEXT } from './clipboard.property-context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_PROPERTY_CONTEXT, UmbPropertyValueCloneController } from '@umbraco-cms/backoffice/property';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

/**
 * Clipboard context for managing clipboard entries for property values
 * @export
 * @class UmbClipboardPropertyContext
 * @augments {UmbContextBase<UmbClipboardPropertyContext>}
 */
export class UmbClipboardPropertyContext extends UmbContextBase<UmbClipboardPropertyContext> {
	#init?: Promise<unknown>;

	#modalManagerContext?: typeof UMB_MODAL_MANAGER_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host, UMB_CLIPBOARD_PROPERTY_CONTEXT);

		this.#init = Promise.all([
			this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (context) => {
				this.#modalManagerContext = context;
			}).asPromise(),
		]);
	}

	/**
	 * Read a clipboard entry for a property. The entry will be translated to the property editor value
	 * @param {string} unique - The unique id of the clipboard entry
	 * @param {string} propertyEditorUiAlias - The alias of the property editor to match
	 * @returns { Promise<unknown> } - Returns the resolved property value
	 */
	async read<ReturnType = unknown>(unique: string, propertyEditorUiAlias: string): Promise<ReturnType | undefined> {
		if (!unique) throw new Error('The Clipboard Entry unique is required');
		if (!propertyEditorUiAlias) throw new Error('Property Editor UI alias is required');
		const manifest = await this.#findPropertyEditorUiManifest(propertyEditorUiAlias);
		return this.#resolvePropertyValue<ReturnType>(unique, manifest);
	}

	/**
	 * Read multiple clipboard entries for a property. The entries will be translated to the property editor values
	 * @param {Array<string>} uniques - The unique ids of the clipboard entries
	 * @param {string} propertyEditorUiAlias - The alias of the property editor to match
	 * @returns { Promise<Array<unknown>> } - Returns an array of resolved property values
	 */
	async readMultiple<ReturnType = unknown>(
		uniques: Array<string>,
		propertyEditorUiAlias: string,
	): Promise<Array<ReturnType>> {
		if (!uniques || !uniques.length) {
			throw new Error('Clipboard entry uniques are required');
		}

		const promises = Promise.allSettled(uniques.map((unique) => this.read(unique, propertyEditorUiAlias)));

		const readResult = await promises;
		// TODO:show message if some entries are not fulfilled
		const fulfilledResult = readResult.filter((result) => result.status === 'fulfilled' && result.value) as Array<
			PromiseFulfilledResult<ReturnType>
		>;
		// Map the values and remove undefined.
		const propertyValues = fulfilledResult.map((result) => result.value).filter((x) => x);

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
	async write(args: {
		name: string;
		icon?: string;
		propertyValue: any;
		propertyEditorUiAlias: string;
	}): Promise<UmbClipboardEntryDetailModel | undefined> {
		const clipboardContext = await this.getContext(UMB_CLIPBOARD_CONTEXT);

		const copyValueResolver = new UmbClipboardCopyPropertyValueTranslatorValueResolver(this);
		const values = await copyValueResolver.resolve(args.propertyValue, args.propertyEditorUiAlias);

		const entryPreset: Partial<UmbClipboardEntryDetailModel> = {
			name: args.name,
			values,
			icon: args.icon,
		};

		return await clipboardContext.write(entryPreset);
	}

	/**
	 * Pick a clipboard entry for a property. The entry will be translated to the property editor value
	 * @param args - Arguments for picking a clipboard entry
	 * @param {boolean} args.multiple - Allow multiple clipboard entries to be picked
	 * @param {string} args.propertyEditorUiAlias - The alias of the property editor to match
	 * @param {() => Promise<boolean>} args.filter - A filter function to filter clipboard entries
	 * @returns { Promise<{ selection: Array<UmbEntityUnique>; propertyValues: Array<any> }> }
	 */
	async pick(args: {
		multiple: boolean;
		propertyEditorUiAlias: string;
		filter?: (value: any, config: any) => Promise<boolean>;
	}): Promise<{ selection: Array<UmbEntityUnique>; propertyValues: Array<any> }> {
		await this.#init;

		const pasteTranslatorManifests = this.getPasteTranslatorManifests(args.propertyEditorUiAlias);
		const propertyEditorUiManifest = await this.#findPropertyEditorUiManifest(args.propertyEditorUiAlias);
		const config = (await this.getContext(UMB_PROPERTY_CONTEXT)).getConfig();

		const valueResolver = new UmbClipboardPastePropertyValueTranslatorValueResolver(this);

		const modal = this.#modalManagerContext?.open(this, UMB_CLIPBOARD_ENTRY_PICKER_MODAL, {
			data: {
				asyncFilter: async (clipboardEntryDetail) => {
					const hasSupportedPasteTranslator = this.hasSupportedPasteTranslator(
						pasteTranslatorManifests,
						clipboardEntryDetail.values,
					);

					if (!hasSupportedPasteTranslator) {
						return false;
					}

					const pasteTranslator = await valueResolver.getPasteTranslator(
						clipboardEntryDetail.values,
						propertyEditorUiManifest.alias,
					);

					if (pasteTranslator.isCompatibleValue) {
						const propertyValue = await valueResolver.resolve(
							clipboardEntryDetail.values,
							propertyEditorUiManifest.alias,
						);

						return pasteTranslator.isCompatibleValue(propertyValue, config, args.filter);
					}

					return true;
				},
			},
		});

		const result = await modal?.onSubmit();
		const selection = result?.selection || [];

		if (!selection.length) {
			throw new Error('No clipboard entry selected');
		}

		let propertyValues: Array<any> = [];

		if (args.multiple) {
			throw new Error('Multiple clipboard entries not supported');
		} else {
			const selected = selection[0];

			if (!selected) {
				throw new Error('No clipboard entry selected');
			}

			const propertyValue = await this.#resolvePropertyValue(selected, propertyEditorUiManifest);
			propertyValues = [propertyValue];
		}

		return {
			selection,
			propertyValues,
		};
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

	async #resolvePropertyValue<ValueType>(
		clipboardEntryUnique: string,
		propertyEditorUiManifest: ManifestPropertyEditorUi,
	): Promise<ValueType | undefined> {
		if (!clipboardEntryUnique) {
			throw new Error('Unique id is required');
		}

		if (!propertyEditorUiManifest.alias) {
			throw new Error('Property Editor UI alias is required');
		}

		if (!propertyEditorUiManifest.meta.propertyEditorSchemaAlias) {
			throw new Error('Property Editor UI Schema alias is required');
		}

		const clipboardContext = await this.getContext(UMB_CLIPBOARD_CONTEXT);
		const entry = await clipboardContext.read(clipboardEntryUnique);

		if (!entry) {
			throw new Error(`Could not find clipboard entry with unique id: ${clipboardEntryUnique}`);
		}

		const valueResolver = new UmbClipboardPastePropertyValueTranslatorValueResolver<ValueType>(this);
		const propertyValue = await valueResolver.resolve(entry.values, propertyEditorUiManifest.alias);

		const cloner = new UmbPropertyValueCloneController(this);
		const clonedValue = await cloner.clone<ValueType>({
			editorAlias: propertyEditorUiManifest.meta.propertyEditorSchemaAlias,
			alias: propertyEditorUiManifest.alias,
			value: propertyValue,
		});

		return clonedValue.value;
	}

	/**
	 * Get all clipboard paste translators for a property editor ui
	 * @param {string} propertyEditorUiAlias - The alias of the property editor to match
	 * @returns {Array<ManifestClipboardPastePropertyValueTranslator>} - Returns an array of clipboard paste translators
	 */
	getPasteTranslatorManifests(propertyEditorUiAlias: string) {
		return umbExtensionsRegistry.getByTypeAndFilter(
			'clipboardPastePropertyValueTranslator',
			(manifest) => manifest.toPropertyEditorUi === propertyEditorUiAlias,
		);
	}

	/**
	 * Check if the clipboard entry values has supported paste translator
	 * @param {Array<ManifestClipboardPastePropertyValueTranslator>} manifests - The paste translator manifests
	 * @param {UmbClipboardEntryValuesType} clipboardEntryValues - The clipboard entry values
	 * @returns {boolean} - Returns true if the clipboard entry values has supported paste translator
	 */
	hasSupportedPasteTranslator(
		manifests: Array<ManifestClipboardPastePropertyValueTranslator>,
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

export { UmbClipboardPropertyContext as api };
