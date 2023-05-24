import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbMediaWorkspaceContext } from './media-workspace.context.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
@customElement('umb-media-workspace-edit')
export class UmbMediaWorkspaceEditElement extends UmbLitElement {
	@state()
	_id?: string;

	#umbWorkspaceContext?: UmbMediaWorkspaceContext;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.#umbWorkspaceContext = instance as UmbMediaWorkspaceContext;
			this.#observeId();
		});
	}

	#observeId() {
		if (!this.#umbWorkspaceContext) return;
		this.observe(this.#umbWorkspaceContext.data, (data) => (this._id = data?.id));
	}

	render() {
		if (!this._id) return nothing;
		return html` <umb-workspace-editor alias="Umb.Workspace.Media">
			<div id="header" slot="header">TODO: MISSING INPUT COMPONENT</div>
			<umb-workspace-action-menu
				slot="action-menu"
				entity-type="media"
				unique="${this._id}"></umb-workspace-action-menu>
		</umb-workspace-editor>`;
	}

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
}

export default UmbMediaWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-workspace-edit': UmbMediaWorkspaceEditElement;
	}
}
