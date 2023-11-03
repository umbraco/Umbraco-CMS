import { PropertyValueMap, css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-webhooks-workspace')
export class UmbWebhooksWorkspaceElement extends UmbLitElement {

	firstUpdated(props: PropertyValueMap<unknown>) {
		super.firstUpdated(props);

	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
	}

	load(): void {
		// Not relevant for this workspace -added to prevent the error from popping up
	}

	create(): void {
		// Not relevant for this workspace
	}

	render() {
		return html`
			<umb-workspace-editor alias="Umb.Workspace.Webhooks" headline="Webhooks" .enforceNoFooter=${true}>
			</umb-workspace-editor>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			uui-tab-group {
				--uui-tab-divider: var(--uui-color-border);
				border-left: 1px solid var(--uui-color-border);
				border-right: 1px solid var(--uui-color-border);
			}
		`,
	];
}

export default UmbWebhooksWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhooks-workspace': UmbWebhooksWorkspaceElement;
	}
}
