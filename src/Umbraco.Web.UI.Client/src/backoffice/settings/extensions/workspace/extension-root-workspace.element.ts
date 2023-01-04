import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { isManifestElementNameType } from '@umbraco-cms/extensions-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { ManifestTypes } from '@umbraco-cms/models';

@customElement('umb-extension-root-workspace')
export class UmbExtensionRootWorkspaceElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	@state()
	private _extensions: Array<ManifestTypes> = [];

	connectedCallback(): void {
		super.connectedCallback();
		this._observeExtensions();
	}

	private _observeExtensions() {
		this.observe<Array<ManifestTypes>>(umbExtensionsRegistry.extensions, (extensions) => {
			this._extensions = [...extensions];
		});
	}

	render() {
		return html`
			<umb-workspace-layout headline="Extensions" alias="Umb.Workspace.ExtensionRoot">
				<uui-box>
					<p>List of currently loaded extensions</p>
					<uui-table>
						<uui-table-head>
							<uui-table-head-cell>Type</uui-table-head-cell>
							<uui-table-head-cell>Name</uui-table-head-cell>
							<uui-table-head-cell>Alias</uui-table-head-cell>
							<uui-table-head-cell>Actions</uui-table-head-cell>
						</uui-table-head>

						${this._extensions.map(
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
			</umb-workspace-layout>
		`;
	}
}

export default UmbExtensionRootWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-extension-root-workspace': UmbExtensionRootWorkspaceElement;
	}
}
