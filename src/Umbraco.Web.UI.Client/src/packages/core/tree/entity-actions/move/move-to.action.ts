import { UMB_TREE_PICKER_MODAL } from '../../tree-picker-modal/index.js';
import type { UmbTreeItemModel } from '../../types.js';
import type { UmbTreeRepository } from '../../data/tree-repository.interface.js';
import { UmbTreePlacementReloadController } from '../tree-placement-reload.controller.js';
import type { UmbMoveRepository } from './move-repository.interface.js';
import type { MetaEntityActionMoveToKind } from './types.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { linkEntityExpansionEntries } from '@umbraco-cms/backoffice/utils';

export class UmbMoveToEntityAction extends UmbEntityActionBase<MetaEntityActionMoveToKind> {
	protected async _getPickableFilter(unique: string): Promise<((item: UmbTreeItemModel) => boolean) | undefined> {
		return (treeItem) => treeItem.unique !== unique;
	}

	#searchConfig() {
		const alias = this.args.meta.searchProviderAlias;
		return alias ? { providerAlias: alias } : undefined;
	}

	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		if (!this.args.entityType) throw new Error('Entity Type is not available');

		const treeRepository = await createExtensionApiByAlias<UmbTreeRepository>(
			this,
			this.args.meta.treeRepositoryAlias,
		).catch(() => undefined);

		try {
			await this.#moveTo(this.args.unique, this.args.entityType, treeRepository);
		} finally {
			treeRepository?.destroy();
		}
	}

	async #moveTo(unique: string, entityType: string, treeRepository: UmbTreeRepository | undefined) {
		const [ancestors, pickableFilter] = await Promise.all([
			this.#requestAncestors(unique, entityType, treeRepository),
			this._getPickableFilter(unique),
		]);

		const treeExpansion = ancestors.length ? linkEntityExpansionEntries(ancestors) : undefined;

		const value = await umbOpenModal(this, UMB_TREE_PICKER_MODAL, {
			data: {
				headline: '#actions_move',
				confirmLabel: '#general_move',
				treeAlias: this.args.meta.treeAlias,
				foldersOnly: this.args.meta.foldersOnly,
				expandTreeRoot: true,
				treeExpansion,
				pickableFilter,
				search: this.#searchConfig(),
			},
		}).catch(() => undefined);

		// The modal was cancelled.
		if (!value) return;

		const destinationUnique = value.selection[0];
		if (destinationUnique === undefined) throw new Error('Destination Unique is not available');

		const moveRepository = await createExtensionApiByAlias<UmbMoveRepository>(this, this.args.meta.moveRepositoryAlias);
		if (!moveRepository) throw new Error('Move Repository is not available');

		const { error } = await moveRepository.requestMoveTo({
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
			// A move keeps the unique and entity type, so the moved entity is the one now in the destination.
			placedEntity: { unique, entityType },
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

export default UmbMoveToEntityAction;
