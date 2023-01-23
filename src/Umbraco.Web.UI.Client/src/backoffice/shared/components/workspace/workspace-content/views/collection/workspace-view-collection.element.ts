import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UmbWorkspaceContentContext } from '../../workspace-content.context';
import {
	UmbCollectionContext,
	UMB_COLLECTION_CONTEXT_TOKEN,
} from 'src/backoffice/shared/collection/collection.context';
import { UmbMediaStore, UmbMediaStoreItemType } from 'src/backoffice/media/media/media.store';

import '../../../../../../shared/components/content-property/content-property.element';
import '../../../../../../shared/collection/dashboards/dashboard-collection.element';
import { UmbLitElement } from '@umbraco-cms/context-api';

@customElement('umb-workspace-view-collection')
export class UmbWorkspaceViewCollectionElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}
		`,
	];

	private _workspaceContext?: UmbWorkspaceContentContext;

	private _collectionContext?: UmbCollectionContext<UmbMediaStoreItemType, UmbMediaStore>;

	constructor() {
		super();

		// TODO: Figure out how to get the magic string for the workspace context.
		this.consumeContext<UmbWorkspaceContentContext>('umbWorkspaceContext', (nodeContext) => {
			this._workspaceContext = nodeContext;
			this._provideWorkspace();
		});
	}

	protected _provideWorkspace() {
		if (this._workspaceContext?.entityKey != null) {
			this._collectionContext = new UmbCollectionContext(
				this,
				this._workspaceContext.entityKey,
				this._workspaceContext.getStore()?.storeAlias || '' // The store is available when the context is available.
			);
			this.provideContext(UMB_COLLECTION_CONTEXT_TOKEN, this._collectionContext);
		}
	}

	render() {
		return html`<umb-collection entityType=${ifDefined(this._workspaceContext?.entityType)}></umb-collection>`;
	}
}

export default UmbWorkspaceViewCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-collection': UmbWorkspaceViewCollectionElement;
	}
}
