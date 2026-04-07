import { UMB_PARTIAL_VIEW_FROM_SNIPPET_MODAL } from '../snippet-modal/index.js';
import { UmbEntityCreateOptionActionBase } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { MetaEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbFromSnippetPartialViewCreateOptionAction extends UmbEntityCreateOptionActionBase<MetaEntityCreateOptionAction> {
	override async execute() {
		await umbOpenModal(this, UMB_PARTIAL_VIEW_FROM_SNIPPET_MODAL, {
			data: {
				parent: {
					entityType: this.args.entityType,
					unique: this.args.unique,
				},
			},
		});
	}
}

export { UmbFromSnippetPartialViewCreateOptionAction as api };
