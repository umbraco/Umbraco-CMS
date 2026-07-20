import type { UmbContentTypeModel } from '../types.js';
import type { UmbContentTypeStructureManager } from './content-type-structure-manager.class.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

const MoveRootContainersIntoFirstTabHelperControllerAlias = Symbol('moveRootContainersHelper');
/**
 * This class is a helper class that specifically takes care of moving owner root containers and properties into the first created tab.
 * This will give the user the experience of the first tab becoming the 'main' tab.
 */
export class UmbContentTypeMoveRootGroupsIntoFirstTabHelper<T extends UmbContentTypeModel> extends UmbControllerBase {
	#structure?: UmbContentTypeStructureManager<T>;

	constructor(host: UmbControllerHost, structure: UmbContentTypeStructureManager<T>) {
		super(host, MoveRootContainersIntoFirstTabHelperControllerAlias);
		this.#structure = structure;
		this.#observeContainers();
	}

	async #observeContainers() {
		if (!this.#structure) return;

		await this.observe(
			this.#structure.ownerContainersOf('Tab', null),
			async (tabContainers) => {
				if (!tabContainers) return;
				// If the amount of containers now became 1, we should move all root containers into this tab:
				if (tabContainers?.length === 1) {
					const firstTabId = tabContainers[0].id;
					const structure = this.#structure; // This may be destroyed before the async operations are done, so lets keep a copy reference.
					if (!structure) return;

					// Move root Properties into the first Tab:
					const rootProperties = await structure.getOwnerPropertiesOf(null);
					rootProperties?.forEach((property) => {
						structure.updateProperty(null, property.unique, { container: { id: firstTabId } });
					});

					// Move root Groups into the first Tab:
					const rootContainers = structure.getOwnerContainers('Group', null);
					rootContainers?.forEach((groupContainer) => {
						structure.updateContainer(null, groupContainer.id, { parent: { id: firstTabId } });
					});
				}
				this.destroy();
			},
			'_observeMainContainer',
		).asPromise();
	}

	override destroy() {
		super.destroy();
		this.#structure = undefined;
	}
}
