import { UmbRenameModalData, UmbRenameModalValue } from './rename-modal.token.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-rename-modal')
export class UmbRenameModalElement extends UmbModalBaseElement<UmbRenameModalData, UmbRenameModalValue> {
	render() {
		return html` <uui-dialog-layout class="uui-text"> THIS IS MY RENAME MODAL </uui-dialog-layout> `;
	}

	static styles = [UmbTextStyles];
}

export default UmbRenameModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-rename-modal': UmbRenameModalElement;
	}
}
