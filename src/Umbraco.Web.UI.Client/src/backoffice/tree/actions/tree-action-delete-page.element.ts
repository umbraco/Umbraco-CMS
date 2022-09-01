import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../core/context';
import type { ManifestEntityAction } from '../../../core/models';
import { UmbActionService } from '../actions.service';

@customElement('umb-tree-action-delete-page')
export default class UmbTreeActionDeletePageElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	private _actionService?: UmbActionService;

	constructor() {
		super();

		this.consumeContext('umbActionService', (actionService: UmbActionService) => {
			this._actionService = actionService;
		});
	}

	private _save() {
		this._actionService?.close();
	}

	private _back() {
		this._actionService?.closeTopPage();
	}

	render() {
		return html`<h2>Delete page 1</h2>
			<p>
				Lorem ipsum dolor sit amet consectetur adipisicing elit. Unde voluptas veritatis voluptates ducimus ipsa.
				Assumenda, porro laboriosam, at quis, sunt architecto accusantium repellat odio esse sit ipsam et cum totam.
			</p>
			<uui-button label="Back" look="secondary" @click=${this._back}></uui-button>
			<uui-button label="Save" look="primary" color="positive" @click=${this._save}></uui-button>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-action-delete-page': UmbTreeActionDeletePageElement;
	}
}
