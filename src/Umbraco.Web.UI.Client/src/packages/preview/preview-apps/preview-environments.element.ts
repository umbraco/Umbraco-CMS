import { UmbPreviewRepository } from '../repository/preview.repository.js';
import { UMB_PREVIEW_CONTEXT } from '../context/preview.context-token.js';
import type { UmbPopoverToggleEvent } from './types.js';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { umbPeekError } from '@umbraco-cms/backoffice/notification';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

type UmbPreviewEnvironmentItem = {
	alias: string;
	label: string;
	icon: string;
	urlProviderAlias?: string;
};

@customElement('umb-preview-environments')
export class UmbPreviewEnvironmentsElement extends UmbLitElement {
	#fallbackIcon = 'icon-multiple-windows';

	@state()
	private _culture?: string;

	@state()
	private _items: Array<UmbPreviewEnvironmentItem> = [];

	@state()
	private _popoverOpen = false;

	@state()
	private _segment?: string;

	@state()
	private _unique?: string;

	#previewRepository = new UmbPreviewRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_PREVIEW_CONTEXT, (context) => {
			this.observe(context?.unique, (unique) => (this._unique = unique), '_observeUnique');
			this.observe(context?.culture, (culture) => (this._culture = culture), '_observeCulture');
			this.observe(context?.segment, (segment) => (this._segment = segment), '_observeSegment');
		});
	}

	override connectedCallback() {
		super.connectedCallback();
		this.hidden = true;
		this.#getPreviewOptions();
	}

	async #getPreviewOptions() {
		this.observe(
			umbExtensionsRegistry.byTypeAndFilter('workspaceActionMenuItem', (ext) => ext.kind === 'previewOption'),
			(manifests) => {
				this._items = manifests.map((manifest) => ({
					alias: manifest.alias,
					label: (manifest.meta as any).label || manifest.name,
					icon: (manifest.meta as any).icon || this.#fallbackIcon,
					urlProviderAlias: (manifest.meta as any).urlProviderAlias,
				}));

				this.hidden = !this._items.length;
			},
		);
	}

	async #onClick(item: UmbPreviewEnvironmentItem) {
		if (!this._unique) return;
		if (!item.urlProviderAlias) return;

		const previewUrlData = await this.#previewRepository.getPreviewUrl(
			this._unique,
			item.urlProviderAlias,
			this._culture,
			this._segment,
		);

		if (previewUrlData.url) {
			// Add cache-busting parameter to ensure the preview tab reloads with the new preview session
			const previewUrl = new URL(previewUrlData.url, window.document.baseURI);
			previewUrl.searchParams.set('rnd', Date.now().toString());
			const previewWindow = window.open(previewUrl.toString(), `umbpreview-${this._unique}`);
			previewWindow?.focus();
			return;
		}

		if (previewUrlData.message) {
			umbPeekError(this, {
				color: 'danger',
				headline: this.localize.term('general_preview'),
				message: previewUrlData.message,
			});
		}
	}

	#onPopoverToggle(event: UmbPopoverToggleEvent) {
		this._popoverOpen = event.newState === 'open';
	}

	override render() {
		if (!this._items.length) return nothing;
		return html`
			<uui-button look="primary" popovertarget="options-popover">
				<div>
					<uui-icon name=${this.#fallbackIcon}></uui-icon>
					<span>Preview environments</span>
				</div>
				<uui-symbol-expand slot="extra" id="expand-symbol" .open=${this._popoverOpen}></uui-symbol-expand>
			</uui-button>
			<uui-popover-container id="options-popover" placement="top-end" @toggle=${this.#onPopoverToggle}>
				<umb-popover-layout>
					${repeat(
						this._items,
						(item) => item.alias,
						(item) => html`
							<uui-menu-item label=${this.localize.string(item.label)} @click=${() => this.#onClick(item)}>
								<uui-icon slot="icon" name=${item.icon}></uui-icon>
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

export { UmbPreviewEnvironmentsElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-preview-environments': UmbPreviewEnvironmentsElement;
	}
}
