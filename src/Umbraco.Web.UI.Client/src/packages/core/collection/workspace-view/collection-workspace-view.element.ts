import type { ManifestWorkspaceViewCollectionKind } from './types.js';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-collection-workspace-view';
@customElement(elementName)
export class UmbCollectionWorkspaceViewElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	manifest?: ManifestWorkspaceViewCollectionKind;

	override render() {
		if (!this.manifest) return html` <div>No Manifest</div>`;
		if (!this.manifest.meta.collectionAlias) return html` <div>No Collection Alias in Manifest</div>`;
		return html`<umb-collection alias=${this.manifest.meta.collectionAlias}></umb-collection>`;
	}
}

export { UmbCollectionWorkspaceViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbCollectionWorkspaceViewElement;
	}
}
