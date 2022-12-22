import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import '../../components/collection/collection.element';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbMediaStore, UmbMediaStoreItemType } from '@umbraco-cms/stores/media/media.store';
import { UmbCollectionContext } from '@umbraco-cms/components/collection/collection.context';

@customElement('umb-dashboard-media-management')
export class UmbDashboardMediaManagementElement extends UmbContextProviderMixin(
	UmbContextConsumerMixin(UmbObserverMixin(LitElement))
) {
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

	private _entityKey!: string;
	@property()
	public get entityKey(): string {
		return this._entityKey;
	}
	public set entityKey(value: string) {
		this._entityKey = value;
		this._provideWorkspace();
	}

	

	constructor() {
		super();
		// TODO: subscribe selection.
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
		if(this._entityKey) {
			this._collectionContext = new UmbCollectionContext(this, this._entityKey, 'umbMediaStore');
			this.provideContext('umbCollectionContext', this._collectionContext);
		}
	}

	

	render() {
		return html`<umb-collection entityType="media"></umb-collection>`;
	}
}

export default UmbDashboardMediaManagementElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-media-management': UmbDashboardMediaManagementElement;
	}
}
