import type { UmbContentTreeItemModel } from '../../types.js';
import { customElement, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbSortChildrenOfModalElement } from '@umbraco-cms/backoffice/tree';

@customElement('umb-sort-children-of-content-modal')
export class UmbSortChildrenOfContentModalElement extends UmbSortChildrenOfModalElement {
	#localizeDateOptions: Intl.DateTimeFormatOptions = {
		day: 'numeric',
		month: 'short',
		year: 'numeric',
		hour: 'numeric',
		minute: '2-digit',
	};

	static override styles = [UmbTextStyles, css``];
}

export { UmbSortChildrenOfContentModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		['umb-sort-children-of-content-modal']: UmbSortChildrenOfContentModalElement;
	}
}
