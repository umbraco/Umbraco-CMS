import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/document';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

// The Example Workspace Context Controller:
export class WorkspaceContextNameManipulation extends UmbContextBase {
	constructor(host: UmbControllerHost) {
		super(host, MANIPULATE_NAME_WORKSPACE_CONTEXT);

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (workspace) => {
			this.observe(workspace?.loading.isOn, (isLoading) => {
				// Only manipulate the name when we are not loading:
				if (isLoading) return;
				// Get if new:
				const isNew = workspace?.getIsNew() ?? false;
				if (isNew) {
					// Set the name if its already empty( We do not want to overwrite if its a Blueprint)
					// Notice we can but a ! on the workspace, if the Document is new, then we also know we have a workspace.
					// Notice we need to provide a Variant-ID to getName, as Document names are variant specific. Here we get the Invariant name — this will need to be extended if you are looking to support multiple variants.
					const variantId = UmbVariantId.CreateInvariant();
					const name = workspace!.getName(variantId);
					if (name === undefined) {
						const manipulatedName = `New Document - ${new Date().toLocaleDateString('en-Gb')}`;
						workspace!.setName(manipulatedName, variantId);
					}
				}
			});
		});
	}
}

// Declare a api export, so Extension Registry can initialize this class:
export const api = WorkspaceContextNameManipulation;

// Declare a Context Token that other elements can use to request the WorkspaceContextNameManipulation:
export const MANIPULATE_NAME_WORKSPACE_CONTEXT = new UmbContextToken<WorkspaceContextNameManipulation>(
	'UmbWorkspaceContext',
	'example.workspaceContext.initialName',
);
