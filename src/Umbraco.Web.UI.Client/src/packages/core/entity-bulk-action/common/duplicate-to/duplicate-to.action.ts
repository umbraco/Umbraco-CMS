import { UmbEntityBulkActionBase } from '../../entity-bulk-action-base.js';
import { UmbBulkTreePickerActionController } from '../bulk-tree-picker-action.controller.js';
import type { UmbBulkDuplicateToRepository } from './duplicate-to-repository.interface.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import type { MetaEntityBulkActionDuplicateToKind } from '@umbraco-cms/backoffice/extension-registry';

export class UmbDuplicateToEntityBulkAction extends UmbEntityBulkActionBase<MetaEntityBulkActionDuplicateToKind> {
	async execute() {
		await new UmbBulkTreePickerActionController(this).run({
			selection: this.selection,
			pickerHeadline: '#actions_copyTo',
			pickerConfirmLabel: '#general_copy',
			progressHeadline: '#actions_copyInProgress',
			foldersOnly: this.args.meta.foldersOnly,
			hideTreeRoot: this.args.meta.hideTreeRoot,
			treeAlias: this.args.meta.treeAlias,
			searchProviderAlias: this.args.meta.searchProviderAlias,
			perform: async (destinationUnique) => {
				const bulkDuplicateRepository = await createExtensionApiByAlias<UmbBulkDuplicateToRepository>(
					this,
					this.args.meta.bulkDuplicateRepositoryAlias,
				);
				if (!bulkDuplicateRepository) throw new Error('Bulk Duplicate Repository is not available');

				return bulkDuplicateRepository.requestBulkDuplicateTo({
					uniques: this.selection,
					destination: { unique: destinationUnique },
				});
			},
		});
	}
}

export { UmbDuplicateToEntityBulkAction as api };
