import { UMB_PREVIEW_CONTEXT } from '../context/preview.context-token.js';
import type { UmbPopoverToggleEvent } from './types.js';
import { css, customElement, html, nothing, query, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import type { UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-preview-culture')
export class UmbPreviewCultureElement extends UmbLitElement {
	#languageRepository = new UmbLanguageCollectionRepository(this);

	@query('#cultures-popover')
	private _popoverElement?: UUIPopoverContainerElement;

	@state()
	private _culture?: UmbLanguageDetailModel;

	@state()
	private _cultures: Array<UmbLanguageDetailModel> = [];

	@state()
	private _popoverOpen = false;

	constructor() {
		super();
		this.addEventListener('blur', this.#onBlur, true); // Use capture phase to catch blur events
	}

	override connectedCallback() {
		super.connectedCallback();
		this.hidden = true;
		this.#loadCultures();
	}

	async #loadCultures() {
		const { data: langauges } = await this.#languageRepository.requestCollection({ skip: 0, take: 100 });
		this._cultures = langauges?.items ?? [];

		const searchParams = new URLSearchParams(window.location.search);
		const culture = searchParams.get('culture');

		if (culture && culture !== this._culture?.unique) {
			this._culture = this._cultures.find((c) => c.unique === culture);
		}

		this.hidden = !this._cultures.length;
	}

	async #onClick(culture: UmbLanguageDetailModel) {
		if (this._culture === culture) return;
		this._culture = culture;

		const previewContext = await this.getContext(UMB_PREVIEW_CONTEXT);
		previewContext?.updateIFrame({ culture: culture.unique });

		this._popoverElement?.hidePopover();
	}

	#onPopoverToggle(event: UmbPopoverToggleEvent) {
		this._popoverOpen = event.newState === 'open';
	}

	#onBlur = () => {
		if (this._popoverOpen) {
			this._popoverElement?.hidePopover();
		}
	};

	override render() {
		if (this._cultures.length <= 1) return nothing;
		return html`
			<uui-button look="primary" popovertarget="cultures-popover">
				<div>
					<uui-icon name="icon-globe"></uui-icon>
					<span>${this._culture?.name ?? this.localize.term('treeHeaders_languages')}</span>
				</div>
				<uui-symbol-expand slot="extra" id="expand-symbol" .open=${this._popoverOpen}></uui-symbol-expand>
			</uui-button>
			<uui-popover-container id="cultures-popover" placement="top-end" @toggle=${this.#onPopoverToggle}>
				<umb-popover-layout>
					${repeat(
						this._cultures,
						(item) => item.unique,
						(item) => html`
							<uui-menu-item
								label=${item.name}
								?active=${item.unique === this._culture?.unique}
								@click=${() => this.#onClick(item)}>
								<uui-icon slot="icon" name="icon-globe"></uui-icon>
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

export { UmbPreviewCultureElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-preview-culture': UmbPreviewCultureElement;
	}
}
