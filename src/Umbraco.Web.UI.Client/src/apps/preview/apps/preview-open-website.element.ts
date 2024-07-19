import { UMB_PREVIEW_CONTEXT } from '../preview.context.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-preview-open-website';
@customElement(elementName)
export class UmbPreviewOpenWebsiteElement extends UmbLitElement {
	async #onClick() {
		const previewContext = await this.getContext(UMB_PREVIEW_CONTEXT);
		previewContext.openWebsite();
	}

	override render() {
		return html`
			<uui-button look="primary" @click=${this.#onClick}>
				<div>
					<uui-icon name="icon-out"></uui-icon>
					<span>${this.localize.term('preview_openWebsiteLabel')}</span>
				</div>
			</uui-button>
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
		`,
	];
}

export { UmbPreviewOpenWebsiteElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbPreviewOpenWebsiteElement;
	}
}
