import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../core/context';
import type { ManifestEntityAction } from '../../../core/models';
import { UmbActionService } from '../actions.service';

@customElement('umb-tree-action-create-page')
export default class UmbTreeActionCreatePageElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	private _actionService?: UmbActionService;

	constructor() {
		super();

		this.consumeContext('umbActionService', (actionService: UmbActionService) => {
			this._actionService = actionService;
		});
	}

	private _next() {
		this._actionService?.openPage('umb-tree-action-create-page-2');
	}

	private _back() {
		this._actionService?.closeTopPage();
	}

	render() {
		return html`<h2>Create page 1</h2>
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
