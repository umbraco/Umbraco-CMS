import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import UmbTreeItemActionElement from '../../shared/tree-item-action.element';

@customElement('umb-tree-action-create-page')
export class UmbTreeActionCreatePageElement extends UmbTreeItemActionElement {
	static styles = [UUITextStyles, css``];

	private _next() {
		this._actionPageService?.openPage('umb-tree-action-create-page-2');
	}

	private _back() {
		this._actionPageService?.closeTopPage();
	}

	render() {
		return html`<h2>Create page 1 for entity: ${this._entity.name}</h2>
			<p>This is the first create page, here you can go next or back (it just closes the modal for now)</p>
			<uui-button label="Back" look="secondary" @click=${this._back}></uui-button>
			<uui-button label="Next" look="primary" @click=${this._next}></uui-button>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-action-create-page': UmbTreeActionCreatePageElement;
	}
}
