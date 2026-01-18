import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_CONTENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

// The Example Workspace Context Controller:
export class ExampleWorkspaceContextNameManipulation extends UmbControllerBase {
	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, async (workspace) => {
			if (!workspace) return;
			// Set the name if it's already empty (We do not want to overwrite if it's a Blueprint)
			// Notice we need to provide a Variant-ID to getName, as Document names are variant specific. Here we get the Invariant name — this will need to be extended if you are looking to support multiple variants.
			const variantId = UmbVariantId.CreateInvariant();
			const name = workspace.getName(variantId);
			if (name === undefined) {
				const manipulatedName = `New Document - ${new Date().toLocaleDateString('en-GB')}`;
				workspace.setName(manipulatedName, variantId);
			}
		});
	}
}

// Declare a api export, so Extension Registry can initialize this class:
export { ExampleWorkspaceContextNameManipulation as api };
