import { UMB_MEDIA_PLACEHOLDER_ENTITY_TYPE } from '../../entity.js';
import type { UmbMediaCollectionItemModel } from './types.js';
import { css, customElement, html, ifDefined, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import type { UmbEntityCollectionItemElement } from '@umbraco-cms/backoffice/collection';
import { UmbFileDropzoneItemStatus } from '@umbraco-cms/backoffice/dropzone';

import '@umbraco-cms/backoffice/imaging';

@customElement('umb-media-collection-item-card')
export class UmbMediaCollectionItemCardElement extends UmbLitElement implements UmbEntityCollectionItemElement {
	#item?: UmbMediaCollectionItemModel | undefined;

	@property({ type: Object })
	public get item(): UmbMediaCollectionItemModel | undefined {
		return this.#item;
	}
	public set item(value: UmbMediaCollectionItemModel | undefined) {
		this.#item = value;
	}

	@property({ type: Boolean })
	selectable = false;

	@property({ type: Boolean })
	selected = false;

	@property({ type: Boolean })
	selectOnly = false;

	@property({ type: Boolean })
	disabled = false;

	@property({ type: String })
	href?: string;

	#onSelected(event: CustomEvent) {
		if (!this.item) return;
		event.stopPropagation();
		this.dispatchEvent(new UmbSelectedEvent(this.item.unique));
	}

	#onDeselected(event: CustomEvent) {
		if (!this.item) return;
		event.stopPropagation();
		this.dispatchEvent(new UmbDeselectedEvent(this.item.unique));
	}

	override render() {
		if (!this.item) return nothing;

		if (this.item.entityType === UMB_MEDIA_PLACEHOLDER_ENTITY_TYPE) {
			return this.#renderPlaceholder(this.item);
		}
		return html`
			<uui-card-media
				name=${ifDefined(this.item.name)}
				href=${ifDefined(this.href)}
				data-mark="${this.item.entityType}:${this.item.unique}"
				?selectable=${this.selectable}
				?select-only=${this.selectOnly}
				?selected=${this.selected}
				?disabled=${this.disabled}
				@selected=${this.#onSelected}
				@deselected=${this.#onDeselected}>
				<umb-imaging-thumbnail
					.unique=${this.item.unique}
					alt=${ifDefined(this.item.name)}
					icon=${ifDefined(this.item.icon)}></umb-imaging-thumbnail>
				<slot name="actions" slot="actions"></slot>
			</uui-card-media>
		`;
	}

	#renderPlaceholder(item: UmbMediaCollectionItemModel) {
		const complete = item.status === UmbFileDropzoneItemStatus.COMPLETE;
		const error = item.status !== UmbFileDropzoneItemStatus.WAITING && !complete;
		return html`<uui-card-media disabled class="media-placeholder-item" name=${ifDefined(item.name)}>
			<umb-temporary-file-badge
				.progress=${item.progress ?? 0}
				?complete=${complete}
				?error=${error}></umb-temporary-file-badge>
		</uui-card-media>`;
	}

	static override styles = [
		css`
			uui-card-media {
				height: 100%;
			}

			slot[name='actions'] {
				--uui-button-background-color: var(--uui-color-surface);
				--uui-button-background-color-hover: var(--uui-color-surface);
			}
		`,
	];
}

export { UmbMediaCollectionItemCardElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-collection-item-card': UmbMediaCollectionItemCardElement;
	}
}
