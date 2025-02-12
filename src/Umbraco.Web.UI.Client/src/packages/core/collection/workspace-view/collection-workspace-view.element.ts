import type { UmbCollectionConfiguration } from '../types.js';
import type { ManifestWorkspaceViewCollectionKind } from './types.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-collection-workspace-view')
export class UmbCollectionWorkspaceViewElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	public manifest?: ManifestWorkspaceViewCollectionKind;

	@state()
	protected _config?: UmbCollectionConfiguration;

	@state()
	protected _filter?: unknown;

	override render() {
		if (!this.manifest) return html` <div>No Manifest</div>`;
		if (!this.manifest.meta.collectionAlias) return html` <div>No Collection Alias in Manifest</div>`;
		return html`<umb-collection
			alias=${this.manifest.meta.collectionAlias}
			.config=${this._config}
			.filter=${this._filter}></umb-collection>`;
	}
}

export { UmbCollectionWorkspaceViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-workspace-view': UmbCollectionWorkspaceViewElement;
	}
}
