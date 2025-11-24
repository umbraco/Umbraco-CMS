import { UMB_CONTENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';
import { html, customElement, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-example-entity-content-type-unique-condition')
export class UmbWorkspaceExampleViewUniqueElement extends UmbLitElement {
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
			<h3>Content Type Unique Condition Test</h3>
			<p>It appears only on documents with GUID: <strong>${this._contentTypeUniques}</strong></p>
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

export { UmbWorkspaceExampleViewUniqueElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-example-entity-content-type-condition-unique': UmbWorkspaceExampleViewUniqueElement;
	}
}
