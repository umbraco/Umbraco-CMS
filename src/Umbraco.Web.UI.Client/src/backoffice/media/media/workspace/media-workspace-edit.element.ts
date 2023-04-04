import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbMediaWorkspaceContext } from './media-workspace.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-media-workspace-edit')
export class UmbMediaWorkspaceEditElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#header {
				margin: 0 var(--uui-size-layout-1);
				flex: 1 1 auto;
			}

			#footer {
				margin: 0 var(--uui-size-layout-1);
			}
		`,
	];

	@state()
	_key?: string;

	#umbWorkspaceContext?: UmbMediaWorkspaceContext;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.#umbWorkspaceContext = instance as UmbMediaWorkspaceContext;
			this.#observeKey();
		});
	}

	#observeKey() {
		if (!this.#umbWorkspaceContext) return;
		this.observe(this.#umbWorkspaceContext.data, (data) => (this._key = data?.key));
	}

	render() {
		if (!this._key) return nothing;
		return html` <umb-workspace-layout alias="Umb.Workspace.Media">
			<div id="header" slot="header">TODO: MISSING INPUT COMPONENT</div>
			<umb-workspace-action-menu
				slot="action-menu"
				entity-type="media"
				unique="${this._key}"></umb-workspace-action-menu>
		</umb-workspace-layout>`;
	}
}

export default UmbMediaWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-workspace-edit': UmbMediaWorkspaceEditElement;
	}
}
