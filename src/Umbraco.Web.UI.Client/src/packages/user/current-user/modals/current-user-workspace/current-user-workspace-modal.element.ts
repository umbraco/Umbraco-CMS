import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';

@customElement('umb-current-user-workspace-modal')
export class UmbCurrentUserWorkspaceModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext;

	private _close() {
		this.modalContext?.reject();
	}

	private _save() {
		this.modalContext?.submit();
	}

	override render() {
		return html`
			<umb-body-layout headline="Edit Profile">
				<div slot="actions">
					<uui-button @click=${this._close} look="secondary" .label=${this.localize.term('general_close')}>
						${this.localize.term('general_close')}
					</uui-button>
					<uui-button @click=${this._save} look="primary" .label=${this.localize.term('general_save')}>
						${this.localize.term('general_save')}
					</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];
}

export default UmbCurrentUserWorkspaceModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-workspace-modal': UmbCurrentUserWorkspaceModalElement;
	}
}
