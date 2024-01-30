import { UMB_MEDIA_WORKSPACE_CONTEXT } from './media-workspace.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
@customElement('umb-media-workspace-editor')
export class UmbMediaWorkspaceEditorElement extends UmbLitElement {
	@state()
	_unique?: string;

	#umbWorkspaceContext?: typeof UMB_MEDIA_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (instance) => {
			this.#umbWorkspaceContext = instance;
			this.#observeUnique();
		});
	}

	#observeUnique() {
		if (!this.#umbWorkspaceContext) return;
		this.observe(this.#umbWorkspaceContext.data, (data) => (this._unique = data?.unique));
	}

	render() {
		if (!this._unique) return nothing;
		return html` <umb-workspace-editor alias="Umb.Workspace.Media">
			<div id="header" slot="header">TODO: MISSING INPUT COMPONENT</div>
			<umb-workspace-action-menu
				slot="action-menu"
				entity-type="media"
				unique="${this._unique}"></umb-workspace-action-menu>
		</umb-workspace-editor>`;
	}

	static styles = [
		UmbTextStyles,
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

export default UmbMediaWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-workspace-editor': UmbMediaWorkspaceEditorElement;
	}
}
