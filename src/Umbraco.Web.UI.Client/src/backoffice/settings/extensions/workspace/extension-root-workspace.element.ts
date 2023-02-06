import { html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { isManifestElementNameType, umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import type { ManifestBase } from '@umbraco-cms/models';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/modal';

@customElement('umb-extension-root-workspace')
export class UmbExtensionRootWorkspaceElement extends UmbLitElement {
	@state()
	private _extensions?: Array<ManifestBase> = undefined;

	private _modalService?: UmbModalService;

	connectedCallback(): void {
		super.connectedCallback();
		this._observeExtensions();

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (modalService) => {
			this._modalService = modalService;
		});
	}

	private _observeExtensions() {
		this.observe(umbExtensionsRegistry.extensionsSortedByTypeAndWeight(), (extensions) => {
			this._extensions = extensions || undefined;
		});
	}

	#removeExtension(extension: ManifestBase) {
		const modalHandler = this._modalService?.confirm({
			headline: 'Unload extension',
			confirmLabel: 'Unload',
			content: html`<p>Are you sure you want to unload the extension <strong>${extension.alias}</strong>?</p>`,
			color: 'danger',
		});

		modalHandler?.onClose().then(({ confirmed }: any) => {
			if (confirmed) {
				umbExtensionsRegistry.unregister(extension.alias);
			}
		});
	}

	render() {
		return html`
			<umb-workspace-layout headline="Extensions" alias="Umb.Workspace.ExtensionRoot">
				<uui-box>
					<uui-table>
						<uui-table-head>
							<uui-table-head-cell>Type</uui-table-head-cell>
							<uui-table-head-cell>Weight</uui-table-head-cell>
							<uui-table-head-cell>Name</uui-table-head-cell>
							<uui-table-head-cell>Alias</uui-table-head-cell>
							<uui-table-head-cell>Actions</uui-table-head-cell>
						</uui-table-head>

						${this._extensions?.map(
							(extension) => html`
								<uui-table-row>
									<uui-table-cell>${extension.type}</uui-table-cell>
									<uui-table-cell>${extension.weight ? extension.weight : 'Not Set'} </uui-table-cell>
									<uui-table-cell>
										${isManifestElementNameType(extension) ? extension.name : `[Custom extension] ${extension.name}`}
									</uui-table-cell>
									<uui-table-cell>${extension.alias}</uui-table-cell>
									<uui-table-cell>
										<uui-button
											label="Unload"
											color="danger"
											look="primary"
											@click=${() => this.#removeExtension(extension)}>
											<uui-icon name="umb:trash"></uui-icon>
										</uui-button>
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
