import type { UmbInputRichMediaElement } from '../../components/input-rich-media/input-rich-media.element.js';
import type { UmbCropModel, UmbMediaPickerValueModel } from '../types.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbMediaClipboardConfig } from '../../clipboard/types.js';
import {
	UMB_CLIPBOARD_PROPERTY_CONTEXT,
	type UmbClipboardCopyRequestEvent,
	type UmbClipboardPasteRequestEvent,
} from '@umbraco-cms/backoffice/clipboard';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyEditorUiInteractionMemoryManager } from '@umbraco-cms/backoffice/property-editor';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type { UmbTreeStartNode } from '@umbraco-cms/backoffice/tree';

import '../../components/input-rich-media/input-rich-media.element.js';
/**
 * @element umb-property-editor-ui-media-picker
 */
@customElement('umb-property-editor-ui-media-picker')
export class UmbPropertyEditorUIMediaPickerElement
	extends UmbFormControlMixin<UmbMediaPickerValueModel | undefined, typeof UmbLitElement, undefined>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this.#interactionMemoryManager.setPropertyEditorConfig(config);

		if (!config) return;

		this._allowedMediaTypes = config.getValueByAlias<string>('filter')?.split(',') ?? undefined;
		this._focalPointEnabled = Boolean(config.getValueByAlias('enableLocalFocalPoint'));
		this._multiple = Boolean(config.getValueByAlias('multiple'));
		this._preselectedCrops = config?.getValueByAlias<Array<UmbCropModel>>('crops') ?? [];

		const startNodeId = config.getValueByAlias<string>('startNodeId') ?? '';
		this._startNode = startNodeId ? { unique: startNodeId, entityType: UMB_MEDIA_ENTITY_TYPE } : undefined;

		const minMax = config.getValueByAlias<UmbNumberRangeValueType>('validationLimit');
		this._min = minMax?.min ?? 0;
		this._max = minMax?.max ?? Infinity;
	}

	/**
	 * Sets the input to mandatory, meaning validation will fail if the value is empty.
	 * @type {boolean}
	 */
	@property({ type: Boolean })
	mandatory?: boolean;

	@property({ type: String })
	mandatoryMessage = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	private _startNode?: UmbTreeStartNode;

	@state()
	private _focalPointEnabled: boolean = false;

	@state()
	private _preselectedCrops: Array<UmbCropModel> = [];

	@state()
	private _allowedMediaTypes?: Array<string>;

	@state()
	private _multiple: boolean = false;

	@state()
	private _min: number = 0;

	@state()
	private _max: number = Infinity;

	@state()
	private _alias?: string;

	@state()
	private _variantId?: string;

	@state()
	private _interactionMemories: Array<UmbInteractionMemoryModel> = [];

	@state()
	private _clipboardConfig?: UmbMediaClipboardConfig;

	#interactionMemoryManager = new UmbPropertyEditorUiInteractionMemoryManager(this, {
		memoryUniquePrefix: 'UmbMediaPicker',
	});

	#clipboardContext?: typeof UMB_CLIPBOARD_PROPERTY_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this.observe(context?.alias, (alias) => (this._alias = alias));
			this.observe(context?.variantId, (variantId) => (this._variantId = variantId?.toString() || 'invariant'));
		});

		this.observe(this.#interactionMemoryManager.memoriesForPropertyEditor, (interactionMemories) => {
			this._interactionMemories = interactionMemories ?? [];
		});

		// Absent for property editors that do not register the clipboard property context, hence the optional
		// handling all the way through: no context means no clipboard affordances at all.
		this.consumeContext(UMB_CLIPBOARD_PROPERTY_CONTEXT, (context) => {
			this.#clipboardContext = context;

			this.observe(
				context?.copyAvailable,
				(available) => {
					this.#clipboardCopyAvailable = available ?? false;
					this.#updateClipboardConfig();
				},
				'observeClipboardCopyAvailable',
			);

			this.observe(
				context?.pasteAvailable,
				async (available) => {
					this.#clipboardPasteTypes = available ? await context!.getSupportedPasteEntryValueTypes() : undefined;
					this.#updateClipboardConfig();
				},
				'observeClipboardPasteAvailable',
			);
		});
	}

	#clipboardCopyAvailable = false;
	#clipboardPasteTypes?: Array<string>;

	#updateClipboardConfig() {
		const clipboardContext = this.#clipboardContext;
		const types = this.#clipboardPasteTypes;

		this._clipboardConfig = clipboardContext
			? {
					copy: { enabled: this.#clipboardCopyAvailable },
					paste: {
						enabled: !!types?.length,
						types: types ?? [],
						pickableFilter: (entry) => clipboardContext.isEntryPastable(entry),
					},
				}
			: undefined;
	}

	override firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-rich-media')!);
	}

	override focus() {
		return this.shadowRoot?.querySelector<UmbInputRichMediaElement>('umb-input-rich-media')?.focus();
	}

	#onChange(event: CustomEvent & { target: UmbInputRichMediaElement }) {
		const isEmpty = event.target.value?.length === 0;
		this.value = isEmpty ? undefined : event.target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	// The copy translators consume this property editor's value, so the value for a copied item is picked out here
	// rather than in the input — the input only asks, naming the item and what it is called.
	async #onClipboardCopyRequest(event: UmbClipboardCopyRequestEvent) {
		const entry = this.value?.find((item) => item.key === event.unique);

		if (!entry) {
			throw new Error(`Could not find a media picker value for the item with key: ${event.unique}`);
		}

		await this.#clipboardContext?.write({
			propertyValue: [structuredClone(entry)],
			itemName: event.name,
			icon: event.icon,
		});
	}

	// The paste translators resolve to this property editor's value, so the merge belongs here rather than in the
	// input — the input only asks, naming the clipboard entries.
	async #onClipboardPasteRequest(event: UmbClipboardPasteRequestEvent) {
		if (!this.#clipboardContext) return;

		const propertyValues = await this.#clipboardContext.readMultiple<UmbMediaPickerValueModel>(event.entryUniques);
		const pasted = propertyValues.flat();

		const currentValue = this.value ?? [];

		// Everything in the entry is added, however many items it holds and whatever the editor allows — as when
		// picking, an over-long selection is for validation to report and the user to trim.
		const additions = pasted.filter((addition) => !currentValue.some((entry) => entry.mediaKey === addition.mediaKey));

		if (!additions.length) return;

		this.value = [...currentValue, ...additions];
		this.dispatchEvent(new UmbChangeEvent());
	}

	async #onInputInteractionMemoriesChange(event: UmbChangeEvent) {
		const target = event.target as UmbInputRichMediaElement;
		const interactionMemories = target.interactionMemories;

		if (interactionMemories && interactionMemories.length > 0) {
			await this.#interactionMemoryManager.saveMemoriesForPropertyEditor(interactionMemories);
		} else {
			await this.#interactionMemoryManager.deleteMemoriesForPropertyEditor();
		}
	}

	override render() {
		return html`
			<umb-input-rich-media
				.alias=${this._alias}
				.allowedContentTypeIds=${this._allowedMediaTypes}
				.focalPointEnabled=${this._focalPointEnabled}
				.value=${this.value ?? []}
				.max=${this._max}
				.min=${this._min}
				.preselectedCrops=${this._preselectedCrops}
				.startNode=${this._startNode}
				.variantId=${this._variantId}
				.required=${this.mandatory}
				.requiredMessage=${this.mandatoryMessage}
				?multiple=${this._multiple}
				@change=${this.#onChange}
				?readonly=${this.readonly}
				.clipboardConfig=${this._clipboardConfig}
				@clipboard-copy-request=${this.#onClipboardCopyRequest}
				@clipboard-paste-request=${this.#onClipboardPasteRequest}
				.interactionMemories=${this._interactionMemories}
				@interaction-memories-change=${this.#onInputInteractionMemoriesChange}>
			</umb-input-rich-media>
		`;
	}
}

export { UmbPropertyEditorUIMediaPickerElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-media-picker': UmbPropertyEditorUIMediaPickerElement;
	}
}
