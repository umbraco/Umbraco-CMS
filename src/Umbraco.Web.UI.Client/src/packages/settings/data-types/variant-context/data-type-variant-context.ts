import { UmbDataTypeWorkspaceContext } from "../workspace/data-type-workspace.context.js";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbInvariantWorkspaceVariantContext } from "@umbraco-cms/backoffice/workspace";

export class UmbDataTypeVariantContext extends UmbInvariantWorkspaceVariantContext<UmbDataTypeWorkspaceContext> {


	properties = this._workspace.properties

	// default data:

	constructor(host: UmbControllerHost, workspace: UmbDataTypeWorkspaceContext) {
		super(host, workspace);
	}

}
