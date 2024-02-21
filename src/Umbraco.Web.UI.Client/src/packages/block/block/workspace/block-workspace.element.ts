import type { UmbBlockWorkspaceContext } from './block-workspace.context.js';
import { UmbBlockWorkspaceEditorElement } from './block-workspace-editor.element.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import { UmbWorkspaceIsNewRedirectController } from '@umbraco-cms/backoffice/workspace';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionsApiInitializer, createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { decodeFilePath } from '@umbraco-cms/backoffice/utils';

@customElement('umb-block-workspace')
export class UmbBlockWorkspaceElement extends UmbLitElement {
	//
	#manifest?: ManifestWorkspace;
	#workspaceContext?: UmbBlockWorkspaceContext;
	#editorElement = () => {
		const element = new UmbBlockWorkspaceEditorElement();
		element.workspaceAlias = this.#manifest!.alias;
		return element;
	};

	@state()
	_routes: UmbRoute[] = [];

	public set manifest(manifest: ManifestWorkspace) {
		this.#manifest = manifest;
		createExtensionApi(manifest, [this, { manifest: manifest }]).then((context) => {
			if (context) {
				this.#gotWorkspaceContext(context);
				// TODO: Do we need to recreate when ID changed? Or is that a responsibility of the context it self?
				new UmbExtensionsApiInitializer(this, umbExtensionsRegistry, 'workspaceContext', [
					this,
					this.#workspaceContext,
				]);
			}
		});
	}

	#gotWorkspaceContext(context: UmbApi) {
		this.#workspaceContext = context as UmbBlockWorkspaceContext;

		this._routes = [
			{
				path: 'create/:elementTypeKey',
				component: this.#editorElement,
				setup: async (_component, info) => {
					const elementTypeKey = info.match.params.elementTypeKey;
					this.#workspaceContext!.create(elementTypeKey);

					new UmbWorkspaceIsNewRedirectController(
						this,
						this.#workspaceContext!,
						this.shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:udi',
				component: this.#editorElement,
				setup: (_component, info) => {
					const udi = decodeFilePath(info.match.params.udi);
					this.#workspaceContext!.load(udi);
				},
			},
		];
	}

	render() {
		return html`<umb-router-slot .routes="${this._routes}"></umb-router-slot>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbBlockWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-workspace': UmbBlockWorkspaceElement;
	}
}
