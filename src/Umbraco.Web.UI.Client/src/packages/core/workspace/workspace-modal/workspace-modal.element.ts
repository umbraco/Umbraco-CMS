import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, CSSResultGroup, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalHandler, UmbWorkspaceData } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-workspace-modal')
export class UmbWorkspaceModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalHandler?: UmbModalHandler;

	@property()
	data?: UmbWorkspaceData;

	private _close() {
		this.modalHandler?.submit();
	}

	render() {
		return html`WORKSPACE MODAL, TBD`;
	}

	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 400px;
			}
		`,
	];
}

export default UmbWorkspaceModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-modal': UmbWorkspaceModalElement;
	}
}
