import type { UmbContentTypeModel, UmbPropertyTypeContainerModel } from '../types.js';
import type { UmbContentTypeStructureManager } from './content-type-structure-manager.class.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

const MoveRootContainersIntoFirstTabHelperControllerAlias = Symbol('moveRootContainersHelper');
/**
 * This class is a helper class that specifically takes care of moving owner root containers into the first created tab.
 * This will give the user the experience of the first tab becoming the 'main' tab.
 */
export class UmbContentTypeMoveRootGroupsIntoFirstTabHelper<T extends UmbContentTypeModel> extends UmbControllerBase {
	#structure?: UmbContentTypeStructureManager<T>;
	#tabContainers?: Array<UmbPropertyTypeContainerModel>;

	constructor(host: UmbControllerHost, structure: UmbContentTypeStructureManager<T>) {
		super(host, MoveRootContainersIntoFirstTabHelperControllerAlias);
		this.#structure = structure;
		this.#observeContainers();
	}

	#observeContainers() {
		if (!this.#structure) return;

		this.observe(
			this.#structure.ownerContainersOf('Tab', null),
			(tabContainers) => {
				const old = this.#tabContainers;
				this.#tabContainers = tabContainers;
				// If the amount of containers was 0 before and now becomes 1, we should move all root containers into this tab:
				if (old?.length === 0 && tabContainers?.length === 1) {
					const firstTabId = tabContainers[0].id;
					const rootContainers = this.#structure?.getOwnerContainers('Group', null);
					rootContainers?.forEach((groupContainer) => {
						this.#structure?.updateContainer(null, groupContainer.id, { parent: { id: firstTabId } });
					});
				}
			},
			'_observeMainContainer',
		);
	}

	override destroy() {
		super.destroy();
		this.#structure = undefined;
		this.#tabContainers = undefined;
	}
}
