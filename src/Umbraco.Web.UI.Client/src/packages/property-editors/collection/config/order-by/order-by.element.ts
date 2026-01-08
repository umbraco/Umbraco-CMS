import type { UmbCollectionColumnConfiguration } from '@umbraco-cms/backoffice/collection';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

/**
 * @element umb-property-editor-ui-collection-order-by
 */
@customElement('umb-property-editor-ui-collection-order-by')
export class UmbPropertyEditorUICollectionOrderByElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	public value: string = '';

	public config?: UmbPropertyEditorConfigCollection;

	@state()
	private _options: Array<Option> = [];

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (instance) => {
			const workspace = instance;
			this.observe(
				await workspace.propertyValueByAlias<Array<UmbCollectionColumnConfiguration>>('includeProperties'),
				(includeProperties) => {
					if (!includeProperties) return;
					const options = includeProperties.map((property) => ({
						name: property.header,
						value: property.alias,
						selected: property.alias === this.value,
					}));
					this._options = [
						{ name: this.localize.term('general_name'), value: 'name', selected: 'name' === this.value },
						...options,
					];
				},
				'_observeIncludeProperties',
			);
		});
	}

	#onChange(e: UUISelectEvent) {
		this.value = e.target.value as string;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		if (!this._options.length) return html`<p><em>Add a column (above) to order by.</em></p>`;
		return html`<uui-select label="select" .options=${this._options} @change=${this.#onChange}></uui-select>`;
	}
}

export default UmbPropertyEditorUICollectionOrderByElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection-order-by': UmbPropertyEditorUICollectionOrderByElement;
	}
}
