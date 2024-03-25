import { UmbFormContext } from '../context/form.context.js';
import { type PropertyValueMap, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-form')
export class UmbFormElement extends UmbLitElement {
	readonly #context = new UmbFormContext(this);

	protected firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);

		this.#context.setFormElement(this.shadowRoot!.querySelector<HTMLFormElement>('form'));
	}

	render() {
		return html`<uui-form>
			<form>
				<slot></slot>
			</form>
		</uui-form>`;
	}
}
