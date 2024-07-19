import { css, html, customElement, property, query } from '@umbraco-cms/backoffice/external/lit';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-input-dropdown-list')
export class UmbInputDropdownListElement extends UUIFormControlMixin(UmbLitElement, undefined) {
	@property({ type: Array })
	public options?: Array<Option>;

	@property({ type: String })
	public placeholder?: string;

	//TODO: show multiple lines when either a) uui-select has the option to do so or b) combobox has popover issue fixed and use this instead of uui-select.
	@property({ type: Boolean })
	public multiple?: boolean;

	@query('uui-select')
	private selectEle!: HTMLInputElement;

	protected override getFormElement() {
		return this.selectEle;
	}

	#onChange(e: UUISelectEvent) {
		e.stopPropagation();
		if (e.target.value) this.value = e.target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`<uui-select
			label=${this.localize.term('formProviderFieldTypes_dropdownName')}
			.placeholder=${this.placeholder ?? ''}
			.options=${this.options ?? []}
			@change=${this.#onChange}></uui-select>`;
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
