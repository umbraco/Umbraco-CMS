import type { UmbPropertyValueData } from '../../workspace/types/property-value-data.type.js';
import { UmbPropertyDatasetBaseContext } from './property-dataset-base-context.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

/**
 *  @element umb-property-dataset
 *  @description - Element for hosting a property dataset. This is needed for umb-property to work.
 *  @slot default - Slot for rendering content within.
 */
@customElement('umb-property-dataset')
export class UmbPropertyDatasetElement extends UmbLitElement {
	// A take on only firing events when the value is changed from the outside.
	#silentOnce = true;

	public readonly context: UmbPropertyDatasetBaseContext;

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
	 * <umb-property-dataset .value="${dataSet}">
	 * 	<umb-property
	 * 		label="My label for this property"
	 * 		description="The description to show on the property"
	 * 		alias="testAlias"
	 * 		property-editor-ui-alias="Umb.PropertyEditorUi.TextBox"
	 * 		.config=${...}>
	 * 	</umb-property>
	 * </umb-property-dataset>
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
	 * The name of the dataset, this name varies depending on the use-case. But this is either
	 * @type {string}
	 * @returns {string}
	 * @memberof UmbBasicVariantElement
	 * @example
	 * ```ts
	 * html`
	 * <umb-property-dataset name="My variant name">
	 * 	...
	 * </umb-property-dataset>
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

		this.context = new UmbPropertyDatasetBaseContext(this);
		this.observe(this.context.name, () => {
			if (!this.#silentOnce) {
				this.dispatchEvent(new UmbChangeEvent());
			} else {
				this.#silentOnce = false;
			}
		});
		this.#silentOnce = true;
		this.observe(this.context.values, () => {
			if (!this.#silentOnce) {
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

export default UmbPropertyDatasetElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-dataset': UmbPropertyDatasetElement;
	}
}
