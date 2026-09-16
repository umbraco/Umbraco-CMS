import { UMB_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT } from '../entity-named-detail-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { umbFocus, UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { umbBindToValidation } from '@umbraco-cms/backoffice/validation';
import type { UmbEntityStateEntry } from '@umbraco-cms/backoffice/entity-state';

/**
 * A name-input workspace header, like `<umb-workspace-header-name-editable>`, plus the entity's
 * `entityState` registry rendered as tags. For any workspace context extending
 * `UmbEntityNamedDetailWorkspaceContextBase`, which already provides both naming and `entityState`.
 * @element umb-entity-named-detail-workspace-header
 */
@customElement('umb-entity-named-detail-workspace-header')
export class UmbEntityNamedDetailWorkspaceHeaderElement extends UmbLitElement {
	/**
	 * The readonly state of the inner input.
	 * @attr
	 */
	readonly = false;

	@state()
	private _name = '';

	@state()
	private _isWritableName = true;

	@state()
	private _states: Array<UmbEntityStateEntry> = [];

	#workspaceContext?: typeof UMB_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;
			this.#observeName();
			this.#observeNameWriteGuardRules();
			this.#observeEntityStates();
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

	#observeEntityStates() {
		this.observe(
			this.#workspaceContext?.entityState.states,
			(states) => {
				this._states = states ?? [];
			},
			'umbObserveEntityStates',
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
			label=${this.localize.term('placeholders_entername')}
			placeholder=${this.localize.term('placeholders_entername')}
			?readonly=${this.readonly || !this._isWritableName}
			required
			${umbBindToValidation(this, `$.name`, this._name)}
			${umbFocus()}>
			<umb-entity-state-tags slot="append" .states=${this._states}></umb-entity-state-tags>
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

			umb-entity-state-tags {
				display: inline-block;
				margin-right: var(--uui-size-space-2);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-named-detail-workspace-header': UmbEntityNamedDetailWorkspaceHeaderElement;
	}
}
