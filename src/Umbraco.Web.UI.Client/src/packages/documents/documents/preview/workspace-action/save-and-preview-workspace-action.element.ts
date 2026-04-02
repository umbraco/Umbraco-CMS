import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionApi, UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { stringOrStringArrayIntersects } from '@umbraco-cms/backoffice/utils';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbWorkspaceActionElement } from '@umbraco-cms/backoffice/workspace';
import type { ManifestWorkspaceActionMenuItem } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-save-and-preview-workspace-action')
export class UmbSaveAndPreviewWorkspaceActionElement extends UmbWorkspaceActionElement {
	override observeExtensions(aliases: string[]) {
		this._extensionsController?.destroy();
		this._extensionsController = new UmbExtensionsElementAndApiInitializer<
			ManifestWorkspaceActionMenuItem,
			'workspaceActionMenuItem',
			ManifestWorkspaceActionMenuItem
		>(
			this,
			umbExtensionsRegistry,
			'workspaceActionMenuItem',
			(manifest) => [{ meta: manifest.meta }],
			(action) => stringOrStringArrayIntersects(action.forWorkspaceActions, aliases),
			async (actions) => {
				const firstAction = actions.shift();

				if (firstAction) {
					this._buttonLabel = (firstAction.manifest.meta as any).label;
					const api = await createExtensionApi(this, firstAction.manifest, []);
					if (api) {
						(api as any).manifest = firstAction.manifest;
						this._actionApi = api;
					}
				}

				this._items = actions;
			},
			undefined, // We can leave the alias to undefined, as we destroy this our selfs.
		);
	}
}

export default UmbSaveAndPreviewWorkspaceActionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-save-and-preview-workspace-action': UmbSaveAndPreviewWorkspaceActionElement;
	}
}
