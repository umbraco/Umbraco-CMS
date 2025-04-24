import type { UmbLanguageDetailModel } from '../types.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';

import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbStructureItemModel } from '@umbraco-cms/backoffice/menu';

export class UmbLanguageNavigationStructureWorkspaceContext extends UmbContextBase {
	// TODO: figure out the correct type where we have "data" available
	#workspaceContext?: any;

	#structure = new UmbArrayState<UmbStructureItemModel>([], (x) => x.unique);
	public readonly structure = this.#structure.asObservable();

	constructor(host: UmbControllerHost) {
		// TODO: set up context token
		super(host, 'UmbMenuStructureWorkspaceContext');

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#requestStructure();
		});
	}

	async #requestStructure() {
		const data = (await this.observe(this.#workspaceContext?.data, () => {})?.asPromise()) as UmbLanguageDetailModel;
		if (!data) throw new Error('Data is not available');

		const items = [
			// TODO: figure out if we can get the root from somewhere
			// so we don't have to hardcode it
			{
				unique: null,
				entityType: 'language-root',
				name: 'Languages',
				isFolder: false,
			},
			{
				unique: data.unique,
				entityType: data.entityType,
				name: data.name,
				isFolder: false,
			},
		];

		this.#structure.setValue(items);
	}
}

export default UmbLanguageNavigationStructureWorkspaceContext;
