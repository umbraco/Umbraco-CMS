import { UMB_COLLECTION_CONTEXT, UmbCollectionDefaultContext } from '../collection-default.context.js';
import { html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-collection-action-bundle')
export class UmbCollectionActionBundleElement extends UmbLitElement {
	#collectionContext?: UmbCollectionDefaultContext<any, any>;

	@state()
	_collectionAlias? = '';

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.#collectionContext = context;
			if (!this.#collectionContext) return;
			this._collectionAlias = this.#collectionContext.getAlias();
		});
	}

	render() {
		return html`
			${this._collectionAlias ? html`<umb-extension-slot type="collectionAction"></umb-extension-slot>` : nothing}
		`;
	}

	static styles = [UmbTextStyles];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-action-bundle': UmbCollectionActionBundleElement;
	}
}
