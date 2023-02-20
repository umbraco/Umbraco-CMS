import { UmbVariantId } from '../../../variants/variant-id.class';
import { UmbWorkspacePropertySetContextInterface } from './workspace-property-set-context.interface';
import { UmbWorkspaceVariantableEntityContextInterface } from './workspace-variantable-entity-context.interface';
import { UmbContextProviderController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbWorkspaceVariantPropertySetContext implements UmbWorkspacePropertySetContextInterface {
	workspaceContext: UmbWorkspaceVariantableEntityContextInterface;
	variantId: UmbVariantId;

	constructor(
		host: UmbControllerHostInterface,
		workspaceContext: UmbWorkspaceVariantableEntityContextInterface,
		variantId: UmbVariantId
	) {
		this.workspaceContext = workspaceContext;
		this.variantId = variantId;
		new UmbContextProviderController(host, 'umbWorkspacePropertySetContext', this);
	}

	setVariantId(variantId: UmbVariantId) {
		this.variantId = variantId;
	}

	propertyValueByAlias(alias: string) {
		return this.workspaceContext.propertyValueByAlias(alias, this.variantId);
	}
	getPropertyValue(alias: string) {
		return this.workspaceContext.getPropertyValue(alias, this.variantId);
	}
	setPropertyValue(alias: string, value: unknown) {
		return this.workspaceContext.setPropertyValue(alias, value, this.variantId);
	}
}
