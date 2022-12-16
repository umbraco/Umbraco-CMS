import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { isManifestElementType } from '@umbraco-cms/extensions-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { ManifestTypes } from '@umbraco-cms/models';

@customElement('umb-workspace-extensions')
export class UmbWorkspaceExtensionsElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	
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
			<umb-workspace-entity-layout headline="Extensions" alias="Umb.Workspace.Extensions">
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
										${isManifestElementType(extension) ? extension.name : 'Custom extension'}
									</uui-table-cell>
									<uui-table-cell>${extension.alias}</uui-table-cell>
									<uui-table-cell><uui-button label="unload" @click=${() => umbExtensionsRegistry.unregister(extension.alias)}></uui-button></uui-table-cell>
								</uui-table-row>
							`
						)}
					</uui-table>
				</uui-box>
			</umb-workspace-entity-layout>
		`;
	}
}

export default UmbWorkspaceExtensionsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-extensions': UmbWorkspaceExtensionsElement;
	}
}
