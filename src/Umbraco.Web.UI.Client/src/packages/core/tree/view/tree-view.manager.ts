import type { UmbTreeViewItemModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbTreeViewManager extends UmbControllerBase {
	#views = new UmbArrayState<UmbTreeViewItemModel>([], (x) => x.unique);
	public readonly views = this.#views.asObservable();

	#currentView = new UmbObjectState<UmbTreeViewItemModel | undefined>(undefined);
	public readonly currentView = this.#currentView.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);
		this.#observeViews();
	}

	public setCurrentView(unique: string) {
		const view = this.#views.getValue().find((view) => view.unique === unique);
		this.#currentView.setValue(view);
	}

	public getCurrentView() {
		return this.#currentView.getValue();
	}

	#observeViews() {
		return new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, 'treeView', null, (views) => {
			const manifests = views.map((view) => view.manifest);

			const items = manifests.map((view) => ({
				unique: view.alias,
				entityType: 'tree-view',
				name: view.meta.label,
				icon: view.meta.icon,
			}));

			this.#views.setValue(items);

			// TODO: change this
			this.setCurrentView(items[0]?.unique ?? '');
		});
	}
}
