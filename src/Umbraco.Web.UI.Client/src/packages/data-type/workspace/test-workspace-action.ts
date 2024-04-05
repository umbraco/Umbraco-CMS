import { UMB_VALIDATION_CONTEXT } from '@umbraco-cms/backoffice/validation';
import { type UmbSubmittableWorkspaceContext, UmbWorkspaceActionBase } from '@umbraco-cms/backoffice/workspace';

export class UmbInvalidateWorkspaceAction extends UmbWorkspaceActionBase<UmbSubmittableWorkspaceContext> {
	async execute() {
		const validationContext = await this.getContext(UMB_VALIDATION_CONTEXT);
		console.log(validationContext);
		validationContext.messages.removeMessagesByType('server');
		//validationContext.messages.addMessage('server', 'values[0]', 'This is a test message from workspace action');
		validationContext.messages.addMessage('server', 'values[1]', 'This is a test message from workspace action');
		return;
	}
}
