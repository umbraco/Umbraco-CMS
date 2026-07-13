import type { UmbElementPublishingWorkspaceContext, UmbElementVariantOptionModel } from '../types.js';
import { UMB_ELEMENT_PUBLISHING_WORKSPACE_CONTEXT } from '../publishing/index.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbPublishableSplitViewVariantSelectorElement } from '@umbraco-cms/backoffice/content';

@customElement('umb-element-workspace-split-view-variant-selector')
export class UmbElementWorkspaceSplitViewVariantSelectorElement extends UmbPublishableSplitViewVariantSelectorElement<
	UmbElementVariantOptionModel,
	UmbElementPublishingWorkspaceContext
> {
	protected override getPublishingContextToken() {
		return UMB_ELEMENT_PUBLISHING_WORKSPACE_CONTEXT;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-workspace-split-view-variant-selector': UmbElementWorkspaceSplitViewVariantSelectorElement;
	}
}
