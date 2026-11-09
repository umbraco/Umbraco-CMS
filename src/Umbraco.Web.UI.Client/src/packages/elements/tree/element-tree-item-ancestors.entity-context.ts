import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../entity.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UMB_ANCESTORS_ENTITY_CONTEXT,
	UmbAncestorsEntityContext,
	type UmbEntityModel,
} from '@umbraco-cms/backoffice/entity';

/**
 * Provides the ancestors of a tree item, built from the parent tree item's ancestors and the parent itself.
 * The tree endpoint does not return ancestors, so they are derived while the tree is rendered.
 */
export class UmbElementTreeItemAncestorsEntityContext extends UmbAncestorsEntityContext {
	#parentUnique: string | null | undefined;
	#parentAncestors: Array<UmbEntityModel> = [];

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_ANCESTORS_ENTITY_CONTEXT, (parentContext) => {
			this.observe(
				parentContext?.ancestors,
				(ancestors) => {
					this.#parentAncestors = ancestors ?? [];
					this.#update();
				},
				'umbObserveParentTreeItemAncestors',
			);
		}).skipHost();
	}

	setParentUnique(unique: string | null | undefined) {
		this.#parentUnique = unique;
		this.#update();
	}

	#update() {
		if (!this.#parentUnique) {
			this.setAncestors([]);
			return;
		}

		this.setAncestors([
			...this.#parentAncestors,
			{ unique: this.#parentUnique, entityType: UMB_ELEMENT_FOLDER_ENTITY_TYPE },
		]);
	}
}
