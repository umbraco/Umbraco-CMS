import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '../entity-detail-workspace.context-token.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityDeletedEvent } from '@umbraco-cms/backoffice/entity-action';

@customElement('umb-entity-detail-workspace-editor')
export class UmbEntityDetailWorkspaceEditorElement extends UmbLitElement {
	@property({ attribute: 'back-path' })
	public backPath?: string;

	@state()
	private _entityType?: string;

	@state()
	private _isLoading = false;

	@state()
	private _isForbidden = false;

	@state()
	private _exists = false;

	@state()
	private _isNew? = false;

	#context?: typeof UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT.TYPE;
	#eventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;
	#unique?: string | null;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, (context) => {
			this.#context = context;
			this.observe(this.#context?.entityType, (entityType) => (this._entityType = entityType));
			this.observe(this.#context?.loading.isOn, (isLoading) => (this._isLoading = isLoading ?? false));
			this.observe(this.#context?.forbidden.isOn, (isForbidden) => (this._isForbidden = isForbidden ?? false));
			this.observe(this.#context?.data, (data) => (this._exists = !!data));
			this.observe(this.#context?.isNew, (isNew) => (this._isNew = isNew));
			this.observe(this.#context?.unique, (unique) => (this.#unique = unique));
		});

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (context) => {
			this.#eventContext?.removeEventListener(
				UmbEntityDeletedEvent.TYPE,
				this.#onEntityDeletedEvent as unknown as EventListener,
			);
			this.#eventContext = context;
			this.#eventContext?.addEventListener(
				UmbEntityDeletedEvent.TYPE,
				this.#onEntityDeletedEvent as unknown as EventListener,
			);
		});
	}

	#onEntityDeletedEvent = (event: UmbEntityDeletedEvent) => {
		// Ignore events if we don't have a unique identifier (e.g., new unsaved entity)
		if (!this.#unique) return;

		if (event.getEntityType() !== this._entityType) return;
		if (event.getUnique() !== this.#unique) return;

		// Navigate to the back path when the entity is deleted
		if (this.backPath) {
			history.pushState({}, '', this.backPath);
		}
	};

	#renderForbidden() {
		if (!this._isLoading && this._isForbidden) {
			return html`<umb-entity-detail-forbidden
				entity-type=${ifDefined(this._entityType)}></umb-entity-detail-forbidden>`;
		}
		return nothing;
	}

	#renderNotFound() {
		if (!this._isLoading && !this._exists) {
			return html`<umb-entity-detail-not-found
				entity-type=${ifDefined(this._entityType)}></umb-entity-detail-not-found>`;
		}
		return nothing;
	}

	protected override render() {
		return html` ${this.#renderForbidden()} ${this.#renderNotFound()}

			<!-- TODO: It is currently on purpose that the workspace editor is always in the DOM, even when it doesn't have data.
			 We currently rely on the entity actions to be available to execute, and we ran into an issue when the entity got deleted; then the DOM got cleared, and the delete action couldn't complete.
			 We need to look into loading the entity actions in the workspace context instead so we don't rely on the DOM.
		 -->
			<umb-workspace-editor
				?loading=${this._isLoading}
				.backPath=${this.backPath}
				class="${this._exists === false ? 'hide' : ''}">
				<slot name="header" slot="header"></slot>
				${this.#renderEntityActions()}
				<slot></slot>
			</umb-workspace-editor>`;
	}

	#renderEntityActions() {
		if (this._isNew) return nothing;
		return html`<umb-workspace-entity-action-menu
			slot="action-menu"
			data-mark="workspace:action-menu"></umb-workspace-entity-action-menu>`;
	}

	override destroy(): void {
		this.#eventContext?.removeEventListener(
			UmbEntityDeletedEvent.TYPE,
			this.#onEntityDeletedEvent as unknown as EventListener,
		);
		super.destroy();
	}

	static override styles = [
		css`
			umb-workspace-editor {
				visibility: visible;
			}

			umb-workspace-editor.hide {
				visibility: hidden;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-detail-workspace-editor': UmbEntityDetailWorkspaceEditorElement;
	}
}
