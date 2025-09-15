import type { UmbInputRichMediaElement } from '../../components/input-rich-media/input-rich-media.element.js';
import type { UmbCropModel, UmbMediaPickerValueModel } from '../types.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbTreeStartNode } from '@umbraco-cms/backoffice/tree';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

import '../../components/input-rich-media/input-rich-media.element.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import {
	UMB_INTERACTION_MEMORY_CONTEXT,
	type UmbInteractionMemoryModel,
} from '@umbraco-cms/backoffice/interaction-memory';
import { simpleHashCode } from '@umbraco-cms/backoffice/observable-api';

const elementName = 'umb-property-editor-ui-media-picker';

/**
 * @element umb-property-editor-ui-media-picker
 */
@customElement(elementName)
export class UmbPropertyEditorUIMediaPickerElement
	extends UmbFormControlMixin<UmbMediaPickerValueModel | undefined, typeof UmbLitElement, undefined>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this.#setConfigHash(config);

		if (!config) return;

		this._allowedMediaTypes = config.getValueByAlias<string>('filter')?.split(',') ?? [];
		this._focalPointEnabled = Boolean(config.getValueByAlias('enableLocalFocalPoint'));
		this._multiple = Boolean(config.getValueByAlias('multiple'));
		this._preselectedCrops = config?.getValueByAlias<Array<UmbCropModel>>('crops') ?? [];

		const startNodeId = config.getValueByAlias<string>('startNodeId') ?? '';
		this._startNode = startNodeId ? { unique: startNodeId, entityType: UMB_MEDIA_ENTITY_TYPE } : undefined;

		const minMax = config.getValueByAlias<UmbNumberRangeValueType>('validationLimit');
		this._min = minMax?.min ?? 0;
		this._max = minMax?.max ?? Infinity;

		this.#getInteractionMemory();
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
	private _allowedMediaTypes: Array<string> = [];

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

	#interactionMemoryContext?: typeof UMB_INTERACTION_MEMORY_CONTEXT.TYPE;
	#configHashCode?: number;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this.observe(context?.alias, (alias) => (this._alias = alias));
			this.observe(context?.variantId, (variantId) => (this._variantId = variantId?.toString() || 'invariant'));
		});

		this.consumeContext(UMB_INTERACTION_MEMORY_CONTEXT, (context) => {
			this.#interactionMemoryContext = context;
			this.#getInteractionMemory();
		});
	}

	#setConfigHash(config: UmbPropertyEditorConfigCollection | undefined) {
		const configString = config ? JSON.stringify(config.toObject()) : '';
		const hashCode = simpleHashCode(configString);
		this.#configHashCode = hashCode;
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

	#getInteractionMemoryUnique() {
		return `UmbMediaPickerPropertyEditorUi${this.#configHashCode ? '-' + this.#configHashCode : ''}`;
	}

	#getInteractionMemory() {
		const memoryUnique = this.#getInteractionMemoryUnique();
		if (!memoryUnique) return;
		if (!this.#interactionMemoryContext) return;

		const memory = this.#interactionMemoryContext.memory.getMemory(memoryUnique);
		this._interactionMemories = memory?.memories ?? [];
	}

	#setInteractionMemory(memories: Array<UmbInteractionMemoryModel>) {
		const memoryUnique = this.#getInteractionMemoryUnique();
		if (!memoryUnique) return;
		if (!this.#interactionMemoryContext) return;

		// Set up memory for the Property Editor + Data Type context which includes all memories from the input
		const propertyEditorMemory: UmbInteractionMemoryModel = {
			unique: memoryUnique,
			memories,
		};

		this.#interactionMemoryContext.memory.setMemory(propertyEditorMemory);
	}

	#deleteInteractionMemory() {
		const unique = this.#getInteractionMemoryUnique();
		if (!unique) return;
		this.#interactionMemoryContext?.memory.deleteMemory(unique);
	}

	#onInteractionMemoriesChange(event: UmbChangeEvent) {
		const target = event.target as UmbInputRichMediaElement;
		const interactionMemories = target.interactionMemories;

		if (interactionMemories && interactionMemories.length > 0) {
			this.#setInteractionMemory(interactionMemories);
		} else {
			this.#deleteInteractionMemory();
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
				.interactionMemories=${this._interactionMemories}
				@interaction-memory-change=${this.#onInteractionMemoriesChange}>
			</umb-input-rich-media>
		`;
	}
}

export { UmbPropertyEditorUIMediaPickerElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbPropertyEditorUIMediaPickerElement;
	}
}
