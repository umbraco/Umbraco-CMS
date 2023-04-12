import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import type { ManifestWorkspaceAction } from '@umbraco-cms/backoffice/extensions-registry';

import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-workspace-footer-layout
 * @description Uses the alias to insert extensions that targets this workspace-alias.
 * @slot - Slot for workspace footer items
 * @slot actions - Slot for workspace actions
 * @export
 * @class UmbWorkspaceFooterLayout
 * @extends {UmbLitElement}
 */
// TODO: stop naming this something with layout. as its not just an layout. it hooks up with extensions.
@customElement('umb-workspace-footer-layout')
export class UmbWorkspaceFooterLayoutElement extends UmbLitElement {
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

	// TODO: Some event/callback from umb-extension-slot that can be utilized to hide the footer, if empty.
	render() {
		return html`
			<umb-footer-layout>
				<slot></slot>
				<slot name="actions" slot="actions"></slot>
				<umb-extension-slot
					slot="actions"
					type="workspaceAction"
					.filter=${(extension: ManifestWorkspaceAction) => extension.conditions.workspaces.includes(this.alias)}
					default-element="umb-workspace-action"></umb-extension-slot>
				<slot name="actions" slot="actions"></slot>
			</umb-footer-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-footer-layout': UmbWorkspaceFooterLayoutElement;
	}
}
