import { UMB_PREVIEW_CONTEXT } from '../context/preview.context-token.js';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSegmentCollectionRepository } from '@umbraco-cms/backoffice/segment';
import type { UmbSegmentCollectionItemModel } from '@umbraco-cms/backoffice/segment';
import type { UmbPopoverToggleEvent } from './types.js';

@customElement('umb-preview-segment')
export class UmbPreviewSegmentElement extends UmbLitElement {
	#segmentRepository = new UmbSegmentCollectionRepository(this);

	@state()
	private _segment?: UmbSegmentCollectionItemModel;

	@state()
	private _segments: Array<UmbSegmentCollectionItemModel> = [];

	@state()
	private _popoverOpen = false;

	override connectedCallback() {
		super.connectedCallback();
		this.hidden = true;
		this.#loadSegments();
	}

	async #loadSegments() {
		const { data } = await this.#segmentRepository.requestCollection({ skip: 0, take: 100 });
		this._segments = data?.items ?? [];

		const searchParams = new URLSearchParams(window.location.search);
		const segment = searchParams.get('segment');

		if (segment && segment !== this._segment?.unique) {
			this._segment = this._segments.find((c) => c.unique === segment);
		}

		this.hidden = !this._segments.length;
	}

	async #onClick(segment?: UmbSegmentCollectionItemModel) {
		if (this._segment === segment) return;
		this._segment = segment;

		const previewContext = await this.getContext(UMB_PREVIEW_CONTEXT);
		previewContext?.updateIFrame({ segment: segment?.unique });
	}

	#onPopoverToggle(event: UmbPopoverToggleEvent) {
		this._popoverOpen = event.newState === 'open';
	}

	override render() {
		if (!this._segments.length) return nothing;
		return html`
			<uui-button look="primary" popovertarget="segments-popover">
				<div>
					<uui-icon name="icon-filter"></uui-icon>
					<span>${this._segment?.name ?? 'Segments'}</span>
				</div>
				<uui-symbol-expand slot="extra" id="expand-symbol" .open=${this._popoverOpen}></uui-symbol-expand>
			</uui-button>
			<uui-popover-container id="segments-popover" placement="top-end" @toggle=${this.#onPopoverToggle}>
				<umb-popover-layout>
					<uui-menu-item label="Default" ?active=${!this._segment} @click=${() => this.#onClick()}></uui-menu-item>
					${repeat(
						this._segments,
						(item) => item.unique,
						(item) => html`
							<uui-menu-item
								label=${item.name}
								?active=${item.unique === this._segment?.unique}
								@click=${() => this.#onClick(item)}>
							</uui-menu-item>
						`,
					)}
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	static override styles = [
		css`
			:host {
				display: flex;
				border-left: 1px solid var(--uui-color-header-contrast);
				--uui-button-font-weight: 400;
				--uui-button-padding-left-factor: 3;
				--uui-button-padding-right-factor: 3;
				--uui-menu-item-flat-structure: 1;
			}

			:host([hidden]) {
				display: none;
			}

			#expand-symbol {
				transform: rotate(-90deg);
				margin-left: var(--uui-size-space-3, 9px);

				&[open] {
					transform: rotate(0deg);
				}
			}

			uui-button > div {
				display: flex;
				align-items: center;
				gap: var(--uui-size-2, 6px);
			}

			umb-popover-layout {
				--uui-color-surface: var(--uui-color-header-surface);
				--uui-color-border: var(--uui-color-header-surface);
				color: var(--uui-color-header-contrast);
			}
		`,
	];
}

export { UmbPreviewSegmentElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-preview-segment': UmbPreviewSegmentElement;
	}
}
