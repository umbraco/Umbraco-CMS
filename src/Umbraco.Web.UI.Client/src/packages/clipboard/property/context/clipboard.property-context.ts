import { UMB_CLIPBOARD_CONTEXT } from '../../context/index.js';
import {
	UMB_CLIPBOARD_ENTRY_PICKER_MODAL,
	type UmbClipboardEntryDetailModel,
	type UmbClipboardEntryValuesType,
} from '../../clipboard-entry/index.js';
import type {
	ManifestClipboardCopyPropertyValueTranslator,
	ManifestClipboardPastePropertyValueTranslator,
} from '../value-translator/types.js';
import {
	UmbClipboardCopyPropertyValueTranslatorValueResolver,
	UmbClipboardPastePropertyValueTranslatorValueResolver,
} from '../value-translator/index.js';
import { UMB_CLIPBOARD_PROPERTY_CONTEXT } from './clipboard.property-context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import {
	UMB_PROPERTY_CONTEXT,
	UMB_PROPERTY_DATASET_CONTEXT,
	UmbPropertyValueCloneController,
} from '@umbraco-cms/backoffice/property';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { mergeObservables, UmbStringState } from '@umbraco-cms/backoffice/observable-api';

/**
 * Clipboard context for managing clipboard entries for property values
 *
 * Every clipboard operation is keyed on a property editor UI alias, because that is what selects the copy and
 * paste value translators. The alias is derived from the surrounding property context, so callers inside a
 * property editor can omit it; callers acting on behalf of another property editor — blocks, for instance —
 * pass it explicitly.
 * @export
 * @class UmbClipboardPropertyContext
 * @augments {UmbContextBase}
 */
export class UmbClipboardPropertyContext extends UmbContextBase {
	#localize = new UmbLocalizationController(this);

	#propertyContext?: typeof UMB_PROPERTY_CONTEXT.TYPE;
	#datasetContext?: typeof UMB_PROPERTY_DATASET_CONTEXT.TYPE;

	#propertyInit: Promise<unknown>;
	#datasetInit: Promise<unknown>;

	#propertyEditorUiAlias = new UmbStringState<string | undefined>(undefined);

	/**
	 * Whether the surrounding property editor can copy its value: its alias resolves and at least one copy
	 * translator targets it. Observe this to gate copy affordances.
	 * @memberof UmbClipboardPropertyContext
	 */
	readonly copyAvailable = mergeObservables(
		[this.#propertyEditorUiAlias.asObservable(), umbExtensionsRegistry.byType('clipboardCopyPropertyValueTranslator')],
		([alias, manifests]) => !!alias && manifests.some((manifest) => manifest.fromPropertyEditorUi === alias),
	);

	/**
	 * Whether the surrounding property editor can paste an entry: its alias resolves and at least one paste
	 * translator targets it. Observe this to gate paste affordances.
	 * @memberof UmbClipboardPropertyContext
	 */
	readonly pasteAvailable = mergeObservables(
		[this.#propertyEditorUiAlias.asObservable(), umbExtensionsRegistry.byType('clipboardPastePropertyValueTranslator')],
		([alias, manifests]) => !!alias && manifests.some((manifest) => manifest.toPropertyEditorUi === alias),
	);

	constructor(host: UmbControllerHost) {
		super(host, UMB_CLIPBOARD_PROPERTY_CONTEXT);

		this.#propertyInit = this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this.#propertyContext = context;

			this.observe(
				context?.editorManifest,
				(manifest) => {
					this.#propertyEditorUiAlias.setValue(manifest?.alias);
				},
				'observePropertyEditorManifest',
			);
		}).asPromise({ preventTimeout: true });

		this.#datasetInit = this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (context) => {
			this.#datasetContext = context;
		}).asPromise({ preventTimeout: true });

		// Nothing awaits these until a clipboard operation, so the rejection on host disconnect would surface as
		// unhandled. Callers still see it through their own await.
		this.#propertyInit.catch(() => undefined);
		this.#datasetInit.catch(() => undefined);
	}

	/**
	 * Read a clipboard entry for a property. The entry will be translated to the property editor value
	 * @param {string} unique - The unique id of the clipboard entry
	 * @param {string} [propertyEditorUiAlias] - The alias of the property editor to match. Defaults to the surrounding property editor
	 * @returns { Promise<unknown> } - Returns the resolved property value
	 */
	async read<ReturnType = unknown>(unique: string, propertyEditorUiAlias?: string): Promise<ReturnType | undefined> {
		if (!unique) throw new Error('The Clipboard Entry unique is required');
		const alias = await this.#requirePropertyEditorUiAlias(propertyEditorUiAlias);
		const manifest = await this.#findPropertyEditorUiManifest(alias);
		return this.#resolvePropertyValue<ReturnType>(unique, manifest);
	}

	/**
	 * Read multiple clipboard entries for a property. The entries will be translated to the property editor values
	 * @param {Array<string>} uniques - The unique ids of the clipboard entries
	 * @param {string} [propertyEditorUiAlias] - The alias of the property editor to match. Defaults to the surrounding property editor
	 * @returns { Promise<Array<unknown>> } - Returns an array of resolved property values
	 */
	async readMultiple<ReturnType = unknown>(
		uniques: Array<string>,
		propertyEditorUiAlias?: string,
	): Promise<Array<ReturnType>> {
		if (!uniques || !uniques.length) {
			throw new Error('Clipboard entry uniques are required');
		}

		const alias = await this.#requirePropertyEditorUiAlias(propertyEditorUiAlias);

		const promises = Promise.allSettled(uniques.map((unique) => this.read<ReturnType>(unique, alias)));

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
	 * @param {any} args.propertyValue - The property value to write
	 * @param {string} [args.name] - The name of the clipboard entry. Defaults to the location of the property
	 * @param {string} [args.itemName] - The label of a single item within the property value, appended to the derived name
	 * @param {string} [args.icon] - The icon of the clipboard entry. Defaults to the icon of the property editor
	 * @param {string} [args.propertyEditorUiAlias] - The alias of the property editor to match. Defaults to the surrounding property editor
	 * @returns { Promise<UmbClipboardEntryDetailModel | undefined> }
	 */
	async write(args: {
		propertyValue: any;
		name?: string;
		itemName?: string;
		icon?: string;
		propertyEditorUiAlias?: string;
	}): Promise<UmbClipboardEntryDetailModel | undefined> {
		const clipboardContext = await this.getContext(UMB_CLIPBOARD_CONTEXT);
		if (!clipboardContext) {
			throw new Error('Clipboard context is required');
		}

		const alias = await this.#requirePropertyEditorUiAlias(args.propertyEditorUiAlias);

		const copyValueResolver = new UmbClipboardCopyPropertyValueTranslatorValueResolver(this);
		const values = await copyValueResolver.resolve(args.propertyValue, alias);

		const entryPreset: Partial<UmbClipboardEntryDetailModel> = {
			name: args.name ?? (await this.#deriveEntryName(args.itemName)),
			values,
			icon: args.icon ?? this.#propertyContext?.getEditorManifest()?.meta.icon,
		};

		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		if (!notificationContext) {
			throw new Error('Notification context is required');
		}

		try {
			const clipboardEntry = await clipboardContext.write(entryPreset);

			notificationContext.peek('positive', {
				data: { message: this.#localize.term('clipboard_copySuccessHeadline') },
			});

			return clipboardEntry;
		} catch (error) {
			const errorMessage = error instanceof Error ? error.message : String(error);
			notificationContext.peek('danger', { data: { message: errorMessage } });
		}

		return undefined;
	}

	/**
	 * Pick a clipboard entry for a property. The entry will be translated to the property editor value
	 * @param args - Arguments for picking a clipboard entry
	 * @param {boolean} args.multiple - Allow multiple clipboard entries to be picked
	 * @param {string} [args.propertyEditorUiAlias] - The alias of the property editor to match. Defaults to the surrounding property editor
	 * @param {() => Promise<boolean>} args.filter - A filter function to filter clipboard entries
	 * @returns { Promise<{ selection: Array<UmbEntityUnique>; propertyValues: Array<any> }> }
	 */
	async pick(args: {
		multiple: boolean;
		propertyEditorUiAlias?: string;
		filter?: (value: any, config: any) => Promise<boolean>;
	}): Promise<{ selection: Array<UmbEntityUnique>; propertyValues: Array<any> }> {
		const alias = await this.#requirePropertyEditorUiAlias(args.propertyEditorUiAlias);

		const pasteTranslatorManifests = this.getPasteTranslatorManifests(alias);
		const propertyEditorUiManifest = await this.#findPropertyEditorUiManifest(alias);
		const config = this.#propertyContext?.getConfig();

		if (!config) {
			throw new Error('Property context is required');
		}

		const valueResolver = new UmbClipboardPastePropertyValueTranslatorValueResolver(this);

		const result = await umbOpenModal(this, UMB_CLIPBOARD_ENTRY_PICKER_MODAL, {
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
							config,
						);

						return pasteTranslator.isCompatibleValue(propertyValue, config, args.filter);
					}

					return true;
				},
			},
		});

		const selection = result?.selection || [];

		if (!selection.length) {
			throw new Error('No clipboard entry selected');
		}

		let propertyValues: Array<any>;

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
		if (!clipboardContext) {
			throw new Error('Clipboard context is required');
		}
		const entry = await clipboardContext.read(clipboardEntryUnique);

		if (!entry) {
			throw new Error(`Could not find clipboard entry with unique id: ${clipboardEntryUnique}`);
		}

		const valueResolver = new UmbClipboardPastePropertyValueTranslatorValueResolver<ValueType>(this);
		// The config of the property this context lives in. A caller translating on behalf of another property
		// editor still gets this one — which is correct for blocks, the only such caller, because a block editor
		// is its own surrounding property.
		const propertyValue = await valueResolver.resolve(
			entry.values,
			propertyEditorUiManifest.alias,
			this.#propertyContext?.getConfig(),
		);

		const cloner = new UmbPropertyValueCloneController(this);
		const clonedValue = await cloner.clone<ValueType>({
			editorAlias: propertyEditorUiManifest.meta.propertyEditorSchemaAlias,
			alias: propertyEditorUiManifest.alias,
			value: propertyValue,
		});

		return clonedValue.value;
	}

	/**
	 * Get all clipboard copy translators for a property editor ui
	 * @param {string} propertyEditorUiAlias - The alias of the property editor to match
	 * @returns {Array<ManifestClipboardCopyPropertyValueTranslator>} - Returns an array of clipboard copy translators
	 */
	getCopyTranslatorManifests(propertyEditorUiAlias: string): Array<ManifestClipboardCopyPropertyValueTranslator> {
		return umbExtensionsRegistry.getByTypeAndFilter(
			'clipboardCopyPropertyValueTranslator',
			(manifest) => manifest.fromPropertyEditorUi === propertyEditorUiAlias,
		);
	}

	/**
	 * Get all clipboard paste translators for a property editor ui
	 * @param {string} propertyEditorUiAlias - The alias of the property editor to match
	 * @returns {Array<ManifestClipboardPastePropertyValueTranslator>} - Returns an array of clipboard paste translators
	 */
	getPasteTranslatorManifests(propertyEditorUiAlias: string): Array<ManifestClipboardPastePropertyValueTranslator> {
		return umbExtensionsRegistry.getByTypeAndFilter(
			'clipboardPastePropertyValueTranslator',
			(manifest) => manifest.toPropertyEditorUi === propertyEditorUiAlias,
		);
	}

	/**
	 * The clipboard entry value types the property editor has a paste translator for. Pass these to a clipboard
	 * collection request so the data layer filters by type, rather than filtering in the UI.
	 * @param {string} [propertyEditorUiAlias] - The alias of the property editor to match. Defaults to the surrounding property editor
	 * @returns {Promise<Array<string>>} - The value types. Empty only if the property editor cannot paste anything
	 */
	async getSupportedPasteEntryValueTypes(propertyEditorUiAlias?: string): Promise<Array<string>> {
		const alias = await this.#requirePropertyEditorUiAlias(propertyEditorUiAlias);
		return this.getPasteTranslatorManifests(alias).map((manifest) => manifest.fromClipboardEntryValueType);
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

	/**
	 * Decide whether a clipboard entry can be pasted into the property, by asking the paste translator whether the
	 * translated value fits the property configuration. Hand this to a clipboard entry picker so incompatible
	 * entries render as disabled rather than being hidden.
	 * @param {UmbClipboardEntryDetailModel} entry - The clipboard entry to test
	 * @param {string} [propertyEditorUiAlias] - The alias of the property editor to match. Defaults to the surrounding property editor
	 * @returns {Promise<boolean>} - Whether the entry can be pasted into the property
	 */
	async isEntryPastable(entry: UmbClipboardEntryDetailModel, propertyEditorUiAlias?: string): Promise<boolean> {
		const alias = await this.#requirePropertyEditorUiAlias(propertyEditorUiAlias);

		// Answered rather than left to the resolver, which throws: this runs per entry while a list is built, so
		// one unsupported entry would take the whole list down.
		const pasteTranslatorManifests = this.getPasteTranslatorManifests(alias);
		if (!this.hasSupportedPasteTranslator(pasteTranslatorManifests, entry.values)) {
			return false;
		}

		const valueResolver = new UmbClipboardPastePropertyValueTranslatorValueResolver(this);
		const pasteTranslator = await valueResolver.getPasteTranslator(entry.values, alias);
		if (!pasteTranslator.isCompatibleValue) return true;

		const config = this.#propertyContext?.getConfig();
		const propertyValue = await valueResolver.resolve(entry.values, alias, config);
		return pasteTranslator.isCompatibleValue(propertyValue, config);
	}

	async #deriveEntryName(itemName?: string): Promise<string> {
		await this.#datasetInit;

		const workspaceName = this.#localize.string(this.#datasetContext?.getName());
		const propertyLabel = this.#localize.string(this.#propertyContext?.getLabel());

		return [workspaceName, propertyLabel, itemName].filter(Boolean).join(' - ');
	}

	async #requirePropertyEditorUiAlias(propertyEditorUiAlias?: string): Promise<string> {
		if (propertyEditorUiAlias) return propertyEditorUiAlias;

		// The property context can land after a caller has asked, and "not resolved yet" would otherwise be
		// indistinguishable from "no property editor".
		await this.#propertyInit;

		const alias = this.#propertyEditorUiAlias.getValue();
		if (!alias) throw new Error('Could not resolve the property editor UI alias.');

		return alias;
	}
}

export { UmbClipboardPropertyContext as api };
