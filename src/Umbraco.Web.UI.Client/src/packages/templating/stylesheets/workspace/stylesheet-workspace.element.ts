import { serverFilePathFromUrlFriendlyPath } from '../../utils.js';
import { UmbStylesheetWorkspaceEditElement } from './stylesheet-workspace-edit.element.js';
import { UmbStylesheetWorkspaceContext } from './stylesheet-workspace.context.js';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-stylesheet-workspace')
export class UmbStylesheetWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbStylesheetWorkspaceContext(this);
	#element = new UmbStylesheetWorkspaceEditElement();

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'edit/:path',
			component: () => this.#element,
			setup: (_component, info) => {
				const path = info.match.params.path;
				const serverPath = serverFilePathFromUrlFriendlyPath(path);
				this.#workspaceContext.load(serverPath);

				new UmbWorkspaceIsNewRedirectController(
					this,
					this.#workspaceContext,
					this.shadowRoot!.querySelector('umb-router-slot')!
				);
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
