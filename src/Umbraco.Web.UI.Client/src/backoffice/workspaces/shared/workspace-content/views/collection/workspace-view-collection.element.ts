import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import type { UmbWorkspaceNodeContext } from '../../../workspace-context/workspace-node.context';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';

import 'src/backoffice/components/content-property/content-property.element';
import 'src/backoffice/dashboards/collection/dashboard-collection.element';
import { UmbCollectionContext } from '@umbraco-cms/components/collection/collection.context';
import { UmbMediaStore, UmbMediaStoreItemType } from '@umbraco-cms/stores/media/media.store';

@customElement('umb-workspace-view-collection')
export class UmbWorkspaceViewCollectionElement extends UmbContextProviderMixin(UmbContextConsumerMixin(UmbObserverMixin(LitElement))) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}
		`,
	];

	private _workspaceContext?: UmbWorkspaceNodeContext;

	private _collectionContext?:UmbCollectionContext<UmbMediaStoreItemType, UmbMediaStore>;
	

	constructor() {
		super();

		this.consumeContext('umbWorkspaceContext', (nodeContext) => {
			this._workspaceContext = nodeContext;
			this._provideWorkspace();
		});
	}

	connectedCallback(): void {
		super.connectedCallback();
		// TODO: avoid this connection, our own approach on Lit-Controller could be handling this case.
		this._collectionContext?.connectedCallback();
	}
	disconnectedCallback(): void {
		super.connectedCallback()
		// TODO: avoid this connection, our own approach on Lit-Controller could be handling this case.
		this._collectionContext?.disconnectedCallback();
	}

	protected _provideWorkspace() {
		if(this._workspaceContext?.entityKey != null) {
			this._collectionContext = new UmbCollectionContext(this, this._workspaceContext.entityKey, this._workspaceContext.getStore().storeAlias);
			this._collectionContext.connectedCallback();
			this.provideContext('umbCollectionContext', this._collectionContext);
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
