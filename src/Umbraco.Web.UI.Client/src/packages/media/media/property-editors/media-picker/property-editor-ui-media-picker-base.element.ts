import type { UmbInputRichMediaElement } from '../../components/input-rich-media/input-rich-media.element.js';
import type { UmbCropModel, UmbMediaPickerValueModel } from '../types.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import { html, property, state } from '@umbraco-cms/backoffice/external/lit';
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
 * There is one media picker property editor UI per number of items a picker holds, so that the type a media picker
 * property yields follows from the editor rather than from the configuration of the data type it is used through.
 * Both are edited the same way, which is what this base holds.
 */
export abstract class UmbMediaPickerPropertyEditorUIElementBase
	extends UmbFormControlMixin<UmbMediaPickerValueModel | undefined, typeof UmbLitElement, undefined>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	/**
	 * Whether the editor holds more than one media item.
	 */
	protected abstract readonly multiple: boolean;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this.#interactionMemoryManager.setPropertyEditorConfig(config);

		if (!config) return;

		this._allowedMediaTypes = config.getValueByAlias<string>('filter')?.split(',') ?? undefined;
		this._focalPointEnabled = Boolean(config.getValueByAlias('enableLocalFocalPoint'));
		this._preselectedCrops = config?.getValueByAlias<Array<UmbCropModel>>('crops') ?? [];

		const startNodeId = config.getValueByAlias<string>('startNodeId') ?? '';
		this._startNode = startNodeId ? { unique: startNodeId, entityType: UMB_MEDIA_ENTITY_TYPE } : undefined;

		const minMax = config.getValueByAlias<UmbNumberRangeValueType>('validationLimit');
		this._min = minMax?.min ?? 0;
		this._max = this.multiple ? (minMax?.max ?? Infinity) : 1;
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

	#clipboardCopyAvailable = false;
	#clipboardPasteTypes?: Array<string>;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this.observe(context?.alias, (alias) => (this._alias = alias));
			this.observe(context?.variantId, (variantId) => (this._variantId = variantId?.toString() || 'invariant'));
		});

		this.observe(
			this.#interactionMemoryManager.memoriesForPropertyEditor,
			(interactionMemories) => {
				this._interactionMemories = interactionMemories ?? [];
			},
			null,
		);

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

	async #onClipboardPasteRequest(event: UmbClipboardPasteRequestEvent) {
		if (!this.#clipboardContext) return;

		const propertyValues = await this.#clipboardContext.readMultiple<UmbMediaPickerValueModel>(event.entryUniques);
		const pasted = propertyValues.flat();

		if (!this.multiple) {
			// An editor holding one item takes the first pasted item in place of its value.
			const [first] = pasted;
			if (!first) return;
			this.value = [first];
			this.dispatchEvent(new UmbChangeEvent());
			return;
		}

		const currentValue = this.value ?? [];

		// Not clamped to the configured limit: as when picking, an over-long selection is for validation to
		// report and the user to trim.
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
				?multiple=${this.multiple}
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
