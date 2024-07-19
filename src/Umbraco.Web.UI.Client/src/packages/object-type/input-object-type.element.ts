import { UmbObjectTypeRepository } from './object-type.repository.js';
import { html, customElement, property, query, state } from '@umbraco-cms/backoffice/external/lit';
import type { UUISelectElement } from '@umbraco-cms/backoffice/external/uui';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-input-object-type')
export class UmbInputObjectTypeElement extends UUIFormControlMixin(UmbLitElement, '') {
	@query('uui-select')
	private select!: UUISelectElement;

	@property()
	public override set value(value: UUISelectElement['value']) {
		this.select.value = value;
	}
	public override get value(): UUISelectElement['value'] {
		return this.select.value;
	}

	@state()
	private _options: UUISelectElement['options'] = [];

	#repository: UmbObjectTypeRepository;

	constructor() {
		super();

		this.#repository = new UmbObjectTypeRepository(this);

		this.#repository.read().then(({ data }) => {
			if (!data) return;

			this._options = data.items.map((item) => ({ value: item.id, name: item.name ?? '' }));
		});
	}

	protected override getFormElement() {
		return undefined;
	}

	#onChange() {
		this.dispatchEvent(new CustomEvent('change'));
	}

	override render() {
		return html`<uui-select .options=${this._options} @change=${this.#onChange}></uui-select> `;
	}

	static override styles = [];
}

export default UmbInputObjectTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-object-type': UmbInputObjectTypeElement;
	}
}
