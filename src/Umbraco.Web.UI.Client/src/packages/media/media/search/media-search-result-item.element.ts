import type { UmbMediaItemModel } from '../repository/types.js';
import { classMap, css, customElement, html, nothing, property, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbSearchResultItemModel } from '@umbraco-cms/backoffice/search';

@customElement('umb-media-search-result-item')
export class UmbMediaSearchResultItemElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbSearchResultItemModel & UmbMediaItemModel;

	override render() {
		if (!this.item) return nothing;
		const classes = { trashed: this.item.isTrashed };
		return html`
			${when(
				this.item.mediaType.icon ?? this.item.icon,
				(icon) => html`<umb-icon name=${icon}></umb-icon>`,
				() => html`<uui-icon name="icon-picture"></uui-icon>`,
			)}
			<span class=${classMap(classes)}>${this.item.name}</span>
			<div class="extra">
				${when(
					this.item.isTrashed,
					() => html`
						<uui-tag look="secondary">
							<umb-localize key="mediaPicker_trashed">Trashed</umb-localize>
						</uui-tag>
					`,
				)}
			</div>
		`;
	}

	static override styles = [
		css`
			:host {
				border-radius: var(--uui-border-radius);
				outline-offset: -3px;
				padding: var(--uui-size-space-3) var(--uui-size-space-5);

				display: flex;
				gap: var(--uui-size-space-3);
				align-items: center;

				width: 100%;

				> span {
					flex: 1;

					&.trashed {
						text-decoration: line-through;
						opacity: 0.6;
					}
				}
			}
		`,
	];
}

export { UmbMediaSearchResultItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-search-result-item': UmbMediaSearchResultItemElement;
	}
}
