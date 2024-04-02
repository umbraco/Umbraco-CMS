import { UMB_VALIDATION_CONTEXT } from '@umbraco-cms/backoffice/validation';
import { type UmbSubmittableWorkspaceContext, UmbWorkspaceActionBase } from '@umbraco-cms/backoffice/workspace';

export class UmbInvalidateWorkspaceAction extends UmbWorkspaceActionBase<UmbSubmittableWorkspaceContext> {
	async execute() {
		const validationContext = await this.getContext(UMB_VALIDATION_CONTEXT);
		console.log(validationContext);
		return;
	}
}
