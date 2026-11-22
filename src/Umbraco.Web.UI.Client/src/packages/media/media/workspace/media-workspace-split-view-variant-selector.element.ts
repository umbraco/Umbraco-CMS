import type { UmbMediaVariantOptionModel } from '../types.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbWorkspaceSplitViewVariantSelectorElement } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-media-workspace-split-view-variant-selector')
export class UmbMediaWorkspaceSplitViewVariantSelectorElement extends UmbWorkspaceSplitViewVariantSelectorElement<UmbMediaVariantOptionModel> {}

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-workspace-split-view-variant-selector': UmbMediaWorkspaceSplitViewVariantSelectorElement;
	}
}
