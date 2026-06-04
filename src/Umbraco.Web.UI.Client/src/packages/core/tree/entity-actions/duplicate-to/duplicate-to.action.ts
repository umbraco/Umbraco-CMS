import { UMB_DUPLICATE_TO_MODAL } from './modal/duplicate-to-modal.token.js';
import type { MetaEntityActionDuplicateToKind, UmbDuplicateToRepository } from './types.js';
import type { UmbTreeRepository } from '../../data/tree-repository.interface.js';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { linkEntityExpansionEntries } from '@umbraco-cms/backoffice/utils';

export class UmbDuplicateToEntityAction extends UmbEntityActionBase<MetaEntityActionDuplicateToKind> {
	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		if (!this.args.entityType) throw new Error('Entity Type is not available');

		const ancestors = await this.#requestAncestors();

		const value = await umbOpenModal(this, UMB_DUPLICATE_TO_MODAL, {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
				treeAlias: this.args.meta.treeAlias,
				foldersOnly: this.args.meta.foldersOnly,
				expansion: ancestors ? linkEntityExpansionEntries(ancestors) : [],
			},
		});

		const destinationUnique = value.destination.unique;
		if (destinationUnique === undefined) throw new Error('Destination Unique is not available');

		const duplicateRepository = await createExtensionApiByAlias<UmbDuplicateToRepository>(
			this,
			this.args.meta.duplicateRepositoryAlias,
		);
		if (!duplicateRepository) throw new Error('Duplicate repository is not available');

		const { error } = await duplicateRepository.requestDuplicateTo({
			unique: this.args.unique,
			destination: { unique: destinationUnique },
		});

		if (error) {
			throw error;
		}

		this.#reloadMenu();
	}

	async #requestAncestors() {
		const treeRepository = await createExtensionApiByAlias<UmbTreeRepository>(this, this.args.meta.treeRepositoryAlias);
		const { data } =
			(await treeRepository?.requestTreeItemAncestors({
				treeItem: { unique: this.args.unique!, entityType: this.args.entityType! },
			})) ?? {};
		// Exclude self — the API returns the descendant as part of the ancestors list, but we only want to expand its parents.
		return data?.filter((item) => item.unique !== this.args.unique);
	}

	async #reloadMenu() {
		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!actionEventContext) throw new Error('Action event context is not available');
		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);

		// TODO: Reload destination
	}
}

export { UmbDuplicateToEntityAction as api };
