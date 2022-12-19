import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';

@customElement('umb-collection-selection-actions')
export class UmbCollectionSelectionActionsElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				gap: var(--uui-size-3);
				width: 100%;
				padding: var(--uui-size-space-4) var(--uui-size-space-6);
				background-color: var(--uui-color-selected);
				color: var(--uui-color-selected-contrast);
				align-items: center;
				box-sizing: border-box;
			}
		`,
	];

	@property()
	public entityType = 'media';

	@state()
	private _selection: Array<string> = [];

	private _renderSelectionCount() {
		return html`<div>${this._selection.length} of ${4} selected</div>`;
	}

	render() {
		return html`<uui-button label="Clear" look="secondary"></uui-button>
			${this._renderSelectionCount()}
			<umb-extension-slot
				type="collectionBulkAction"
				.filter=${(manifest: any) => manifest.meta.entityType === this.entityType}></umb-extension-slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-selection-actions': UmbCollectionSelectionActionsElement;
	}
}
