import { UMB_CLIPBOARD_PROPERTY_CONTEXT } from '../context/clipboard.property-context-token.js';
import { UMB_PROPERTY_CONTEXT, UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

/**
 * Property-aware facade over the clipboard property context.
 *
 * The clipboard property context requires the caller to supply the property editor UI alias (and, for copy,
 * the entry name). This manager binds to the surrounding property/dataset contexts and derives those itself,
 * so UI components (inputs, modals) can copy and read clipboard values without knowing the alias — keeping
 * them editor-agnostic. The alias is what selects the clipboard copy/paste value translators.
 * @class UmbPropertyClipboardManager
 * @augments {UmbControllerBase}
 */
export class UmbPropertyClipboardManager extends UmbControllerBase {
	#localize = new UmbLocalizationController(this);

	#clipboardContext?: typeof UMB_CLIPBOARD_PROPERTY_CONTEXT.TYPE;
	#propertyContext?: typeof UMB_PROPERTY_CONTEXT.TYPE;
	#datasetContext?: typeof UMB_PROPERTY_DATASET_CONTEXT.TYPE;

	#available = new UmbBooleanState(false);
	/**
	 * Emits whether the clipboard is available for the hosting property editor.
	 * It is absent for property editors that do not register the clipboard property context, so use it to
	 * gate clipboard affordances.
	 * @memberof UmbPropertyClipboardManager
	 */
	readonly isAvailable = this.#available.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_CLIPBOARD_PROPERTY_CONTEXT, (context) => {
			this.#clipboardContext = context;
			this.#available.setValue(!!context);
		});

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this.#propertyContext = context;
		});

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (context) => {
			this.#datasetContext = context;
		});
	}

	/**
	 * Writes a property value to the clipboard as a new entry named after its location.
	 * @param args Arguments for the write.
	 * @param {unknown} args.propertyValue The property value to write; the editor's copy translator shapes it.
	 * @param {string} [args.itemName] Optional label of the specific item, appended to the entry name.
	 * @param {string} [args.icon] Optional icon for the clipboard entry.
	 * @returns {Promise<void>}
	 * @memberof UmbPropertyClipboardManager
	 */
	async write(args: { propertyValue: unknown; itemName?: string; icon?: string }): Promise<void> {
		if (!this.#clipboardContext) throw new Error('Clipboard property context is not available.');
		if (!this.#propertyContext) throw new Error('Property context is not available.');

		const propertyEditorUiAlias = this.#propertyContext.getEditorManifest()?.alias;
		if (!propertyEditorUiAlias) throw new Error('Could not resolve the property editor UI alias to copy.');

		const workspaceName = this.#datasetContext ? this.#localize.string(this.#datasetContext.getName()) : '';
		const propertyLabel = this.#localize.string(this.#propertyContext.getLabel());
		const entryName = [workspaceName, propertyLabel, args.itemName].filter(Boolean).join(' - ');

		await this.#clipboardContext.write({
			icon: args.icon,
			name: entryName,
			propertyValue: args.propertyValue,
			propertyEditorUiAlias,
		});
	}

	/**
	 * Reads clipboard entries and translates them to the hosting editor's property value via the paste translator.
	 * @param {Array<string>} uniques The clipboard entry uniques to read.
	 * @returns {Promise<Array<ValueType>>} The translated property values, or an empty array if none could be resolved.
	 * @memberof UmbPropertyClipboardManager
	 */
	async readMultiple<ValueType = unknown>(uniques: Array<string>): Promise<Array<ValueType>> {
		if (!uniques.length) return [];
		if (!this.#clipboardContext) return [];

		const propertyEditorUiAlias = this.#propertyContext?.getEditorManifest()?.alias;
		if (!propertyEditorUiAlias) return [];

		try {
			return await this.#clipboardContext.readMultiple<ValueType>(uniques, propertyEditorUiAlias);
		} catch {
			// readMultiple throws when nothing resolves — treat as no result.
			return [];
		}
	}
}
