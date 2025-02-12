import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-umbraco-news-dashboard')
export class UmbUmbracoNewsDashboardElement extends UmbLitElement {
	#infoLinks = [
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

	override render() {
		return html`
			<div id="info-links" class="uui-text">
				<uui-box id="our-umbraco">
					<div>
						<h2 class="uui-h3">${this.localize.term('welcomeDashboard_ourUmbracoHeadline')}</h2>
						<p>${this.localize.term('welcomeDashboard_ourUmbracoDescription')}</p>
						<uui-button
							look="primary"
							target="_blank"
							href="https://our.umbraco.com/?utm_source=core&amp;utm_medium=dashboard&amp;utm_content=image&amp;utm_campaign=our"
							label=${this.localize.term('welcomeDashboard_ourUmbracoButton')}></uui-button>
					</div>
				</uui-box>
				${this.#infoLinks.map(
					(link) => html`
						<a class="info-link" target="_blank" href=${link.href}>
							<h3 class="uui-h5">${link.name}</h3>
							<p>${link.description}</p>
						</a>
					`,
				)}
			</div>
		`;
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
				max-width: 1000px;
				grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
				grid-gap: var(--uui-size-space-4);
			}
			.info-link {
				border: 1px solid var(--uui-color-border);
				padding: var(--uui-size-space-4);
				border-radius: calc(var(--uui-border-radius) * 2);
				line-height: 1.5;
				background-color: var(--uui-color-surface);
				text-decoration: none;
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
