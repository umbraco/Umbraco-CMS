import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbContextProviderMixin } from '../../../../core/context';

import { ActionPageEntity } from '../../actions/action.element';
import '../../actions';

@customElement('umb-tree-context-menu-service')
export class UmbTreeContextMenuService extends UmbContextProviderMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
				z-index: 1;
			}
			#backdrop {
				content: '';
				position: absolute;
				inset: 0px;
				background-color: black;
				opacity: 0.5;
				width: 100vw;
				height: 100vh;
				z-index: -1;
			}
			#relative-wrapper {
				background-color: var(--uui-color-surface);
				position: relative;
				display: flex;
				flex-direction: column;
				width: 100%;
				height: 100%;
			}
			#action-modal {
				position: absolute;
				left: 300px;
				height: 100%;
				z-index: 1;
				top: 0;
				width: 300px;
				border: none;
				border-left: 1px solid var(--uui-color-border);
				border-right: 1px solid var(--uui-color-border);
				background-color: var(--uui-color-surface);
			}
		`,
	];

	@state()
	private _modalOpen = false;

	@state()
	private entity: { name: string; key: string } = { name: '', key: '' };

	connectedCallback() {
		super.connectedCallback();
		this.provideContext('umbTreeContextMenuService', this);
	}

	public open(entity: ActionPageEntity) {
		this.entity = entity;
		this._modalOpen = true;
	}

	public close() {
		this._modalOpen = false;
	}

	private _renderBackdrop() {
		// eslint-disable-next-line lit-a11y/click-events-have-key-events
		return this._modalOpen ? html`<div id="backdrop" @click=${this.close}></div>` : nothing;
	}

	private _renderModal() {
		return this._modalOpen
			? html`<umb-action-page-service id="action-modal" .actionEntity=${this.entity}></umb-action-page-service>`
			: nothing;
	}

	render() {
		return html`
			${this._renderBackdrop()}
			<div id="relative-wrapper">
				<slot></slot>
				${this._renderModal()}
			</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-context-menu-service': UmbTreeContextMenuService;
	}
}
