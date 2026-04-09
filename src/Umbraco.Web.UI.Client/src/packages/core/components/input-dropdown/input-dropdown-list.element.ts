import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-input-dropdown-list')
export class UmbInputDropdownListElement extends UmbFormControlMixin<
	string | undefined,
	typeof UmbLitElement,
	undefined
>(UmbLitElement, undefined) {
	@property({ type: Array })
	public set options(value: Array<Option> | undefined) {
		this.#options = value;

		this.value =
			value
				?.filter((option) => option.selected)
				.map((option) => option.value)
				.join(', ') ?? undefined;
	}
	public get options(): Array<Option> | undefined {
		return this.#options;
	}
	#options?: Array<Option> | undefined;

	@property({ type: String })
	public placeholder?: string;

	//TODO: show multiple lines when either a) uui-select has the option to do so or b) combobox has popover issue fixed and use this instead of uui-select.
	@property({ type: Boolean })
	public multiple?: boolean;

	@property({ type: String })
	name?: string = 'Dropdown';

	/**
	 * Sets the input to required, meaning validation will fail if the value is empty.
	 * @type {boolean}
	 */
	@property({ type: Boolean })
	required?: boolean;

	@property({ type: String })
	requiredMessage?: string;

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	constructor() {
		super();

		this.addValidator(
			'valueMissing',
			() => this.requiredMessage ?? UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
			() => !this.readonly && !!this.required && (this.value === undefined || this.value === null || this.value === ''),
		);
	}

	protected override firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('uui-select')!);
	}

	#onChange(e: UUISelectEvent) {
		e.stopPropagation();
		this.value = e.target.value?.toString() ?? undefined;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<uui-select
				label=${this.localize.term(this.localize.term('general_fieldFor', [this.name]))}
				.placeholder=${this.placeholder ?? ''}
				.options=${this.options ?? []}
				@change=${this.#onChange}
				?readonly=${this.readonly}>
			</uui-select>
		`;
	}

	static override styles = [
		css`
			:host {
				display: block;
			}
		`,
	];
}

export default UmbInputDropdownListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-dropdown-list': UmbInputDropdownListElement;
	}
}
