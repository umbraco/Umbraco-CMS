import { UMB_TREE_PICKER_MODAL } from '../../tree-picker-modal/index.js';
import type { UmbMoveRepository } from './move-repository.interface.js';
import type { MetaEntityActionMoveToKind } from './types.js';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import type { UmbTreeItemModel } from '../../types.js';

export class UmbMoveToEntityAction extends UmbEntityActionBase<MetaEntityActionMoveToKind> {
	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		if (!this.args.entityType) throw new Error('Entity Type is not available');

		// 1. Get the repository
		const moveRepository = await createExtensionApiByAlias<UmbMoveRepository>(this, this.args.meta.moveRepositoryAlias);
		if (!moveRepository) throw new Error('Move Repository is not available');

		// 2. Get the filter if the repository provides one
		const customFilter = moveRepository.getSelectableFilter
			? await moveRepository.getSelectableFilter(this.args.unique)
			: undefined;

		const value = await umbOpenModal(this, UMB_TREE_PICKER_MODAL, {
			data: {
				treeAlias: this.args.meta.treeAlias,
				foldersOnly: this.args.meta.foldersOnly,
				expandTreeRoot: true,
				pickableFilter: (treeItem) =>
					treeItem.unique !== this.args.unique && (customFilter ? customFilter(treeItem) : true),
			},
		});

		const destinationUnique = value.selection[0];
		if (destinationUnique === undefined) throw new Error('Destination Unique is not available');

		const { error } = await moveRepository.requestMoveTo({
			unique: this.args.unique,
			destination: { unique: destinationUnique },
		});

		if (error) {
			throw error;
		}

		this.#reloadMenu();
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
