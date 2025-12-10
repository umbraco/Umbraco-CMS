import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, LitElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/variant';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';

@customElement('example-block-workspace-view')
export class ExampleBlockWorkspaceViewElement extends UmbElementMixin(LitElement) implements UmbWorkspaceViewElement {
	@state()
	private _culture?: string | null;
	@state()
	private _segment?: string | null;

	constructor() {
		super();

		this.consumeContext(UMB_VARIANT_CONTEXT, (context) => {
			this.observe(context?.displayVariantId, (variantId) => {
				this._culture = variantId?.culture;
				this._segment = variantId?.segment;
			});
		});
	}

	override render() {
		return html`
			<uui-box class="uui-text">
				<p class="uui-lead">Current variant context culture: ${this._culture}, & segment of : ${this._segment}</p>
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}
		`,
	];
}

export default ExampleBlockWorkspaceViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'example-block-workspace-view': ExampleBlockWorkspaceViewElement;
	}
}
