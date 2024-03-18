import { UmbStylesheetTreeRepository } from '../tree/index.js';
import { UMB_STYLESHEET_NAVIGATION_STRUCTURE_WORKSPACE_CONTEXT } from './stylesheet-navigation-structure.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';

import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

interface UmbStructureItemModel {
	unique: string | null;
	entityType: string;
	name: string;
}

export class UmbStylesheetNavigationStructureWorkspaceContext extends UmbContextBase<UmbStylesheetNavigationStructureWorkspaceContext> {
	#workspaceContext?: any;
	#treeRepository = new UmbStylesheetTreeRepository(this);

	#structure = new UmbArrayState<UmbStructureItemModel>([], (x) => x.unique);
	public readonly structure = this.#structure.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_STYLESHEET_NAVIGATION_STRUCTURE_WORKSPACE_CONTEXT);

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#requestStructure();
		});
	}

	async #requestStructure() {
		const isNew = this.#workspaceContext?.getIsNew();
		const uniqueObservable = isNew ? this.#workspaceContext?.parentUnique : this.#workspaceContext?.unique;

		const unique = (await this.observe(uniqueObservable, () => {})?.asPromise()) as string;
		if (!unique) throw new Error('Unique is not available');

		const { data } = await this.#treeRepository.requestTreeItemAncestors({ descendantUnique: unique });

		if (data) {
			const structureItems = data.map((ancestor) => {
				return {
					unique: ancestor.unique,
					entityType: ancestor.entityType,
					name: ancestor.name,
				};
			});

			this.#structure.setValue(structureItems);
		}
	}
}

export default UmbStylesheetNavigationStructureWorkspaceContext;
