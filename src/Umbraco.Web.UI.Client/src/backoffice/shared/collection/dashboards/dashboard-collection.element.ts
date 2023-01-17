import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import '../collection.element';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UmbMediaStore, UmbMediaStoreItemType } from 'src/backoffice/media/media/media.store';
import {
	UmbCollectionContext,
	UMB_COLLECTION_CONTEXT_ALIAS,
} from 'src/backoffice/shared/collection/collection.context';
import type { ManifestDashboardCollection } from '@umbraco-cms/models';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-dashboard-collection')
export class UmbDashboardCollectionElement extends UmbLitElement {
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

	private _collectionContext?: UmbCollectionContext<UmbMediaStoreItemType, UmbMediaStore>;

	public manifest!: ManifestDashboardCollection;

	@state()
	private _entityType?: string;

	connectedCallback(): void {
		super.connectedCallback();

		if (!this._collectionContext) {
			const manifestMeta = this.manifest.meta as any;
			this._entityType = manifestMeta.entityType as string;
			this._collectionContext = new UmbCollectionContext(this, null, manifestMeta.storeAlias);
			this.provideContext(UMB_COLLECTION_CONTEXT_ALIAS, this._collectionContext);
		}
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
