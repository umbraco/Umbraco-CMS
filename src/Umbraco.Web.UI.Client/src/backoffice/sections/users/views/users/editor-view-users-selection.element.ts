import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../../../../../core/context';
import UmbSectionViewUsersElement, { UserItem } from './section-view-users.element';
import { UmbUserStore } from '../../../../../core/stores/user/user.store';

@customElement('umb-editor-view-users-selection')
export class UmbEditorViewUsersSelectionElement extends UmbContextConsumerMixin(LitElement) {
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

	@state()
	private _selection: Array<string> = [];

	private _usersContext?: UmbSectionViewUsersElement;
	private _selectionSubscription?: Subscription;

	connectedCallback(): void {
		super.connectedCallback();
		this.consumeContext('umbUsersContext', (usersContext: UmbSectionViewUsersElement) => {
			this._usersContext = usersContext;
			this._observeSelection();
		});
	}

	private _observeSelection() {
		this._selectionSubscription?.unsubscribe();
		this._selectionSubscription = this._usersContext?.selection.subscribe((selection: Array<string>) => {
			this._selection = selection;
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._selectionSubscription?.unsubscribe();
	}

	private _handleClearSelection() {
		this._usersContext?.setSelection([]);
	}

	private _renderSelectionCount() {
		return html`<div>${this._selection.length} of [??] selected</div>`;
	}

	render() {
		return html`<uui-button @click=${this._handleClearSelection} label="Clear selection" look="secondary"></uui-button>
			${this._renderSelectionCount()}
			<uui-button style="margin-left: auto" label="Set group" look="secondary"></uui-button>
			<uui-button label="Enable" look="secondary"></uui-button>
			<uui-button label="Unlock" disabled look="secondary"></uui-button>
			<uui-button label="Disable" look="secondary"></uui-button> `;
	}
}

export default UmbEditorViewUsersSelectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-users-selection': UmbEditorViewUsersSelectionElement;
	}
}
