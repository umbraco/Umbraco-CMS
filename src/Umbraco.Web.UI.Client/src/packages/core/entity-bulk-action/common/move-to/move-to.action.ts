import { UmbEntityBulkActionBase } from '../../entity-bulk-action-base.js';
import { UmbBulkMoveOrCopyController } from '../bulk-move-or-copy.controller.js';
import type { UmbBulkMoveToRepository } from './move-to-repository.interface.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import type { MetaEntityBulkActionMoveToKind } from '@umbraco-cms/backoffice/extension-registry';

export class UmbMoveToEntityBulkAction extends UmbEntityBulkActionBase<MetaEntityBulkActionMoveToKind> {
	async execute() {
		await new UmbBulkMoveOrCopyController(this).run({
			selection: this.selection,
			pickerHeadline: '#actions_move',
			pickerConfirmLabel: '#general_move',
			progressHeadlineKey: 'actions_moveInProgress',
			foldersOnly: this.args.meta.foldersOnly,
			hideTreeRoot: this.args.meta.hideTreeRoot,
			treeAlias: this.args.meta.treeAlias,
			searchProviderAlias: this.args.meta.searchProviderAlias,
			perform: async (destinationUnique) => {
				const bulkMoveRepository = await createExtensionApiByAlias<UmbBulkMoveToRepository>(
					this,
					this.args.meta.bulkMoveRepositoryAlias,
				);
				if (!bulkMoveRepository) throw new Error('Bulk Move Repository is not available');

				return bulkMoveRepository.requestBulkMoveTo({
					uniques: this.selection,
					destination: { unique: destinationUnique },
				});
			},
		});
	}
}

export { UmbMoveToEntityBulkAction as api };
