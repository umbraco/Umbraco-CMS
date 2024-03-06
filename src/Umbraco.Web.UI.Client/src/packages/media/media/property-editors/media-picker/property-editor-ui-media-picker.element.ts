import type { UmbInputMediaElement } from '../../components/input-media/input-media.element.js';
import '../../components/input-media/input-media.element.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * @element umb-property-editor-ui-media-picker
 */
@customElement('umb-property-editor-ui-media-picker')
export class UmbPropertyEditorUIMediaPickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	public value: string = '';

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		const validationLimit = config?.getByAlias('validationLimit');
		if (!validationLimit) return;

		const minMax: Record<string, number> = validationLimit.value;

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

	protected updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);
		if (_changedProperties.has('value')) {
			if (typeof this.value !== 'string') {
				//TODO: Temp fix for when the value is an array, this should be fixed elsewhere.
				this.value = '';
			}
			this._items = this.value ? this.value.split(',') : [];
		}
	}

	private _onChange(event: CustomEvent) {
		const selectedIds = (event.target as UmbInputMediaElement).selectedIds;
		//TODO: This is a hack, something changed so now we need to convert the array to a comma separated string to make it work with the server.
		const toCommaSeparatedString = selectedIds.join(',');
		// this.value = (event.target as UmbInputMediaElement).selectedIds;

		this.value = toCommaSeparatedString;
		console.log('property-value-change', this.value);
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`
			<umb-input-media
				@change=${this._onChange}
				.selectedIds=${this._items}
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
