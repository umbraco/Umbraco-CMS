import { UMB_WEBHOOK_ROOT_WORKSPACE_PATH } from '../../constants.js';
import { UMB_WEBHOOK_WORKSPACE_CONTEXT } from './webhook-workspace.context-token.js';
import { html, customElement, state, css } from '@umbraco-cms/backoffice/external/lit';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { umbFocus, UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-webhook-workspace-editor')
export class UmbWebhookWorkspaceEditorElement extends UmbLitElement {
	#workspaceContext?: typeof UMB_WEBHOOK_WORKSPACE_CONTEXT.TYPE;

	@state()
	_url: string = '';

	@state()
	_name = '';

	@state()
	_description = '';

	constructor() {
		super();

		this.consumeContext(UMB_WEBHOOK_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.observe(this.#workspaceContext?.url, (url) => (this._url = url ?? ''));
			this.observe(this.#workspaceContext?.name, (name) => (this._name = name ?? ''));
			this.observe(this.#workspaceContext?.description, (description) => (this._description = description ?? ''));
		});
	}

	#onNameChange(event: InputEvent & { target: UUIInputElement }) {
		const value = event.target.value.toString();
		this.#workspaceContext?.setName(value);
	}

	#onDescriptionChange(event: InputEvent & { target: UUIInputElement }) {
		const value = event.target.value.toString();
		this.#workspaceContext?.setDescription(value);
	}

	override render() {
		return html`
			<umb-entity-detail-workspace-editor back-path=${UMB_WEBHOOK_ROOT_WORKSPACE_PATH}>
				<div id="header" slot="header">
					<div id="editors">
						<uui-input
							id="name"
							label=${this.localize.term('placeholders_entername')}
							placeholder=${this.localize.term('placeholders_entername')}
							.value=${this._name}
							@change=${this.#onNameChange}
							${umbFocus()}>
						</uui-input>

						<uui-input
							id="description"
							.label=${this.localize.term('placeholders_enterDescription')}
							.value=${this._description}
							.placeholder=${this.localize.term('placeholders_enterDescription')}
							@input=${this.#onDescriptionChange}></uui-input>
					</div>
				</div>
			</umb-entity-detail-workspace-editor>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#header {
				width: 100%;
			}

			#name {
				width: 100%;
				z-index: 1;
			}

			#description {
				width: 100%;
				margin-top: -1px;
				--uui-input-height: var(--uui-size-8);
				--uui-input-border-color: transparent;
			}

			#description:hover {
				--uui-input-border-color: var(--uui-color-border);
			}
		`,
	];
}

export default UmbWebhookWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-workspace-editor': UmbWebhookWorkspaceEditorElement;
	}
}
