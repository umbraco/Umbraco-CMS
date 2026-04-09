import { css, customElement, html, nothing, property, unsafeHTML, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { NewsDashboardItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-news-card')
export class UmbNewsCardElement extends UmbLitElement {
	@property({ type: Object })
	item!: NewsDashboardItemResponseModel;

	@property({ type: Number })
	priority: number = 3;

	#renderHeading(priority: number, text: string) {
		if (priority <= 2) return html`<h2 class="card-title uui-h3">${text}</h2>`;
		return html`<h3 class="card-title uui-h4">${text}</h3>`;
	}

	override render() {
		if (!this.item) return nothing;

		const isLastRow = this.priority === 3;

		const showImage = this.priority <= 2 && !!this.item.imageUrl;

		const content = html`
			${when(
				showImage,
				() =>
					this.item.imageUrl
						? html`<img class="card-img" src=${this.item.imageUrl} alt=${this.item.imageAltText ?? ''} />`
						: html`<div class="card-img placeholder" aria-hidden="true"></div>`,
				() => nothing,
			)}
			<div class="card-body uui-text">
				${this.#renderHeading(this.priority, this.item.header)}
				${this.item.body ? html`<div class="card-text">${unsafeHTML(this.item.body)}</div>` : nothing}
				${!isLastRow && this.item.url
					? html`<div class="card-actions">
							<uui-button
								look="primary"
								href=${this.item.url}
								target="_blank"
								rel="noopener"
								label=${this.item.buttonText || 'Open'}></uui-button>
						</div>`
					: nothing}
			</div>
		`;

		// Last row: whole card is a link
		return isLastRow
			? this.item.url
				? html`
						<a class="card normal-priority" role="listitem" href=${this.item.url} target="_blank" rel="noopener">
							${content}
						</a>
					`
				: html` <article class="card normal-priority" role="listitem">${content}</article> `
			: html` <article class="card" role="listitem">${content}</article> `;
	}

	static override styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}

			.card {
				background: var(--uui-color-surface);
				/* TODO: update with variables when UUI v.2 is available */
				border-radius: 16px;
				overflow: hidden;
				display: flex;
				flex-direction: column;
				height: 100%;
			}

			.card-img {
				width: 100%;
				object-fit: cover;
				display: block;
			}

			.card-img.placeholder {
				height: 8px;
			}

			/* TODO: update with uui-styles when UUI v.2 is available */
			.card-body.uui-text h2,
			.card-body.uui-text h3 {
				font-weight: 400;
				line-height: 35px;
			}

			.card-body.uui-text h2 {
				margin-top: -0.2em;
				margin-bottom: 0;
			}

			.card-body.uui-text h3 {
				margin-top: -0.5em;
				margin-bottom: var(--uui-size-space-3);
			}

			.card-body {
				display: flex;
				flex-direction: column;
				padding: var(--uui-size-layout-2);
				flex: 1 1 auto;
				justify-content: space-between;
				gap: var(--uui-size-space-4);
				max-width: 60em;
			}

			.card-title {
				margin: 0;
			}

			.card-text {
				justify-self: start;
			}
			.card-text > p {
				margin: 0;
			}

			.normal-priority {
				display: block;
				text-decoration: none;
				color: inherit;
				overflow: hidden;

				.card-body {
					gap: 0;
				}
			}
			.normal-priority:hover {
				color: var(--uui-color-interactive-emphasis);
			}
			.card-actions {
				margin-top: auto;
				padding-top: var(--uui-size-3);
				align-self: start;
				justify-self: end;
			}
			/* TODO: update accordingly to uui-button when UUI v.2 is available */
			.card-actions uui-button {
				--uui-button-background-color: #283a97;
				--uui-button-border-radius: 16px;
			}
		`,
	];
}

export default UmbNewsCardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-news-card': UmbNewsCardElement;
	}
}
