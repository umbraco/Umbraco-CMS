import { css, customElement, html, ifDefined, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-workspace-info-app-layout')
export class UmbWorkspaceInfoAppLayoutElement extends UmbLitElement {
	@property({ type: String })
	headline?: string;

	protected override render() {
		return html`
			<uui-box headline=${ifDefined(this.headline ? this.localize.string(this.headline) : undefined)}>
				<slot name="header-actions" slot="header-actions"></slot>
				<div id="container">
					<slot></slot>
				</div>
			</uui-box>
		`;
	}

	static override styles = [
		css`
			uui-box {
				--uui-box-default-padding: 0;
			}

			#container {
				padding-left: var(--uui-size-space-4)
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-info-app-layout': UmbWorkspaceInfoAppLayoutElement;
	}
}
