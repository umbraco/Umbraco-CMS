import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../../../../core/context';
import { UmbUserStore } from '../../../../../../core/stores/user/user.store';
import { Subscription } from 'rxjs';
import type { UserEntity } from '../../../../../../core/models';

@customElement('umb-editor-action-users-save')
export class UmbEditorActionUsersSaveElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	private _userStore?: UmbUserStore;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext('umbUserStore', (userStore: UmbUserStore) => {
			this._userStore = userStore;
		});
	}

	private _handleSave() {
		// this._userStore.save()
		console.log('save');
	}

	render() {
		return html`<uui-button @click=${this._handleSave} look="primary" color="positive" label="save"></uui-button>`;
	}
}

export default UmbEditorActionUsersSaveElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-action-users-save': UmbEditorActionUsersSaveElement;
	}
}
