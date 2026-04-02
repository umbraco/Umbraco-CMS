import { UMB_CONTENT_COLLECTION_WORKSPACE_CONTEXT } from './content-collection-workspace.context-token.js';
import { customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbCollectionConfiguration } from '@umbraco-cms/backoffice/collection';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-content-collection-workspace-view')
export class UmbContentCollectionWorkspaceViewElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	private _loading = true;

	@state()
	private _config?: UmbCollectionConfiguration;

	@state()
	private _collectionAlias?: string;

	constructor() {
		super();

		this.consumeContext(UMB_CONTENT_COLLECTION_WORKSPACE_CONTEXT, (workspaceContext) => {
			this._collectionAlias = workspaceContext?.collection.getCollectionAlias();

			this.observe(
				workspaceContext?.collection.collectionConfig,
				(config) => {
					if (config) {
						this._config = config;
						this._loading = false;
					}
				},
				'_observeConfigContentType',
			);
		});
	}

	override render() {
		if (this._loading) return nothing;
		return html`<umb-collection .alias=${this._collectionAlias} .config=${this._config}></umb-collection>`;
	}
}

export { UmbContentCollectionWorkspaceViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-collection-workspace-view': UmbContentCollectionWorkspaceViewElement;
	}
}
