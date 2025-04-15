import { html, customElement, css, state, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbDocumentTreeItemModel } from '@umbraco-cms/backoffice/document';
import type { UmbMediaTreeItemModel } from '@umbraco-cms/backoffice/media';
import { UmbSortChildrenOfModalElement } from '@umbraco-cms/backoffice/tree';
import type { UmbContentTreeItemModel } from '../../types';

@customElement('umb-sort-children-of-content-modal')
export class UmbSortChildrenOfContentModalElement extends UmbSortChildrenOfModalElement {
	#sortBy: string = '';
	#sortDirection: string = '';

	#localizeDateOptions: Intl.DateTimeFormatOptions = {
		day: 'numeric',
		month: 'short',
		year: 'numeric',
		hour: 'numeric',
		minute: '2-digit',
	};

	#onSortChildrenBy(key: string) {
		const oldValue = this._children;

		// If switching column, revert to ascending sort. Otherwise switch from whatever was previously selected.
		if (this.#sortBy !== key) {
			this.#sortDirection = 'asc';
		} else {
			this.#sortDirection = this.#sortDirection === 'asc' ? 'desc' : 'asc';
		}

		// Sort by the new column.
		this.#sortBy = key;
		this._children = [...this._children].sort((a, b) => {
			switch (key) {
				case 'name':
					return a.name.localeCompare(b.name);
				case 'createDate':
					return Date.parse(this.#getCreateDate(a)) - Date.parse(this.#getCreateDate(b));
				default:
					return 0;
			}
		});

		// Reverse the order if sorting descending.
		if (this.#sortDirection === 'desc') {
			this._children.reverse();
		}

		this._sortedUniques.clear();
		this._children.map((c) => c.unique).forEach((u) => this._sortedUniques.add(u));

		this.requestUpdate('_children', oldValue);
	}

	#getCreateDate(item: UmbContentTreeItemModel): string {
		return item.createDate;
	}

	static override styles = [UmbTextStyles, css``];
}

export { UmbSortChildrenOfContentModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		['umb-sort-children-of-content-modal']: UmbSortChildrenOfContentModalElement;
	}
}
