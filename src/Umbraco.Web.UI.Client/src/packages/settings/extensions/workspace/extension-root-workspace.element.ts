import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import { isManifestElementNameType } from '@umbraco-cms/backoffice/extension-api';
import { ManifestTypes, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_CONFIRM_MODAL,
} from '@umbraco-cms/backoffice/modal';

@customElement('umb-extension-root-workspace')
export class UmbExtensionRootWorkspaceElement extends UmbLitElement {
	@state()
	private _extensions?: Array<ManifestTypes> = undefined;

	private _modalContext?: UmbModalManagerContext;

	connectedCallback(): void {
		super.connectedCallback();
		this._observeExtensions();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	private _observeExtensions() {
		this.observe(
			umbExtensionsRegistry.extensions.pipe(
				map((exts) =>
					exts.sort((a, b) => {
						// If type is the same, sort by weight
						if (a.type === b.type) {
							return (b.weight || 0) - (a.weight || 0);
						}
						// Otherwise sort by type
						return a.type.localeCompare(b.type);
					})
				)
			),
			(extensions) => {
				this._extensions = extensions || undefined;
			},
			'_observeExtensionRegistry'
		);
	}

	async #removeExtension(extension: ManifestTypes) {
		const modalHandler = this._modalContext?.open(UMB_CONFIRM_MODAL, {
			headline: 'Unload extension',
			confirmLabel: 'Unload',
			content: html`<p>Are you sure you want to unload the extension <strong>${extension.alias}</strong>?</p>`,
			color: 'danger',
		});

		await modalHandler?.onSubmit();
		umbExtensionsRegistry.unregister(extension.alias);
	}

	render() {
		return html`
			<umb-workspace-editor headline="Extensions" alias="Umb.Workspace.ExtensionRoot" .enforceNoFooter=${true}>
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
			</umb-workspace-editor>
		`;
	}

	static styles = [
		css`
			uui-box {
				margin: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbExtensionRootWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-extension-root-workspace': UmbExtensionRootWorkspaceElement;
	}
}
