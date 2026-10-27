import { UMB_WEBHOOK_WORKSPACE_CONTEXT } from './webhook-workspace.context-token.js';
import { html, customElement, state, css, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { umbFocus, UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-webhook-workspace-editor')
export class UmbWebhookWorkspaceEditorElement extends UmbLitElement {
	#workspaceContext?: typeof UMB_WEBHOOK_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _url: string = '';

	@state()
	private _name = '';

	@state()
	private _description = '';

	@state()
	private _isReadOnly = false;

	constructor() {
		super();

		this.consumeContext(UMB_WEBHOOK_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.observe(this.#workspaceContext?.url, (url) => (this._url = url ?? ''));
			this.observe(this.#workspaceContext?.name, (name) => (this._name = name ?? ''));
			this.observe(this.#workspaceContext?.description, (description) => (this._description = description ?? ''));
		});

		this.consumeContext(UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, (context) => {
			this.observe(context?.readOnly, (isReadOnly) => (this._isReadOnly = isReadOnly ?? false), '_observeIsReadOnly');
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
			<umb-entity-detail-workspace-editor show-back-to-parent-button>
				<div id="header" slot="header">
					<div id="editors">
						<uui-input
							id="name"
							label=${this.localize.term('placeholders_entername')}
							placeholder=${this.localize.term('placeholders_entername')}
							.value=${this._name}
							?readonly=${this._isReadOnly}
							@change=${this.#onNameChange}
							${umbFocus()}>
							${this._isReadOnly
								? html`<uui-tag id="read-only-tag" slot="append" look="secondary">
										${this.localize.term('general_readOnly')}
									</uui-tag>`
								: nothing}
						</uui-input>

						<uui-input
							id="description"
							.label=${this.localize.term('placeholders_enterDescription')}
							.value=${this._description}
							.placeholder=${this.localize.term('placeholders_enterDescription')}
							?readonly=${this._isReadOnly}
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

			#read-only-tag {
				align-self: center;
				margin: 0 var(--uui-size-space-2);
				white-space: nowrap;
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
