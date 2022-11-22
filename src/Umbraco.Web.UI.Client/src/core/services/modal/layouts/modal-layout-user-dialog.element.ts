import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbModalHandler } from '@umbraco-cms/services';

@customElement('umb-modal-layout-user-dialog')
export class UmbModalLayoutUserDialogElement extends LitElement {
	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			:host {
				display: block;
			}
			:host,
			umb-editor-entity-layout {
				width: 100%;
				height: 100%;
			}
			#main {
				padding: var(--uui-size-space-5);
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-3);
			}
		`,
	];

	@property({ attribute: false })
	modalHandler?: UmbModalHandler;

	private _close() {
		this.modalHandler?.close();
	}

	render() {
		return html`
			<umb-editor-entity-layout headline="USER NAME">
				<div id="main">
					<uui-box>
						<b slot="headline">Your profile</b>
						<uui-button look="primary">Edit</uui-button>
						<uui-button look="primary" color="danger">Logout</uui-button>
					</uui-box>
					<uui-box>
						<b slot="headline">External login providers</b>
						<uui-button look="primary">Edit your Umbraco ID profile</uui-button>
						<uui-button look="primary">Change your Umbraco ID password</uui-button>
					</uui-box>
				</div>
				<div slot="actions">
					<uui-button @click=${this._close} look="secondary">Close</uui-button>
				</div>
			</umb-editor-entity-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-user-dialog': UmbModalLayoutUserDialogElement;
	}
}
