import type { ManifestTreeView } from './tree-view.extension.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Fallback used when no `treeView` manifests are registered.
 * Trees should register at least one treeView manifest using `kind: 'classic'`.
 * @deprecated Register a treeView manifest with `kind: 'classic'` on your tree instead.
 */
const CLASSIC_FALLBACK: ManifestTreeView = {
	type: 'treeView',
	kind: 'classic',
	alias: 'Umb.TreeView.Classic.Fallback',
	name: 'Classic Tree View (fallback)',
	element: () => import('./classic/classic-tree-view.element.js'),
	weight: 0,
	meta: {
		label: '#treeView_classic',
		icon: 'icon-blockquote',
	},
};

export class UmbTreeViewManager extends UmbControllerBase {
	#views = new UmbArrayState<ManifestTreeView>([], (x) => x.alias);
	public readonly views = this.#views.asObservable();

	#currentView = new UmbObjectState<ManifestTreeView | undefined>(undefined);
	public readonly currentView = this.#currentView.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);

		new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, 'treeView', null, (result) => {
			const views = result.map((v) => v.manifest);

			this.#views.setValue(views);

			if (!views.length) {
				// No treeView manifests registered — use the built-in classic fallback.
				new UmbDeprecation({
					removeInVersion: '19.0.0',
					deprecated: 'Implicit classic tree view fallback',
					solution:
						"Register a treeView manifest with `kind: 'classic'` on your tree. The automatic fallback will be removed in Umbraco 19.",
				}).warn();
				this.#currentView.setValue(CLASSIC_FALLBACK);
				return;
			}

			this.#currentView.setValue(views[0]);
		});
	}

	setCurrentView(view: ManifestTreeView) {
		this.#currentView.setValue(view);
	}

	getCurrentView(): ManifestTreeView | undefined {
		return this.#currentView.getValue();
	}
}
