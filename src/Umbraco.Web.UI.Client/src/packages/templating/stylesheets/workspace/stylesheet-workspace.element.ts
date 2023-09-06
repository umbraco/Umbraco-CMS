import { serverFilePathFromUrlFriendlyPath } from '../../utils.js';
import { UmbStylesheetWorkspaceEditorElement } from './stylesheet-workspace-editor.element.js';
import { UmbStylesheetWorkspaceContext } from './stylesheet-workspace.context.js';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-stylesheet-workspace')
export class UmbStylesheetWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbStylesheetWorkspaceContext(this);
	#element = new UmbStylesheetWorkspaceEditorElement();

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create/:path',
			component: import('./stylesheet-workspace-editor.element.js'),
			setup: async (_component, info) => {
				const path = info.match.params.path === 'null' ? null : info.match.params.path;
				const serverPath = path === null ? null : serverFilePathFromUrlFriendlyPath(path);
				await this.#workspaceContext.create(serverPath);

				new UmbWorkspaceIsNewRedirectController(
					this,
					this.#workspaceContext,
					this.shadowRoot!.querySelector('umb-router-slot')!,
				);
			},
		},
		{
			path: 'edit/:path',
			component: import('./stylesheet-workspace-editor.element.js'),
			setup: (_component, info) => {
				this.removeControllerByAlias('_observeIsNew');
				const path = info.match.params.path;
				const serverPath = serverFilePathFromUrlFriendlyPath(path);
				this.#workspaceContext.load(serverPath);
			},
		},
	];

	render() {
		return html` <umb-router-slot .routes=${this._routes}></umb-router-slot> `;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];
}

export default UmbStylesheetWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-workspace': UmbStylesheetWorkspaceElement;
	}
}
