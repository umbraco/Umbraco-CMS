import { UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT } from './property-type-settings-modal.context-token.js';
import type {
	UmbPropertyTypeSettingsModalData,
	UmbPropertyTypeSettingsModalValue,
} from './property-type-settings-modal.token.js';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_MODAL_CONTEXT, type UmbModalContext } from '@umbraco-cms/backoffice/modal';

export const UMB_PROPERTY_TYPE_WORKSPACE_ALIAS = 'Umb.Workspace.PropertyType';

/**
 * This is a very simplified workspace context, just to serve one for the imitated property type workspace. (As its not a real workspace, but this does as well provide the ability for extension-conditions to match with this workspace, as entity type and alias is available.)  [NL]
 */
export class UmbPropertyTypeWorkspaceContext
	extends UmbContextBase<UmbPropertyTypeWorkspaceContext>
	implements UmbWorkspaceContext
{
	#modal?: UmbModalContext<UmbPropertyTypeSettingsModalData, UmbPropertyTypeSettingsModalValue>;

	constructor(host: UmbControllerHost) {
		super(host, UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT);

		this.consumeContext(UMB_MODAL_CONTEXT, (context) => {
			this.#modal = context as UmbModalContext<UmbPropertyTypeSettingsModalData, UmbPropertyTypeSettingsModalValue>;
		});
	}

	get workspaceAlias() {
		return UMB_PROPERTY_TYPE_WORKSPACE_ALIAS;
	}

	getUnique() {
		return this.#modal?.getValue()?.alias ?? '';
	}

	getEntityType() {
		return 'property-type';
	}

	getLabel() {
		return this.#modal?.getValue()?.name ?? '';
	}
}

export default UmbPropertyTypeWorkspaceContext;
