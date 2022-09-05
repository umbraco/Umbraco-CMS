import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import UmbActionElement from './action.element';

@customElement('umb-tree-action-create-page-2')
export class UmbTreeActionCreatePageElement extends UmbActionElement {
	static styles = [UUITextStyles, css``];

	private _save() {
		this._actionService?.close();
	}

	private _back() {
		this._actionPageService?.closeTopPage();
	}

	render() {
		return html`<h2>Create page 2 for entity: ${this._entity.name}</h2>
			<p>This is the last create page, here you can go back og save (it just closes the modal for now)</p>
			<uui-button label="Back" look="secondary" @click=${this._back}></uui-button>
			<uui-button label="Save" look="primary" color="positive" @click=${this._save}></uui-button>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-action-create-page-2': UmbTreeActionCreatePageElement;
	}
}
