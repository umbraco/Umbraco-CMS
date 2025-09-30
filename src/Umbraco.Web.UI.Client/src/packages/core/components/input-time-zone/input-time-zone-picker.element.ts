import { html, customElement, css, property, query, until, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIComboboxElement, UUIComboboxEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbTimeZone } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

export interface UmbTimeZoneOption extends UmbTimeZone {
	offset: string;
}

/**
 * @element umb-input-time-zone-picker
 */
@customElement('umb-input-time-zone-picker')
export class UmbInputTimeZonePickerElement extends UmbFormControlMixin<string, typeof UmbLitElement, ''>(
	UmbLitElement,
) {
	/**
	 * Disables the input
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	disabled: boolean = false;

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly: boolean = false;

	@property({ type: Array })
	public set options(value) {
		this.#options = value;
		this._filteredOptions = value;
	}
	public get options() {
		return this.#options;
	}
	#options: Array<UmbTimeZoneOption> = [];

	@state()
	private _filteredOptions: Array<UmbTimeZone> = [];

	@query('#input')
	protected _input?: UUIComboboxElement;

	/**
	 *
	 */
	constructor() {
		super();
		this._filteredOptions = this.options;
	}

	public override async focus() {
		await this.updateComplete;
		this._input?.focus();
	}

	#onSearch(event: UUIComboboxEvent) {
		const searchTerm = (event.currentTarget as UUIComboboxElement)?.search;
		this._filteredOptions = this.options.filter(
			(option) => new RegExp(searchTerm, 'i').test(option.name) || option.offset === searchTerm,
		);
	}

	#onChange(event: UUIComboboxEvent) {
		this.value = ((event.currentTarget as UUIComboboxElement)?.value as string) ?? '';
		this.dispatchEvent(new UmbChangeEvent());
	}

	#renderTimeZoneOption = (option: Option) =>
		html`<uui-combobox-list-option .value=${option.value} .displayValue=${option.name}>
			${option.name}
		</uui-combobox-list-option>`;

	override render() {
		return html`
			<uui-combobox
				id="input"
				pristine
				label="${this.localize.term('general_value')} ${this._input?.value}"
				.value=${this.value}
				@search="${this.#onSearch}"
				@change="${this.#onChange}"
				?disabled=${this.disabled}
				?readonly=${this.readonly}>
				<uui-combobox-list> ${until(repeat(this._filteredOptions, this.#renderTimeZoneOption))} </uui-combobox-list>
			</uui-combobox>
		`;
	}

	static override styles = [
		css`
			#input {
				width: 100%;
			}
		`,
	];
}

export default UmbInputTimeZonePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-time-zone-picker': UmbInputTimeZonePickerElement;
	}
}
