/* eslint-disable lit/attribute-value-entities */
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-umbraco-news-dashboard')
export class UmbUmbracoNewsDashboardElement extends UmbLitElement {
	#infoLinks = [
		{
			name: 'Documentation',
			description: 'Find the answers to all your Umbraco questions',
			href: 'https://docs.umbraco.com/?utm_source=core&utm_medium=dashboard&utm_campaign=docs',
		},
		{
			name: 'Community',
			description: 'Get support and inspiration from driven Umbraco experts',
			href: 'https://our.umbraco.com/?utm_source=core&utm_medium=dashboard&utm_content=text&utm_campaign=our_forum',
		},
		{
			name: 'Resources',
			description: 'Free video tutorials to jumpstart your journey with the CMS',
			href: 'https://umbraco.com/resources/?utm_source=core&utm_medium=dashboard&utm_content=text&utm_campaign=resources',
		},
		{
			name: 'Training',
			description: 'Real-life training and official Umbraco certifications',
			href: 'https://umbraco.com/training/?utm_source=core&utm_medium=dashboard&utm_content=text&utm_campaign=training',
		},
	];

	render() {
		return html`
			<div id="info-links" class="uui-text">
				<uui-box id="our-umbraco">
					<div>
						<h2 class="uui-h3">Our Umbraco - The Friendliest Community</h2>
						<p>
							Our Umbraco, the official community site, is your one-stop-shop for everything Umbraco. Whether you need a
							question answered, cool plugins, or a guide of how to do something in Umbraco, the world's best and
							friendliest community is just a click away.
						</p>
						<uui-button
							look="primary"
							href="https://our.umbraco.com/?utm_source=core&utm_medium=dashboard&utm_content=image&utm_campaign=our"
							label="Visit Our Umbraco"></uui-button>
					</div>
				</uui-box>
				${this.#infoLinks.map(
					(link) => html`
						<a class="info-link" href=${link.href}>
							<h3 class="uui-h5">${link.name}</h3>
							<p>${link.description}</p>
						</a>
					`,
				)}
			</div>
		`;
	}

	static styles = [
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
