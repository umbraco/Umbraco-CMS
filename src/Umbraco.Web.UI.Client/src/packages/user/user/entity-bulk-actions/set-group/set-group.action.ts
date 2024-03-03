import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';

export class UmbSetGroupUserEntityBulkAction extends UmbEntityBulkActionBase<never> {
	#modalContext?: UmbModalManagerContext;

	async execute() {
		//TODO: Implement
		alert('Bulk set group is not implemented yet');
	}

	destroy(): void {}
}
