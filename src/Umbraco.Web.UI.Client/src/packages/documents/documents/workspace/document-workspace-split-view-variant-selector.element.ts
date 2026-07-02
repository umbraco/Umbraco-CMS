import type { UmbDocumentPublishingWorkspaceContext, UmbDocumentVariantOptionModel } from '../types.js';
import { sortVariants } from '../utils.js';
import { UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT } from '../publishing/index.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbPublishableSplitViewVariantSelectorElement } from '@umbraco-cms/backoffice/content';

@customElement('umb-document-workspace-split-view-variant-selector')
export class UmbDocumentWorkspaceSplitViewVariantSelectorElement extends UmbPublishableSplitViewVariantSelectorElement<
	UmbDocumentVariantOptionModel,
	UmbDocumentPublishingWorkspaceContext
> {
	protected override _variantSorter = sortVariants;

	protected override getPublishingContextToken() {
		return UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-split-view-variant-selector': UmbDocumentWorkspaceSplitViewVariantSelectorElement;
	}
}
