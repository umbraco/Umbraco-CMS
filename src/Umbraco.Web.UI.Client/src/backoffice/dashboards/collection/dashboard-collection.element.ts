import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import '../../components/collection/collection.element';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbMediaStore, UmbMediaStoreItemType } from '@umbraco-cms/stores/media/media.store';
import { UmbCollectionContext } from '@umbraco-cms/components/collection/collection.context';
import type { ManifestDashboardCollection } from '@umbraco-cms/models';

@customElement('umb-dashboard-collection')
export class UmbDashboardCollectionElement extends UmbContextProviderMixin(UmbContextConsumerMixin(UmbObserverMixin(LitElement))) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				box-sizing: border-box;
				gap: var(--uui-size-space-5);
				height: 100%;
			}
		`,
	];


	private _collectionContext?:UmbCollectionContext<UmbMediaStoreItemType, UmbMediaStore>;

	public manifest!: ManifestDashboardCollection;
	
	@state()
	private _entityType?:string;


	connectedCallback(): void {
		super.connectedCallback();

		if(!this._collectionContext) {
			const manifestMeta = (this.manifest.meta as any);
			this._entityType = manifestMeta.entityType as string;
			this._collectionContext = new UmbCollectionContext(this, null, manifestMeta.storeAlias);
			this.provideContext('umbCollectionContext', this._collectionContext);
		}

		// TODO: avoid this connection, our own approach on Lit-Controller could be handling this case.
		this._collectionContext?.connectedCallback();
	}
	disconnectedCallback(): void {
		super.connectedCallback()
		// TODO: avoid this connection, our own approach on Lit-Controller could be handling this case.
		this._collectionContext?.disconnectedCallback();
	}

	

	render() {
		return html`<umb-collection entityType=${ifDefined(this._entityType)}></umb-collection>`;
	}
}

export default UmbDashboardCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-collection': UmbDashboardCollectionElement;
	}
}
