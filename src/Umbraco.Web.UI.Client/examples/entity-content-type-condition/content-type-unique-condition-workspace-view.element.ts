import { UMB_CONTENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';
import { html, customElement, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('example-content-type-unique-condition-workspace-view')
export class ExampleContentTypeUniqueConditionWorkspaceViewElement extends UmbLitElement {
	@state()
	private _contentTypeUniques: string[] = [];

	constructor() {
		super();

		this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, (context) => {
			this.observe(context?.structure.contentTypeUniques, (contentTypeUniques) => {
				this._contentTypeUniques = contentTypeUniques || [];
			});
		});
	}

	override render() {
		return html`<uui-box>
			<h3>Content Type Unique Condition Example</h3>
			<p>
				Content Type ${this._contentTypeUniques.length > 1 ? 'ids' : 'id'}:
				<strong>${this._contentTypeUniques}</strong>
			</p>
		</uui-box>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-2);
			}
		`,
	];
}

export { ExampleContentTypeUniqueConditionWorkspaceViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-content-type-unique-condition-workspace-view': ExampleContentTypeUniqueConditionWorkspaceViewElement;
	}
}
