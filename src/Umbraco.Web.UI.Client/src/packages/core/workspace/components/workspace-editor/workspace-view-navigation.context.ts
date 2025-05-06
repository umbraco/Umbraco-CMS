import { UmbWorkspaceViewContext } from './workspace-view.context.js';
import { UMB_WORKSPACE_VIEW_NAVIGATION_CONTEXT } from './workspace-view-navigation.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';

export class UmbWorkspaceViewNavigationContext extends UmbContextBase {
	//
	#init: Promise<void>;
	#views = new UmbBasicState(<Array<UmbWorkspaceViewContext>>[]);
	public readonly views = this.#views.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_WORKSPACE_VIEW_NAVIGATION_CONTEXT);

		this.#init = new UmbExtensionsManifestInitializer(
			this,
			umbExtensionsRegistry,
			'workspaceView',
			null,
			(workspaceViews) => {
				const oldViews = this.#views.getValue();

				// remove ones that are no longer contained in the workspaceViews (And thereby make the new array):
				const newViews = oldViews.filter(
					(view) => !workspaceViews.some((x) => x.manifest.alias === view.manifest.alias),
				);

				let hasDif = newViews.length !== oldViews.length;

				// Add ones that are new:
				workspaceViews
					.filter((view) => !newViews.some((x) => x.manifest.alias === view.manifest.alias))
					.forEach((view) => {
						newViews.push(new UmbWorkspaceViewContext(this, view.manifest));
						hasDif = true;
					});

				if (hasDif) {
					this.#views.setValue(newViews);
				}
			},
			'initViewApis',
			{},
		).asPromise();
	}

	async getViewContext(alias: string): Promise<UmbWorkspaceViewContext | undefined> {
		await this.#init;
		return this.#views.getValue().find((view) => view.manifest.alias === alias);
	}
}
