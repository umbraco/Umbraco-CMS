import { UmbWorkspaceActionBase } from '@umbraco-cms/backoffice/workspace';

export class ExampleIsLoadedTestAction extends UmbWorkspaceActionBase {
	override async execute() {
		alert('Document workspace is loaded!');
	}
}

export { ExampleIsLoadedTestAction as api };
