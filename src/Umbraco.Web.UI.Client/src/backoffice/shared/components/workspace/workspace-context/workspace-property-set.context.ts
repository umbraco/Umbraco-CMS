import { UmbWorkspacePropertySetContextInterface } from './workspace-property-set-context.interface';
import { UmbWorkspaceInvariantableEntityContextInterface } from './workspace-invariantable-entity-context.interface';
import { UmbContextProviderController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbWorkspacePropertySetContext implements UmbWorkspacePropertySetContextInterface {
	workspaceContext: UmbWorkspaceInvariantableEntityContextInterface;

	constructor(host: UmbControllerHostInterface, workspaceContext: UmbWorkspaceInvariantableEntityContextInterface) {
		this.workspaceContext = workspaceContext;
		new UmbContextProviderController(host, 'umbWorkspacePropertySetContext', this);
	}

	propertyValueByAlias(alias: string) {
		return this.workspaceContext.propertyValueByAlias(alias);
	}
	getPropertyValue(alias: string) {
		return this.workspaceContext.getPropertyValue(alias);
	}
	setPropertyValue(alias: string, value: unknown) {
		return this.workspaceContext.setPropertyValue(alias, value);
	}
}
