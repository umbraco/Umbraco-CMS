import { UmbNewsDashboardRepository } from './repository/index.js';
import {
	css,
	customElement,
	html,
	nothing,
	repeat,
	state,
	unsafeHTML,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import { sanitizeHTML } from '@umbraco-cms/backoffice/utils';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { NewsDashboardItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

import './components/umb-news-card.element.js';

interface UmbNewsDashboardGroupedItems {
	priority: number;
	items: Array<NewsDashboardItemResponseModel>;
}

@customElement('umb-umbraco-news-dashboard')
export class UmbUmbracoNewsDashboardElement extends UmbLitElement {
	@state()
	private _items: Array<NewsDashboardItemResponseModel> = [];

	@state()
	private _groupedItems: Array<UmbNewsDashboardGroupedItems> = [];

	@state()
	private _loaded: boolean = false;

	#repo = new UmbNewsDashboardRepository(this);

	override async firstUpdated() {
		const res = await this.#repo.getNewsDashboard();
		this._items = res.data?.items ?? [];
		this._groupedItems = this.#groupItemsByPriority();
		this._loaded = true;
	}

	#groupItemsByPriority(): Array<UmbNewsDashboardGroupedItems> {
		const sanitizedItems = this._items.map((i) => ({
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
		if (!this._loaded) {
			return html`<div class="loader"><uui-loader></uui-loader></div>`;
		}

		if (this._items.length === 0) {
			return this.#renderDefaultContent();
		}

		return html`
			${repeat(
				this._groupedItems,
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

	#renderDefaultContent() {
		const infoLinks = [
			{
				name: this.localize.term('welcomeDashboard_documentationHeadline'),
				description: this.localize.term('welcomeDashboard_documentationDescription'),
				href: 'https://docs.umbraco.com/?utm_source=core&utm_medium=dashboard&utm_campaign=docs',
			},
			{
				name: this.localize.term('welcomeDashboard_communityHeadline'),
				description: this.localize.term('welcomeDashboard_communityDescription'),
				href: 'https://our.umbraco.com/?utm_source=core&utm_medium=dashboard&utm_content=text&utm_campaign=our_forum',
			},
			{
				name: this.localize.term('welcomeDashboard_resourcesHeadline'),
				description: this.localize.term('welcomeDashboard_resourcesDescription'),
				href: 'https://umbraco.com/resources/?utm_source=core&utm_medium=dashboard&utm_content=text&utm_campaign=resources',
			},
			{
				name: this.localize.term('welcomeDashboard_trainingHeadline'),
				description: this.localize.term('welcomeDashboard_trainingDescription'),
				href: 'https://umbraco.com/training/?utm_source=core&utm_medium=dashboard&utm_content=text&utm_campaign=training',
			},
		];
		return html`<div id="info-links" class="uui-text">
			<uui-box id="our-umbraco">
				<div>
					<h2 class="uui-h3">${this.localize.term('welcomeDashboard_ourUmbracoHeadline')}</h2>
					<p>${this.localize.term('welcomeDashboard_ourUmbracoDescription')}</p>
					<uui-button
						look="outline"
						target="_blank"
						href="https://our.umbraco.com/?utm_source=core&amp;utm_medium=dashboard&amp;utm_content=image&amp;utm_campaign=our"
						label=${this.localize.term('welcomeDashboard_ourUmbracoButton')}></uui-button>
				</div>
			</uui-box>
			${infoLinks.map(
				(link) => html`
					<a class="info-link" target="_blank" href=${link.href}>
						<h3 class="uui-h5">${link.name}</h3>
						<p>${link.description}</p>
					</a>
				`,
			)}
		</div>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
				max-width: 1500px;
				container-type: inline-size;
				container-name: dashboard;
			}

			.loader {
				display: flex;
				justify-content: center;
				align-items: center;
			}

			p {
				position: relative;
			}

			#our-umbraco {
				grid-column-start: 1;
				grid-column-end: -1;
				margin-bottom: var(--uui-size-space-4);
			}

			#info-links {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(20%, 1fr));
				grid-gap: var(--uui-size-space-5);
			}

			.info-link {
				border: var(--uui-box-border-width, 0) solid
					var(--uui-box-border-color, var(--uui-color-divider-standalone, #e9e9eb));
				border-radius: var(--uui-box-border-radius, var(--uui-border-radius, 3px));
				box-shadow: var(
					--uui-box-box-shadow,
					var(--uui-shadow-depth-1, 0 1px 3px rgba(0, 0, 0, 0.12), 0 1px 2px rgba(0, 0, 0, 0.24))
				);
				background-color: var(--uui-color-surface);
				text-decoration: none;
				line-height: 1.5;
				padding: var(--uui-size-space-4);
			}

			.info-link h3 {
				margin-top: 0;
				margin-bottom: var(--uui-size-space-1);
			}

			.info-link p {
				margin-top: 0;
				margin-bottom: 0;
			}

			/* Grid */
			.cards {
				--cols: 4;
				--gap: var(--uui-size-space-4);
				width: 100%;
				display: grid;
				grid-template-columns: repeat(
					auto-fit,
					minmax(calc((100% - (var(--cols) - 1) * var(--gap)) / var(--cols)), 1fr)
				);
				gap: var(--gap);
			}

			.cards + .cards {
				margin-top: var(--uui-size-space-5);
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
		`,
	];
}

export default UmbUmbracoNewsDashboardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-umbraco-news-dashboard': UmbUmbracoNewsDashboardElement;
	}
}
