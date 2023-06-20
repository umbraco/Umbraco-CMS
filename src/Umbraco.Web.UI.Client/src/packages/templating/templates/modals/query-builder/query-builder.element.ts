import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { UMB_MODAL_MANAGER_CONTEXT_TOKEN, UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';

export interface TemplateQueryBuilderModalData {
	hidePartialViews?: boolean;
}

export interface TemplateQueryBuilderModalResult {
	value: string;
}

@customElement('umb-templating-query-builder-modal')
export default class UmbChooseInsertTypeModalElement extends UmbModalBaseElement<
	TemplateQueryBuilderModalData,
	TemplateQueryBuilderModalResult
> {
	private _close() {
		this.modalContext?.reject();
	}

	private _modalContext?: UmbModalManagerContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	render() {
		return html`
			<umb-body-layout headline="Query builder">
				<div id="main">
					<uui-box>
						<div>
							I want <uui-button look="outline">all content</uui-button> from
							<uui-button look="outline">all pages </uui-button>
						</div>
						<div>
							where <uui-button look="outline"></uui-button> from <uui-button look="outline">all pages </uui-button>
						</div>
					</uui-box>
				</div>
				<div slot="actions">
					<uui-button @click=${this._close} look="secondary">Close</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				color: var(--uui-color-text);
				--umb-header-layout-height: 70px;
			}

			#main {
				box-sizing: border-box;
				height: calc(
					100dvh - var(--umb-header-layout-height) - var(--umb-footer-layout-height) - 2 * var(--uui-size-layout-1)
				);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-templating-query-builder-modal': UmbChooseInsertTypeModalElement;
	}
}
