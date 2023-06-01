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

	/**
	 * TODO:
	 * It seems like the router-slot cannot find the right parent router-slot as the modal element is injected via a slot. and therefor its search through parentNodes would never get to the router-slot cause its DOM existance is not within the slot, thats only the rendering.
	 * We can fix this by changing the router-slot to use an event to find its parent.
	 */
	render() {
		return html`<umb-workspace .entityType=${this.data?.entityType}></umb-workspace>`;
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
