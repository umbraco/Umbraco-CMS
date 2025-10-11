import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, customElement, html, state, unsafeHTML, when, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbNewsDashboardRepository } from './repository/index.js';
import type { NewsDashboardItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

interface NewsDashboardGroupedItems {
	priority: number;
	items: NewsDashboardItemResponseModel[];
}

@customElement('umb-umbraco-news-dashboard')
export class UmbUmbracoNewsDashboardElement extends UmbLitElement {

	@state()
	private _items: Array<NewsDashboardItemResponseModel> = [];

	@state()
	private _groupedItems: Array<NewsDashboardGroupedItems> = [];

	@state()
	private _loaded: boolean = false;

	#repo = new UmbNewsDashboardRepository(this);

	override async firstUpdated() {
		const res = await this.#repo.getNewsDashboard();
		this._items = res.data.items ?? [];
		this._groupedItems = this.#groupItemsByPriority();
		this._loaded = true;
	}

	#groupItemsByPriority(): Array<NewsDashboardGroupedItems> {

		// Separate items by priority.
		const priority1 = this._items.filter(item => item.priority === "High");
		const priority2 = this._items.filter(item => item.priority === "Medium");
		const priority3 = this._items.filter(item => item.priority === "Normal");

		// Group 1: First 4 items from priority 1.
		const group1Items = priority1.slice(0, 4);
		const overflow1 = priority1.slice(4);

		// Group 2: Overflow from priority 1 + priority 2 items (max 4 total).
		const group2Items = [...overflow1, ...priority2].slice(0, 4);
		const overflow2Count = overflow1.length + priority2.length - 4;
		const overflow2 = overflow2Count > 0
			? [...overflow1, ...priority2].slice(4)
			: [];

		// Group 3: Overflow from groups 1 & 2 + priority 3 items.
		const group3Items = [...overflow2, ...priority3];

		return [
			{ priority: 1, items: group1Items },
			{ priority: 2, items: group2Items },
			{ priority: 3, items: group3Items }
		];
	}

	override render() {
		if (!this._loaded) {
			return;
		}

		if (this._items.length === 0) {
			return this.#renderDefaultContent();
		}

		// TODO:
		// - We are getting and rendering HTML here. Would be good to sanitize it to only allow basic formatting and links.
		// - Layout needs improvement:
		//   - Each row should span to fill the container.
		//   - Spacing between rows.
		//   - The last row should wrap if there are more than 4, so 4 to a row.
		// - Styling: general review of matching what we have in Umbraco 13 within the style of the new backoffice.
		return html`
			<uui-box headline="Welcome to Umbraco">
				<div slot="header">Why hello there 😃! This is the Umbraco dashboard. Here you will find various helpful and interesting information about Umbraco.</div>
				${this._groupedItems.map(
					(g) => html`
						<div class="cards">
							${g.items.map(
								(i) => html`
									<article class="card">
										${when(g.priority <= 2,
											() => html`${i.imageUrl
												? html`<img class="card-img" src=${i.imageUrl} alt=${i.imageAltText ?? ''} />`
												: html`<div class="card-img placeholder" aria-hidden="true"></div>`}`,
											() => nothing)}

										<div class="card-body">
											<h4 class="card-title">${i.header}</h4>
											${i.body ? html`<div class="card-text">${unsafeHTML(i.body)}</div>` : null}
											${i.url
												? html`<div class="card-actions">
														<uui-button look="outline" href=${i.url} target="_blank" rel="noopener">
															${i.buttonText || 'Open'}
														</uui-button>
													</div>`
												: null}
										</div>
									</article>
								`,
							)}
						</div>
					`
				)}
			</uui-box>
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

			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}

			/* Grid */
			.cards {
				display: grid;
				grid-template-columns: repeat(4, minmax(0, 1fr));
				gap: var(--uui-size-space-4);
			}
			@media (max-width: 1200px) {
				.cards {
					grid-template-columns: repeat(2, 1fr);
				}
			}
			@media (max-width: 700px) {
				.cards {
					grid-template-columns: 1fr;
				}
			}

			/* Card */
			.card {
				background: var(--uui-color-surface);
				border: 1px solid var(--uui-color-divider);
				border-radius: var(--uui-border-radius, 8px);
				box-shadow: var(--uui-shadow-depth-1);
				overflow: hidden;
				display: flex;
				flex-direction: column;
			}

			.card-img {
				width: 100%;
				height: 160px;
				object-fit: cover;
				display: block;
				background: var(--uui-color-surface-emphasis);
			}
			.card-img.placeholder {
				height: 8px;
			}

			.card-body {
				padding: var(--uui-size-space-4);
			}
			.card-title {
				margin: 0 0 var(--uui-size-space-2) 0;
			}
			.card-text {
				margin: 0 0 var(--uui-size-space-2) 0;
				color: var(--uui-color-text);
				opacity: 0.85;
			}
			.card-actions {
				margin-top: var(--uui-size-space-2);
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
