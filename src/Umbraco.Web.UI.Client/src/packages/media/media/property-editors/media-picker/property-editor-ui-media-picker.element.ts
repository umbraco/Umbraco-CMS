import type { UmbInputRichMediaElement } from '../../components/input-rich-media/input-rich-media.element.js';

import type { UmbCropModel, UmbMediaPickerPropertyValue } from './index.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { NumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

import '../../components/input-rich-media/input-rich-media.element.js';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';

/**
 * @element umb-property-editor-ui-media-picker
 */

@customElement('umb-property-editor-ui-media-picker')
export class UmbPropertyEditorUIMediaPickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ attribute: false })
	public set value(value: Array<UmbMediaPickerPropertyValue>) {
		this.#value = value;
		this._items = this.value ? this.value.map((x) => x.mediaKey) : [];
	}
	//TODO: Add support for document specific crops. The server side already supports this.
	public get value() {
		return this.#value;
	}

	@state()
	private _startNode: string = '';

	@state()
	private _focalPointEnabled: boolean = false;

	@state()
	private _crops: Array<UmbCropModel> = [];

	@state()
	private _allowedMediaTypes: Array<string> = [];

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._multiple = Boolean(config.getValueByAlias('multiple'));
		this._startNode = config.getValueByAlias<string>('startNodeId') ?? '';
		this._focalPointEnabled = Boolean(config.getValueByAlias('enableFocalPoint'));
		this._crops = config?.getValueByAlias<Array<UmbCropModel>>('crops') ?? [];

		const filter = config.getValueByAlias<string>('filter') ?? '';
		this._allowedMediaTypes = filter?.split(',') ?? [];

		const minMax = config.getValueByAlias<NumberRangeValueType>('validationLimit');
		this._limitMin = minMax?.min ?? 0;
		this._limitMax = minMax?.max ?? Infinity;
	}
	public get config() {
		return undefined;
	}

	@state()
	private _multiple: boolean = false;

	@state()
	_items: Array<string> = [];

	@state()
	private _limitMin: number = 0;

	@state()
	private _limitMax: number = Infinity;

	#value: Array<UmbMediaPickerPropertyValue> = [];

	@state()
	private _alias?: string;

	@state()
	private _variantId?: string;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this.observe(context.alias, (alias) => (this._alias = alias));
			this.observe(context.variantId, (variantId) => (this._variantId = variantId?.toString() || 'invariant'));
		});
	}

	#onChange(event: CustomEvent & { target: UmbInputRichMediaElement }) {
		const selection = event.target.selection;

		const result = selection.map((mediaKey) => {
			return {
				key: UmbId.new(),
				mediaKey,
				mediaTypeAlias: '',
				focalPoint: null,
				crops: [],
			};
		});

		this.value = result;
		this._items = this.value ? this.value.map((x) => x.mediaKey) : [];
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`
			<umb-input-rich-media
				@change=${this.#onChange}
				?multiple=${this._multiple}
				.variantId=${this._variantId}
				.alias=${this._alias}
				.allowedContentTypeIds=${this._allowedMediaTypes}
				.startNode=${this._startNode}
				.focalPointEnabled=${this._focalPointEnabled}
				.crops=${this._crops}
				.selection=${this._items}
				.min=${this._limitMin}
				.max=${this._limitMax}>
			</umb-input-rich-media>
		`;
	}
}

export default UmbPropertyEditorUIMediaPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-media-picker': UmbPropertyEditorUIMediaPickerElement;
	}
}
