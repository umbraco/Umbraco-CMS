import { html, customElement, property, state, map } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

/**
 * @element umb-property-editor-ui-time-zone-picker
 */
@customElement('umb-property-editor-ui-time-zone-picker')
export class UmbPropertyEditorUITimeZonePickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@state()
	private _value: Array<string> = [];

	@property({ type: Array })
	public set value(value: Array<string>) {
		this._value = value || [];
	}

	public get value(): Array<string> {
		return this._value;
	}

	@state()
	private _list: Array<Option> = [];

	@state()
	private _mandatory = false;

	@property({ attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	override firstUpdated() {
		if (!this.value) return;
		this._list = Intl.supportedValuesOf('timeZone').map((option) => ({
			value: option,
			name: option.replaceAll('_', ' '),
			selected: this._value.includes(option) || this._value.some((v) => this.#getBrowserTimeZoneName(v) === option),
		}));

		// Remove UTC from the list, if present, and add it at the top
		this._list = this._list.filter((option) => option.value !== 'UTC');
		this._list.unshift({ value: 'UTC', name: 'UTC', selected: this._value.includes('UTC') });

		this._mandatory = this.config?.getValueByAlias<boolean>('mandatory') ?? false;
	}

	#getBrowserTimeZoneName(timeZone: string): string {
		return new Intl.DateTimeFormat(undefined, { timeZone: timeZone }).resolvedOptions().timeZone;
	}

	#onChange(event: Event) {
		const select = event.target as HTMLSelectElement;
		this._value = Array.from(select.selectedOptions).map((option) => option.value);
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<select multiple ?required=${this._mandatory} @change=${this.#onChange} size="15">
				${map(this._list, (item) => html`<option value=${item.value} ?selected=${item.selected}>${item.name}</option>`)}
			</select>
		`;
	}
}

export default UmbPropertyEditorUITimeZonePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-time-zone-picker': UmbPropertyEditorUITimeZonePickerElement;
	}
}
