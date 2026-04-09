import { UmbNewsDashboardRepository } from './repository/index.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { NewsDashboardItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

import './components/umb-news-container.element.js';

@customElement('umb-umbraco-news-dashboard')
export class UmbUmbracoNewsDashboardElement extends UmbLitElement {
	@state()
	private _items: Array<NewsDashboardItemResponseModel> = [];

	@state()
	private _loaded: boolean = false;

	#repo = new UmbNewsDashboardRepository(this);

	override async firstUpdated() {
		const res = await this.#repo.getNewsDashboard();
		this._items = res.data?.items ?? [];
		this._loaded = true;
	}

	override render() {
		if (!this._loaded) {
			return html`<div class="loader"><uui-loader></uui-loader></div>`;
		}

		if (this._items.length === 0) {
			return this.#renderDefaultContent();
		}

		return html`<umb-news-container .items=${this._items}></umb-news-container>`;
	}

	#renderDefaultContent() {
		const infoLinks = [
			{
				name: this.localize.term('welcomeDashboard_documentationHeadline'),
				description: this.localize.term('welcomeDashboard_documentationDescription'),
				href: 'https://docs.umbraco.com/?utm_source=core&utm_medium=dashboard&utm_campaign=docs',
			},
			{
				name: this.localize.term('welcomeDashboard_umbracoCommunityHeadline'),
				description: this.localize.term('welcomeDashboard_umbracoCommunityDescription'),
				href: 'https://community.umbraco.com/',
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
			<uui-box id="umbraco-forum">
				<div>
					<h2 class="uui-h3">${this.localize.term('welcomeDashboard_umbracoForumHeadline')}</h2>
					<p>${this.localize.term('welcomeDashboard_umbracoForumDescription')}</p>
					<uui-button
						look="outline"
						target="_blank"
						href="https://forum.umbraco.com/"
						label=${this.localize.term('welcomeDashboard_umbracoForumButton')}></uui-button>
				</div>
			</uui-box>
			${infoLinks.map(
				(link) => html`
					<a class="info-link" target="_blank" href=${link.href}>
						<h4>${link.name}</h4>
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

			#umbraco-forum {
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
		`,
	];
}

export default UmbUmbracoNewsDashboardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-umbraco-news-dashboard': UmbUmbracoNewsDashboardElement;
	}
}
