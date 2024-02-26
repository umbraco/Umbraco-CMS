import type { UmbTreeItemModelBase } from '@umbraco-cms/backoffice/tree';

export interface UmbTreeItemElement extends HTMLElement {
	item?: UmbTreeItemModelBase;
}
