import type { UmbPropertyValueData } from '../types/property-value-data.type.js';
import { UmbPropertyDatasetContextBase } from './property-dataset-base-context.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

/**
 *  @element umb-property-dataset
 *  @description - Element for hosting a property dataset. This is needed for umb-property to work.
 *  @slot default - Slot for rendering content within.
 */
@customElement('umb-property-dataset')
export class UmbPropertyDatasetElement extends UmbLitElement {
	// Determine wether state change should fire an event when the value is changed.
	#allowChangeEvent = false;

	public readonly context: UmbPropertyDatasetContextBase;

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
		this.#allowChangeEvent = false;
		this.context.setValues(value);
		// Above might not trigger a observer callback (if no change), so set the allow change event to true:
		this.#allowChangeEvent = true;
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
		this.#allowChangeEvent = false;
		this.context.setName(value);
		// Above might not trigger a observer callback (if no change), so set the allow change event to true:
		this.#allowChangeEvent = true;
	}

	constructor() {
		super();

		// Prevent any child events escaping this element.
		this.addEventListener('change', (e) => {
			if (e.target !== this) {
				e.stopImmediatePropagation();
			}
		});

		this.context = new UmbPropertyDatasetContextBase(this);
		// prevent the first change event from firing:
		this.#allowChangeEvent = false;
		this.observe(this.context.name, this.#observerCallback);
		// prevent the first change event from firing:
		this.#allowChangeEvent = false;
		this.observe(this.context.values, this.#observerCallback);
	}

	#observerCallback = () => {
		if (this.#allowChangeEvent) {
			this.dispatchEvent(new UmbChangeEvent());
		} else {
			// Set allow change event to true.
			this.#allowChangeEvent = true;
		}
	};

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
