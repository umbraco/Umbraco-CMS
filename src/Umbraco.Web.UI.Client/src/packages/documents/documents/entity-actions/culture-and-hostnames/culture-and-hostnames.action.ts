import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_CULTURE_AND_HOSTNAMES_MODAL } from '@umbraco-cms/backoffice/document';

export class UmbDocumentCultureAndHostnamesEntityAction extends UmbEntityActionBase<never> {
	override async execute() {
		await umbOpenModal(this, UMB_CULTURE_AND_HOSTNAMES_MODAL, {
			data: { unique: this.args.unique },
		});
	}
}

export { UmbDocumentCultureAndHostnamesEntityAction as api };
