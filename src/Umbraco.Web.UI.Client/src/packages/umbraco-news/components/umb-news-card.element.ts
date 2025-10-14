import { css, customElement, html, nothing, property, unsafeHTML, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { NewsDashboardItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('umb-news-card')
export class UmbNewsCardElement extends UmbLitElement {
	@property({ type: Object })
	item!: NewsDashboardItemResponseModel;

	@property({ type: Number })
	priority: number = 3;

	#renderHeading(priority: number, text: string) {
		if (priority <= 2) return html`<h2 class="card-title">${text}</h2>`;
		return html`<h3 class="card-title">${text}</h3>`;
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
			<div class="card-body">
				${this.#renderHeading(this.priority, this.item.header)}
				${this.item.body ? html`<div class="card-text">${unsafeHTML(this.item.body)}</div>` : nothing}
				${!isLastRow && this.item.url
					? html`<div class="card-actions">
							<uui-button look="outline" href=${this.item.url} target="_blank" rel="noopener">
								${this.item.buttonText || 'Open'}
							</uui-button>
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

	static override styles = css`
		:host {
			display: block;
			height: 100%;
		}

		.card {
			background: var(--uui-color-surface);
			border-radius: var(--uui-border-radius, 8px);
			box-shadow: var(
				--uui-box-box-shadow,
				var(--uui-shadow-depth-1, 0 1px 3px rgba(0, 0, 0, 0.12), 0 1px 2px rgba(0, 0, 0, 0.24))
			);
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

		.card-body {
			display: flex;
			flex-direction: column;
			padding: var(--uui-size-space-5);
			flex: 1 1 auto;
			justify-content: space-between;
			gap: var(--uui-size-space-3, 9px);
		}

		.card-title {
			margin: 0;
		}

		.card-text > p {
			margin: 0;
		}

		.normal-priority {
			display: block;
			border: 1px solid var(--uui-color-divider);
			border-radius: var(--uui-border-radius, 8px);
			text-decoration: none;
			color: inherit;
			overflow: hidden;

			.card-body {
				gap: 0;
			}
		}

		.card-actions {
			align-self: end;
		}
	`;
}

export default UmbNewsCardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-news-card': UmbNewsCardElement;
	}
}
