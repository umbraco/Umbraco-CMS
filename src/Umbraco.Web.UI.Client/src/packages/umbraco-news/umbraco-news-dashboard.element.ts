import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
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

					<umb-tree
						alias="Umb.Tree.DataType"
						.props=${{
							location: [
								null,
								'5f485f78-19f2-4841-89b3-2bb668f103eb',
								'950174a0-c84f-444b-bd66-a25a0ee4c848',
								'20df0d05-186c-4ec5-bbe0-8df08abc257e',
							],
						}}></umb-tree>
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
				grid-template-columns: repeat(auto-fill, minmax(20%, 1fr));
				grid-gap: var(--uui-size-space-4);
				max-width: 1000px;
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
