import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, CSSResultGroup, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalHandler, UmbWorkspaceData, UmbWorkspaceResult } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-workspace-modal')
export class UmbWorkspaceModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalHandler?: UmbModalHandler;

	@property()
	data?: UmbWorkspaceData;

	private _reject = (event: CustomEvent) => {
		event.stopPropagation();
		this.modalHandler?.reject();
	};

	private _submit = (event: CustomEvent<UmbWorkspaceResult>) => {
		event.stopPropagation();
		this.modalHandler?.submit(event.detail);
	};

	/**
	 * TODO: Consider if this binding and events integration is the right for communicating back the modal handler. Or if we should go with some Context API. like a Modal Context API.
	 *
	 */
	render() {
		return html`<umb-workspace
			.entityType=${this.data?.entityType}
			.preset=${this.data?.preset}
			@workspace-submit=${this._submit}
			@workspace-reject=${this._reject}></umb-workspace>`;
	}

	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			:host {
				display: block;
				height: 100%;
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
