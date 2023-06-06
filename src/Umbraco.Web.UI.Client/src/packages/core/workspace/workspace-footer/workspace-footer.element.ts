import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { ManifestWorkspaceAction } from '@umbraco-cms/backoffice/extension-registry';

import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_MODAL_CONTEXT_TOKEN, UmbModalContext } from '@umbraco-cms/backoffice/modal';

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
	private _alias = '';

	/**
	 * Alias of the workspace. The Layout will render the workspace actions that are registered for this workspace alias.
	 * @public
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property()
	public get alias() {
		return this._alias;
	}
	public set alias(value) {
		const oldValue = this._alias;
		this._alias = value;
		if (oldValue !== this._alias) {
			this.requestUpdate('alias', oldValue);
		}
	}

	@state()
	_withinModal = false;

	#modalContext?: UmbModalContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (context) => {
			this.#modalContext = context;
		});
	}

	private _onClose = () => {
		this.#modalContext?.reject();
	};

	// TODO: Some event/callback from umb-extension-slot that can be utilized to hide the footer, if empty.
	render() {
		return html`
			<umb-footer-layout>
				<slot></slot>
				<slot name="actions" slot="actions"></slot>
				${this.#modalContext
					? html`<uui-button slot="actions" label="Close" @click=${this._onClose}></uui-button>`
					: ''}
				<umb-extension-slot
					slot="actions"
					type="workspaceAction"
					.filter=${(extension: ManifestWorkspaceAction) => extension.conditions.workspaces.includes(this.alias)}
					default-element="umb-workspace-action"></umb-extension-slot>
				<slot name="actions" slot="actions"></slot>
			</umb-footer-layout>
		`;
	}

	static styles = [
		UUITextStyles,
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
