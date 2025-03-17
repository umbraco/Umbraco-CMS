import { UMB_PREVIEW_CONTEXT } from '../preview.context.js';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';

@customElement('umb-preview-culture')
export class UmbPreviewCultureElement extends UmbLitElement {
	#languageRepository = new UmbLanguageCollectionRepository(this);

	@state()
	private _culture?: UmbLanguageDetailModel;

	@state()
	private _cultures: Array<UmbLanguageDetailModel> = [];

	override connectedCallback() {
		super.connectedCallback();
		this.#getCultures();
	}

	async #getCultures() {
		const { data: langauges } = await this.#languageRepository.requestCollection({ skip: 0, take: 100 });
		this._cultures = langauges?.items ?? [];

		const searchParams = new URLSearchParams(window.location.search);
		const culture = searchParams.get('culture');

		if (culture && culture !== this._culture?.unique) {
			this._culture = this._cultures.find((c) => c.unique === culture);
		}
	}

	async #onClick(culture: UmbLanguageDetailModel) {
		if (this._culture === culture) return;
		this._culture = culture;

		const previewContext = await this.getContext(UMB_PREVIEW_CONTEXT);
		previewContext?.updateIFrame({ culture: culture.unique });
	}

	override render() {
		if (this._cultures.length <= 1) return nothing;
		return html`
			<uui-button look="primary" popovertarget="cultures-popover">
				<div>
					<uui-icon name="icon-globe"></uui-icon>
					<span>${this._culture?.name ?? this.localize.term('treeHeaders_languages')}</span>
				</div>
			</uui-button>
			<uui-popover-container id="cultures-popover" placement="top-end">
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
			}

			uui-button > div {
				display: flex;
				align-items: center;
				gap: 5px;
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
