import { css, customElement, html, nothing, property, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { NewsDashboardItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

import './umb-news-card.element.js';
import { sanitizeHTML } from '@umbraco-cms/backoffice/utils';

@customElement('umb-news-container')
export class UmbNewsContainerElement extends UmbLitElement {
	@property({ type: Array })
	items: Array<NewsDashboardItemResponseModel> = [];

	#groupItemsByPriority(items: NewsDashboardItemResponseModel[]) {
		const sanitizedItems = items.map((i) => ({
			...i,
			body: i.body ? sanitizeHTML(i.body) : '',
		}));

		// Separate items by priority.
		const priority1 = sanitizedItems.filter((item) => item.priority === 'High');
		const priority2 = sanitizedItems.filter((item) => item.priority === 'Medium');
		const priority3 = sanitizedItems.filter((item) => item.priority === 'Normal');

		// Group 1: First 4 items from priority 1.
		const group1Items = priority1.slice(0, 4);
		const overflow1 = priority1.slice(4);

		// Group 2: Overflow from priority 1 + priority 2 items (max 4 total).
		const group2Items = [...overflow1, ...priority2].slice(0, 4);
		const overflow2Count = overflow1.length + priority2.length - 4;
		const overflow2 = overflow2Count > 0 ? [...overflow1, ...priority2].slice(4) : [];

		// Group 3: Overflow from groups 1 & 2 + priority 3 items.
		const group3Items = [...overflow2, ...priority3];

		return [
			{ priority: 1, items: group1Items },
			{ priority: 2, items: group2Items },
			{ priority: 3, items: group3Items },
		];
	}

	override render() {
		if (!this.items?.length) return nothing;

		const groups = this.#groupItemsByPriority(this.items);

		return html`
			${repeat(
				groups,
				(g) => g.priority,
				(g) => html`
					<div class="cards" role="list" aria-label=${`Priority ${g.priority}`}>
						${repeat(
							g.items,
							(i, idx) => i.url || i.header || idx,
							(i) => html`<umb-news-card .item=${i} .priority=${g.priority}></umb-news-card>`,
						)}
					</div>
				`,
			)}
		`;
	}

	static override styles = css`
		.cards {
			--cols: 4;
			--gap: var(--uui-size-space-5);
			width: 100%;
			display: grid;
			grid-template-columns: repeat(auto-fit, minmax(calc((100% - (var(--cols) - 1) * var(--gap)) / var(--cols)), 1fr));
			gap: var(--gap);
		}

		.cards + .cards {
			margin-top: var(--uui-size-space-5);
		}

		/* For when container-type is not been assigned, not so sure about it???*/
		@media (max-width: 1200px) {
			.cards {
				grid-template-columns: repeat(auto-fit, minmax(2, 1fr));
			}
		}
		@media (max-width: 700px) {
			.cards {
				grid-template-columns: 1fr;
			}
		}

		@container dashboard (max-width: 1200px) {
			.cards {
				grid-template-columns: repeat(auto-fit, minmax(2, 1fr));
			}
		}
		@container dashboard (max-width: 700px) {
			.cards {
				grid-template-columns: 1fr;
			}
		}
	`;
}

export default UmbNewsContainerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-news-container': UmbNewsContainerElement;
	}
}
