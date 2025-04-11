import { UMB_CONTENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';
import { UmbWorkspaceActionBase, type UmbWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

// The Example Incrementor Workspace Action Controller:
export class ExampleWorkspaceActionManipulateReadonlyPermission
	extends UmbWorkspaceActionBase
	implements UmbWorkspaceAction
{
	#isOn = false;

	// This method is executed
	override async execute() {
		const context = await this.getContext(UMB_CONTENT_WORKSPACE_CONTEXT);
		if (!context) {
			throw new Error('No context found');
		}
		if (this.#isOn) {
			context.propertyReadOnlyGuard.removeRule('exampleRule');
		} else {
			context.propertyReadOnlyGuard.addRule({
				unique: 'exampleRule',
				propertyType: { unique: '1_tipTap' }, // Notice ID is very short here as this is the mock data. Real Property type IDs are GUIDs.
				//variantId: UmbVariantId.Create({ culture: 'en-US', segment: null }),
				permitted: true,
			});
		}

		this.#isOn = !this.#isOn;
	}
}

// Declare a api export, so Extension Registry can initialize this class:
export const api = ExampleWorkspaceActionManipulateReadonlyPermission;
