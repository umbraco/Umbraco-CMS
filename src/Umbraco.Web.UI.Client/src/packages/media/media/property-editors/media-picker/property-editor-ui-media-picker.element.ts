import type { UmbInputMediaElement } from '../../components/input-media/input-media.element.js';
import '../../components/input-media/input-media.element.js';
import type { UmbMediaPickerPropertyValue } from './index.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbId } from '@umbraco-cms/backoffice/id';

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

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		const validationLimit = config?.getByAlias('validationLimit');
		if (!validationLimit) return;

		const minMax: Record<string, number> = validationLimit.value as any;

		this._limitMin = minMax.min ?? 0;
		this._limitMax = minMax.max ?? Infinity;
	}
	public get config() {
		return undefined;
	}

	@state()
	_items: Array<string> = [];

	@state()
	private _limitMin: number = 0;
	@state()
	private _limitMax: number = Infinity;

	#value: Array<UmbMediaPickerPropertyValue> = [];

	#onChange(event: CustomEvent) {
		const selection = (event.target as UmbInputMediaElement).selection;

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
			<umb-input-media
				@change=${this.#onChange}
				.selection=${this._items}
				.min=${this._limitMin}
				.max=${this._limitMax}>
				<umb-localize key="general_add">Add</umb-localize>
			</umb-input-media>
		`;
	}
}

export default UmbPropertyEditorUIMediaPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-media-picker': UmbPropertyEditorUIMediaPickerElement;
	}
}
