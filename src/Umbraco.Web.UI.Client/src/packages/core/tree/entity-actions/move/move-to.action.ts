import { UMB_TREE_PICKER_MODAL } from '../../tree-picker-modal/index.js';
import type { UmbTreeItemModel } from '../../types.js';
import type { UmbTreeRepository } from '../../data/tree-repository.interface.js';
import type { UmbMoveRepository } from './move-repository.interface.js';
import type { MetaEntityActionMoveToKind } from './types.js';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { linkEntityExpansionEntries } from '@umbraco-cms/backoffice/utils';

export class UmbMoveToEntityAction extends UmbEntityActionBase<MetaEntityActionMoveToKind> {
	protected async _getPickableFilter(unique: string): Promise<((item: UmbTreeItemModel) => boolean) | undefined> {
		return (treeItem) => treeItem.unique !== unique;
	}

	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		if (!this.args.entityType) throw new Error('Entity Type is not available');

		const ancestors = await this.#requestAncestors();

		const value = await umbOpenModal(this, UMB_TREE_PICKER_MODAL, {
			data: {
				treeAlias: this.args.meta.treeAlias,
				foldersOnly: this.args.meta.foldersOnly,
				expandTreeRoot: true,
				treeExpansion: ancestors.length ? linkEntityExpansionEntries(ancestors) : undefined,
				pickableFilter: await this._getPickableFilter(this.args.unique),
			},
		});

		const destinationUnique = value.selection[0];
		if (destinationUnique === undefined) throw new Error('Destination Unique is not available');

		const moveRepository = await createExtensionApiByAlias<UmbMoveRepository>(this, this.args.meta.moveRepositoryAlias);
		if (!moveRepository) throw new Error('Move Repository is not available');

		const { error } = await moveRepository.requestMoveTo({
			unique: this.args.unique,
			destination: { unique: destinationUnique },
		});

		if (error) {
			throw error;
		}

		this.#reloadMenu();
	}

	async #requestAncestors() {
		try {
			const treeRepository = await createExtensionApiByAlias<UmbTreeRepository>(this, this.args.meta.treeRepositoryAlias);
			const { data } =
				(await treeRepository?.requestTreeItemAncestors({
					treeItem: { unique: this.args.unique!, entityType: this.args.entityType! },
				})) ?? {};
			// Exclude self — the API returns the descendant as part of the ancestors list, but we only want to expand its parents.
			return data?.filter((item) => item.unique !== this.args.unique) ?? [];
		} catch {
			// Tree pre-expansion is a UX convenience — if it fails the modal still opens normally.
			return [];
		}
	}

	async #reloadMenu() {
		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!actionEventContext) throw new Error('Action Event Context is not available');
		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);

		// TODO: Reload destination
	}
}

export default UmbMoveToEntityAction;
