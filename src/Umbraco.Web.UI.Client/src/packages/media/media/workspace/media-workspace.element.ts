import { type UmbMediaWorkspaceContext } from './media-workspace.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { type UmbApi, createExtensionApi, UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry, type ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';
import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-media-workspace')
export class UmbMediaWorkspaceElement extends UmbLitElement {
	#workspaceContext?: UmbMediaWorkspaceContext;

	@state()
	_routes: UmbRoute[] = [];

	public set manifest(manifest: ManifestWorkspace) {
		createExtensionApi(manifest, [this]).then((context) => {
			if (context) {
				this.#gotWorkspaceContext(context);
			}
		});
	}

	#gotWorkspaceContext(context: UmbApi) {
		this.#workspaceContext = context as UmbMediaWorkspaceContext;

		this._routes = [
			{
				path: 'create/:parentId', //  /:mediaTypeKey
				component: import('./media-workspace-editor.element.js'),
				setup: async (_component, info) => {
					// TODO: Remember the perspective of permissions here, we need to check if the user has access to create a document of this type under this parent?
					const parentId = info.match.params.parentId === 'null' ? null : info.match.params.parentId;
					//const mediaTypeKey = info.match.params.mediaTypeKey;
					this.#workspaceContext!.create(parentId /** , mediaTypeKey */);

					new UmbWorkspaceIsNewRedirectController(
						this,
						this.#workspaceContext!,
						this.shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:id',
				component: import('./media-workspace-editor.element.js'),
				setup: (_component, info) => {
					const id = info.match.params.id;
					this.#workspaceContext!.load(id);
				},
			},
		];

		new UmbExtensionsApiInitializer(this, umbExtensionsRegistry, 'workspaceContext', [this, this.#workspaceContext]);
	}

	render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot>`;
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

export default UmbMediaWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-workspace': UmbMediaWorkspaceElement;
	}
}
