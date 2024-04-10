import { UmbEntityActionBase } from '../../entity-action-base.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbMoveToRepository } from '@umbraco-cms/backoffice/repository';
import type { MetaEntityActionMoveToKind } from '@umbraco-cms/backoffice/extension-registry';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';

export class UmbMoveToEntityAction extends UmbEntityActionBase<MetaEntityActionMoveToKind> {
	async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		if (!this.args.entityType) throw new Error('Entity Type is not available');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, this.args.meta.treePickerModal) as any; // TODO: make generic picker interface with selection
		const value = await modalContext.onSubmit();
		const destinationUnique = value.selection[0];
		if (destinationUnique === undefined) throw new Error('Destination Unique is not available');

		const moveRepository = await createExtensionApiByAlias<UmbMoveToRepository>(
			this,
			this.args.meta.moveToRepositoryAlias,
		);
		if (!moveRepository) throw new Error('Move Repository is not available');

		await moveRepository.requestMove({ unique: this.args.unique, destination: { unique: destinationUnique } });
		// TODO: refresh tree
	}
}

export default UmbMoveToEntityAction;
