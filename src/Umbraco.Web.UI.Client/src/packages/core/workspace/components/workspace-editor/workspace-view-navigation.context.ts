import { UmbWorkspaceViewContext } from './workspace-view.context.js';
import { UMB_WORKSPACE_VIEW_NAVIGATION_CONTEXT } from './workspace-view-navigation.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';
import { UmbHintManager } from '@umbraco-cms/backoffice/utils';

export class UmbWorkspaceViewNavigationContext extends UmbContextBase {
	//
	#init: Promise<void>;
	/**
	 * State holding the permitted Workspace Views as a Workspace View Context
	 */
	#views = new UmbBasicState(<Array<UmbWorkspaceViewContext>>[]);
	public readonly views = this.#views.asObservable();

	/** HERE THIS IS THE PLACE TO CONTINUE!!!!!!! */

	// TODO: We still need to sync these up to the workspace context...
	#hints = new UmbHintManager(this);

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
				const viewsToKeep = oldViews.filter(
					(view) => !workspaceViews.some((x) => x.manifest.alias === view.manifest.alias),
				);

				const diff = viewsToKeep.length !== workspaceViews.length;

				if (diff) {
					const newViews = [...viewsToKeep];

					// Add ones that are new:
					workspaceViews
						.filter((view) => !viewsToKeep.some((x) => x.manifest.alias === view.manifest.alias))
						.forEach((view) => {
							const context = new UmbWorkspaceViewContext(this, view.manifest);
							context.hints.bindWith(this.#hints);
							newViews.push(context);
						});

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
