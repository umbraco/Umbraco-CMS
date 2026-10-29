import { UMB_TREE_PICKER_MODAL } from '../../tree-picker-modal/index.js';
import type { UmbTreeRepository } from '../../data/tree-repository.interface.js';
import { UmbTreePlacementReloadController } from '../tree-placement-reload.controller.js';
import type { MetaEntityActionDuplicateToKind, UmbDuplicateToRepository } from './types.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { linkEntityExpansionEntries } from '@umbraco-cms/backoffice/utils';

export class UmbDuplicateToEntityAction extends UmbEntityActionBase<MetaEntityActionDuplicateToKind> {
	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		if (!this.args.entityType) throw new Error('Entity Type is not available');

		const treeRepository = await createExtensionApiByAlias<UmbTreeRepository>(
			this,
			this.args.meta.treeRepositoryAlias,
		).catch(() => undefined);

		try {
			await this.#duplicateTo(this.args.unique, this.args.entityType, treeRepository);
		} finally {
			treeRepository?.destroy();
		}
	}

	async #duplicateTo(unique: string, entityType: string, treeRepository: UmbTreeRepository | undefined) {
		const ancestors = await this.#requestAncestors(unique, entityType, treeRepository);

		const value = await umbOpenModal(this, UMB_TREE_PICKER_MODAL, {
			data: {
				headline: '#actions_copyTo',
				confirmLabel: '#general_copy',
				treeAlias: this.args.meta.treeAlias,
				foldersOnly: this.args.meta.foldersOnly,
				expandTreeRoot: true,
				treeExpansion: ancestors.length ? linkEntityExpansionEntries(ancestors) : undefined,
			},
		}).catch(() => undefined);

		// The modal was cancelled.
		if (!value) return;

		const destinationUnique = value.selection[0];
		if (destinationUnique === undefined) throw new Error('Destination Unique is not available');

		const duplicateRepository = await createExtensionApiByAlias<UmbDuplicateToRepository>(
			this,
			this.args.meta.duplicateRepositoryAlias,
		);
		if (!duplicateRepository) throw new Error('Duplicate repository is not available');

		const { data: duplicateUnique, error } = await duplicateRepository.requestDuplicateTo({
			unique,
			destination: { unique: destinationUnique },
		});

		if (error) {
			throw error;
		}

		// TODO: If the tree picker modal value included the entity type of the picked items, the destination could be passed to the reload directly, saving a request. [MR]
		await new UmbTreePlacementReloadController(this).reload({
			treeRepository,
			source: { unique, entityType },
			// The copy has the entity type of the original. Its unique is only known when the repository returns it.
			placedEntity: duplicateUnique ? { unique: duplicateUnique, entityType } : undefined,
		});
	}

	async #requestAncestors(unique: string, entityType: string, treeRepository: UmbTreeRepository | undefined) {
		try {
			const { data } = (await treeRepository?.requestTreeItemAncestors({ treeItem: { unique, entityType } })) ?? {};
			// Exclude self — the API returns the descendant as part of the ancestors list, but we only want to expand its parents.
			return data?.filter((item) => item.unique !== unique) ?? [];
		} catch {
			// Tree pre-expansion is a UX convenience — if it fails the modal still opens normally.
			return [];
		}
	}
}

export { UmbDuplicateToEntityAction as api };
