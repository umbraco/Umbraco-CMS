import { UMB_COLLECTION_CONTEXT } from '../default/collection-default.context-token.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-collection-action-bundle')
export class UmbCollectionActionBundleElement extends UmbLitElement {
	@state()
	private _hideCollectionActions = false;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.observe(
				context?.hideCollectionActions,
				(hideCollectionActions) => (this._hideCollectionActions = hideCollectionActions ?? false),
				'umbCollectionHideCollectionActionsObserver',
			);
		});
	}

	override render() {
		if (this._hideCollectionActions) return nothing;
		return html`<umb-extension-with-api-slot type="collectionAction"></umb-extension-with-api-slot>`;
	}

	static override readonly styles = [
		css`
			:host {
				display: contents;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-action-bundle': UmbCollectionActionBundleElement;
	}
}
