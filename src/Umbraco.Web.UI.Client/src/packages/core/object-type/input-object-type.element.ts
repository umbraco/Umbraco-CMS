import { html, customElement, property, query, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin, UUISelectElement, UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbObjectTypeRepository } from './object-type.repository';

@customElement('umb-input-object-type')
export class UmbInputObjectTypeElement extends FormControlMixin(UmbLitElement) {
	@query('uui-select')
	private select!: UUISelectElement;

	@property()
	public set value(value: UUISelectElement['value']) {
		this.select.value = value;
	}
	public get value(): UUISelectElement['value'] {
		return this.select.value;
	}

	@state()
	private _options: UUISelectElement['options'] = [];

	#repository: UmbObjectTypeRepository;

	constructor() {
		super();

		this.#repository = new UmbObjectTypeRepository(this);

		this.#repository.read().then(({ data, error }) => {
			if (!data) return;

			this._options = data.items.map((item) => ({ value: item.id, name: item.name ?? '' }));
		});
	}

	protected getFormElement() {
		return undefined;
	}

	#onChange() {
		this.dispatchEvent(new CustomEvent('change'));
	}

	render() {
		return html`<uui-select .options=${this._options} @change=${this.#onChange}></uui-select> `;
	}

	static styles = [];
}

export default UmbInputObjectTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-object-type': UmbInputObjectTypeElement;
	}
}
