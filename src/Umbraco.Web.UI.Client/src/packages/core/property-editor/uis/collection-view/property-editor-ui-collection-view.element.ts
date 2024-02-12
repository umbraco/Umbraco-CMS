import type { UmbPropertyEditorConfigCollection } from '../../config/index.js';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * @element umb-property-editor-ui-collection-view
 */
@customElement('umb-property-editor-ui-collection-view')
export class UmbPropertyEditorUICollectionViewElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value?: string;

	@state()
	pageSize = 0;

	@state()
	orderDirection = 'asc';

	@state()
	useInfiniteEditor = false;

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this.pageSize = Number(config?.getValueByAlias('pageSize')) || 0;
		this.orderDirection = config?.getValueByAlias('orderDirection') ?? 'asc';
		this.useInfiniteEditor = config?.getValueByAlias('useInfiniteEditor') ?? false;
	}

	render() {
		// TODO: [LK] Figure out how to pass in the configuration to the collection view.
		return html`<umb-collection alias="Umb.Collection.Document"></umb-collection>`;
	}
}

export default UmbPropertyEditorUICollectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection-view': UmbPropertyEditorUICollectionViewElement;
	}
}
