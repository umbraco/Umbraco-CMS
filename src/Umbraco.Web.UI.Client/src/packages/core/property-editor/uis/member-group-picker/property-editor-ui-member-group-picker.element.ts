import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbInputMemberGroupElement } from '@umbraco-cms/backoffice/member-group';

/**
 * @element umb-property-editor-ui-member-group-group-picker
 */
@customElement('umb-property-editor-ui-member-group-group-picker')
export class UmbPropertyEditorUIMemberGroupPickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	// private _value: Array<string> = [];

	// @property({ type: Array })
	// public get value(): Array<string> {
	// 	return this._value;
	// }
	// public set value(value: Array<string>) {
	// 	this._value = Array.isArray(value) ? value : value ? [value] : [];
	// }

	@property({ type: String })
	public value: string = '';

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		const validationLimit = config?.find((x) => x.alias === 'validationLimit');

		this._limitMin = (validationLimit?.value as any)?.min;
		this._limitMax = (validationLimit?.value as any)?.max;
	}

	@state()
	_items: Array<string> = [];

	@state()
	private _limitMin?: number;
	@state()
	private _limitMax?: number;

	protected updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);
		if (_changedProperties.has('value')) {
			this._items = this.value ? this.value.split(',') : [];
		}
	}

	private _onChange(event: CustomEvent) {
		//TODO: This is a hack, something changed so now we need to convert the array to a comma separated string to make it work with the server.
		const toCommaSeparatedString = (event.target as UmbInputMemberGroupElement).selectedIds.join(',');
		// this.value = (event.target as UmbInputMemberGroupElement).selectedIds;
		this.value = toCommaSeparatedString;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`
			<umb-input-member-group
				@change=${this._onChange}
				.selectedIds=${this._items}
				.min=${this._limitMin ?? 0}
				.max=${this._limitMax ?? Infinity}
				>Add</umb-input-member-group
			>
		`;
	}
}

export default UmbPropertyEditorUIMemberGroupPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-member-group-group-picker': UmbPropertyEditorUIMemberGroupPickerElement;
	}
}
