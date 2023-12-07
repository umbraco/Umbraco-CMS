import type { UmbPropertyValueData } from '../types/property-value-data.type.js';
import { UmbBasicVariantContext } from './basic-variant-context.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-basic-variant')
export class UmbBasicVariantElement extends UmbLitElement {
	// A take on only firing events when the value is changed from the outside.
	#silentOnce = true;

	public readonly context: UmbBasicVariantContext;

	/**
	 * The value of the dataset.
	 * @returns {Array<UmbPropertyValueData>}
	 * @memberof UmbBasicVariantElement
	 * @example
	 * ```ts
	 * const dataSet = [
	 * 	{
	 * 		alias: 'testAlias',
	 * 		value: 'value as a string',
	 * 	},
	 *  {
	 * 		alias: 'anotherAlias',
	 * 		value: 123,
	 * 	}
	 * ]
	 *
	 * html`
	 * <umb-basic-variant .value="${dataSet}">
	 * 	<umb-workspace-property
	 * 		label="My label for this property"
	 * 		description="The description to show on the property"
	 * 		alias="testAlias"
	 * 		property-editor-ui-alias="Umb.PropertyEditorUi.TextBox"
	 * 		.config=${...}>
	 * 	</umb-workspace-property>
	 * </umb-basic-variant>
	 * `
	 * ```
	 */
	@property({ attribute: false })
	public get value(): Array<UmbPropertyValueData> {
		return this.context.getValues();
	}
	public set value(value: Array<UmbPropertyValueData>) {
		this.#silentOnce = true;
		this.context.setValues(value);
	}

	/**
	 * The name of the dataset.
	 * @returns {string}
	 * @memberof UmbBasicVariantElement
	 * @example
	 * ```ts
	 * html`
	 * <umb-basic-variant name="My variant name">
	 * 	...
	 * </umb-basic-variant>
	 * `
	 */
	@property({ attribute: false })
	public get name(): string | undefined {
		return this.context.getName();
	}
	public set name(value: string | undefined) {
		this.#silentOnce = true;
		this.context.setName(value);
	}

	constructor() {
		super();

		// Prevent any child events escaping this element.
		this.addEventListener('change', (e) => {
			if (e.target !== this) {
				e.stopImmediatePropagation();
			}
		});

		this.context = new UmbBasicVariantContext(this);
		this.observe(this.context.name, () => {
			if (!this.#silentOnce) {
				console.log('——— name fire event!');
				this.dispatchEvent(new UmbChangeEvent());
			} else {
				this.#silentOnce = false;
			}
		});
		this.#silentOnce = true;
		this.observe(this.context.values, () => {
			if (!this.#silentOnce) {
				console.log('——— value fire event!');
				this.dispatchEvent(new UmbChangeEvent());
			} else {
				this.#silentOnce = false;
			}
		});
	}

	render() {
		return html`<slot></slot>`;
	}
}

export default UmbBasicVariantElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-basic-variant': UmbBasicVariantElement;
	}
}
