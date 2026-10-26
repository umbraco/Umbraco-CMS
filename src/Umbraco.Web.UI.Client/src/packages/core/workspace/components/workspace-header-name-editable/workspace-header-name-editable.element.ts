import { UMB_NAMABLE_WORKSPACE_CONTEXT } from '../../namable/index.js';
import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '../../entity-detail/entity-detail-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, nothing, state, property } from '@umbraco-cms/backoffice/external/lit';
import { umbFocus, UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { umbBindToValidation } from '@umbraco-cms/backoffice/validation';

@customElement('umb-workspace-header-name-editable')
export class UmbWorkspaceHeaderNameEditableElement extends UmbLitElement {
	/**
	 * The label for the inner input.
	 * @attr
	 */
	@property()
	label?: string;

	/**
	 * The placeholder for the inner input.
	 * @attr
	 */
	@property()
	placeholder?: string;

	/**
	 * The readonly state of the inner input.
	 * @attr
	 */
	@property({ type: Boolean })
	readonly = false;

	@state()
	private _name = '';

	@state()
	private _isWritableName = true;

	@state()
	private _isReadOnly = false;

	#workspaceContext?: typeof UMB_NAMABLE_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_NAMABLE_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;
			this.#observeName();
			this.#observeNameWriteGuardRules();
		});

		this.consumeContext(UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context?.readOnly,
				(isReadOnly) => (this._isReadOnly = isReadOnly ?? false),
				'umbObserveWorkspaceReadOnly',
			);
		});
	}

	#observeNameWriteGuardRules() {
		this.observe(
			this.#workspaceContext?.nameWriteGuard?.isPermittedForName(),
			(isPermitted) => {
				this._isWritableName = isPermitted ?? true;
			},
			'umbObserveWorkspaceNameWriteGuardRules',
		);
	}

	#observeName() {
		if (!this.#workspaceContext) return;
		this.observe(
			this.#workspaceContext.name,
			(name) => {
				if (name !== this._name) {
					this._name = name ?? '';
				}
			},
			'observeWorkspaceName',
		);
	}

	#onNameInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.setName(target.value);
			}
		}
	}

	override render() {
		return html`<uui-input
			id="nameInput"
			data-mark="input:workspace-name"
			.value=${this._name}
			@input="${this.#onNameInput}"
			label=${this.label ?? this.localize.term('placeholders_entername')}
			placeholder=${this.placeholder ?? this.localize.term('placeholders_entername')}
			?readonly=${this.readonly || !this._isWritableName}
			required
			${umbBindToValidation(this, `$.name`, this._name)}
			${umbFocus()}>
			${this._isReadOnly
				? html`<uui-tag id="read-only-tag" slot="append" look="secondary">
						${this.localize.term('general_readOnly')}
					</uui-tag>`
				: nothing}
		</uui-input>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: contents;
			}

			#nameInput {
				flex: 1 1 auto;
			}

			#read-only-tag {
				align-self: center;
				margin: 0 var(--uui-size-space-2);
				white-space: nowrap;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-header-name-editable': UmbWorkspaceHeaderNameEditableElement;
	}
}
