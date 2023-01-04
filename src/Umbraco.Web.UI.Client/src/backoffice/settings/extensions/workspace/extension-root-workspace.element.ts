import { html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { isManifestElementNameType } from '@umbraco-cms/extensions-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import type { ManifestTypes } from '@umbraco-cms/models';
import { UmbLitElement } from 'src/core/element/lit-element.element';

@customElement('umb-extension-root-workspace')
export class UmbExtensionRootWorkspaceElement extends UmbLitElement {
	@state()
	private _extensions?: Array<ManifestTypes> = undefined;

	connectedCallback(): void {
		super.connectedCallback();
		this._observeExtensions();
	}

	private _observeExtensions() {
		this.observe<Array<ManifestTypes>>(umbExtensionsRegistry.extensions, (extensions) => {
			this._extensions = extensions || undefined;
		});
	}

	render() {
		return html`
			<umb-workspace-entity headline="Extensions" alias="Umb.Workspace.ExtensionRoot">
				<uui-box>
					<p>List of currently loaded extensions</p>
					<uui-table>
						<uui-table-head>
							<uui-table-head-cell>Type</uui-table-head-cell>
							<uui-table-head-cell>Name</uui-table-head-cell>
							<uui-table-head-cell>Alias</uui-table-head-cell>
							<uui-table-head-cell>Actions</uui-table-head-cell>
						</uui-table-head>

						${this._extensions?.map(
							(extension) => html`
								<uui-table-row>
									<uui-table-cell>${extension.type}</uui-table-cell>
									<uui-table-cell>
										${isManifestElementNameType(extension) ? extension.name : 'Custom extension'}
									</uui-table-cell>
									<uui-table-cell>${extension.alias}</uui-table-cell>
									<uui-table-cell>
										<uui-button
											label="unload"
											@click=${() => umbExtensionsRegistry.unregister(extension.alias)}></uui-button>
									</uui-table-cell>
								</uui-table-row>
							`
						)}
					</uui-table>
				</uui-box>
			</umb-workspace-entity>
		`;
	}
}

export default UmbExtensionRootWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-extension-root-workspace': UmbExtensionRootWorkspaceElement;
	}
}
