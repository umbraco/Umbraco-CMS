import { UMB_CONTENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';
import { html, customElement, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('example-content-type-alias-condition-workspace-view')
export class ExampleContentTypeAliasConditionWorkspaceViewElement extends UmbLitElement {
	@state()
	private _contentTypeAliases: string[] = [];

	constructor() {
		super();

		this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, (context) => {
			this.observe(context?.structure.contentTypeAliases, (contentTypeAliases) => {
				this._contentTypeAliases = contentTypeAliases || [];
			});
		});
	}

	override render() {
		return html`<uui-box>
			<h3>Content Type Alias Condition Example</h3>
			<p>
				Content Type ${this._contentTypeAliases.length > 1 ? 'aliases' : 'alias'}:
				<strong>${this._contentTypeAliases}</strong>
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

export { ExampleContentTypeAliasConditionWorkspaceViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-content-type-alias-condition-workspace-view': ExampleContentTypeAliasConditionWorkspaceViewElement;
	}
}
