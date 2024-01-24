import { UMB_SAVEABLE_WORKSPACE_CONTEXT } from '../../saveable-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';

import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_MODAL_CONTEXT, UmbModalContext } from '@umbraco-cms/backoffice/modal';

/**
 * @element umb-workspace-footer
 * @description Uses the alias to insert extensions that targets this workspace-alias.
 * @slot - Slot for workspace footer items
 * @slot actions - Slot for workspace actions
 * @export
 * @class UmbWorkspaceFooterLayout
 * @extends {UmbLitElement}
 */
// TODO: stop naming this something with layout. as its not just an layout. it hooks up with extensions.
@customElement('umb-workspace-footer')
export class UmbWorkspaceFooterLayoutElement extends UmbLitElement {
	@state()
	_withinModal = false;

	@state()
	_modalContext?: UmbModalContext;

	@state()
	_isNew?: boolean;

	constructor() {
		super();
		this.consumeContext(UMB_SAVEABLE_WORKSPACE_CONTEXT, (context) => {
			this._isNew = context.getIsNew();
		});
		this.consumeContext(UMB_MODAL_CONTEXT, (context) => {
			this._modalContext = context;
		});
	}

	#rejectModal = () => {
		this._modalContext?.reject();
	};

	// TODO: Some event/callback from umb-extension-slot that can be utilized to hide the footer, if empty.
	render() {
		return html`
			<umb-footer-layout>
				<slot></slot>
				${this._modalContext
					? html`<uui-button
							slot="actions"
							label=${this._isNew ? 'Cancel' : 'Close'}
							@click=${this.#rejectModal}></uui-button>`
					: ''}
				<slot name="actions" slot="actions"></slot>
				<umb-extension-slot
					slot="actions"
					type="workspaceAction"
					default-element="umb-workspace-action"></umb-extension-slot>
				<slot name="actions" slot="actions"></slot>
			</umb-footer-layout>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
			}

			umb-extension-slot[slot='actions'] {
				display: flex;
				gap: var(--uui-size-space-2);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-footer': UmbWorkspaceFooterLayoutElement;
	}
}
