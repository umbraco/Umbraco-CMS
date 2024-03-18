import { UMB_LANGUAGE_NAVIGATION_STRUCTURE_WORKSPACE_CONTEXT } from './language-navigation-structure.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';

import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

interface UmbStructureItemModel {
	unique: string | null;
	entityType: string;
	name: string;
}

export class UmbLanguageNavigationStructureWorkspaceContext extends UmbContextBase<UmbLanguageNavigationStructureWorkspaceContext> {
	#workspaceContext?: any;

	#structure = new UmbArrayState<UmbStructureItemModel>([], (x) => x.unique);
	public readonly structure = this.#structure.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_LANGUAGE_NAVIGATION_STRUCTURE_WORKSPACE_CONTEXT);

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#requestAncestors();
		});
	}

	async #requestAncestors() {
		/* TODO: implement breadcrumb for new items
		 We currently miss the parent item name for new items. We need to align with backend
		 how to solve it */
		const isNew = this.#workspaceContext?.getIsNew();
		if (isNew === true) return;

		const data = await this.observe(this.#workspaceContext?.data, () => {})?.asPromise();
		if (!data) throw new Error('Data is not available');

		const items = [
			{
				unique: null,
				entityType: 'language-root',
				name: 'Languages',
			},
			{
				unique: data.unique,
				entityType: data.entityType,
				name: data.name,
			},
		];

		this.#structure.setValue(items);
	}
}

export default UmbLanguageNavigationStructureWorkspaceContext;
