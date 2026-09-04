import type { UmbInputRichMediaElement } from '../../components/input-rich-media/input-rich-media.element.js';
import type { UmbCropModel, UmbMediaPickerValueModel } from '../types.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import { UmbDynamicRootResolver } from '@umbraco-cms/backoffice/content-picker';
import type { UmbContentPickerDynamicRoot } from '@umbraco-cms/backoffice/content-picker';
import { html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
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
export abstract class UmbPropertyEditorUIMediaPickerElementBase
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

		this.#dynamicRoot = config.getValueByAlias<UmbContentPickerDynamicRoot>('dynamicRoot');

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

	#dynamicRoot?: UmbContentPickerDynamicRoot;

	#dynamicRootResolver = new UmbDynamicRootResolver(this);

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

	#interactionMemoryManager = new UmbPropertyEditorUiInteractionMemoryManager(this, {
		memoryUniquePrefix: 'UmbMediaPicker',
	});

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
	}

	override async firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-rich-media')!);

		// A fixed start node wins; the dynamic root is only resolved when there is none.
		if (!this._startNode) {
			const unique = await this.#dynamicRootResolver.resolveStartNodeUnique(this.#dynamicRoot);
			if (unique) {
				this._startNode = { unique, entityType: UMB_MEDIA_ENTITY_TYPE };
			}
		}
	}

	override focus() {
		return this.shadowRoot?.querySelector<UmbInputRichMediaElement>('umb-input-rich-media')?.focus();
	}

	#onChange(event: CustomEvent & { target: UmbInputRichMediaElement }) {
		const isEmpty = event.target.value?.length === 0;
		this.value = isEmpty ? undefined : event.target.value;
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
				.interactionMemories=${this._interactionMemories}
				@interaction-memories-change=${this.#onInputInteractionMemoriesChange}>
			</umb-input-rich-media>
		`;
	}
}
