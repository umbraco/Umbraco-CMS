import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '../entity-detail-workspace.context-token.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityDeletedEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

const umbBackPathDeprecation = new UmbDeprecation({
	deprecated: 'The `backPath` property on `<umb-entity-detail-workspace-editor>`.',
	removeInVersion: '20',
	solution: 'Implement `navigationParentItemPath` on the workspace context instead.',
});

@customElement('umb-entity-detail-workspace-editor')
export class UmbEntityDetailWorkspaceEditorElement extends UmbLitElement {
	#backPath?: string;

	/**
	 * A fallback "back to parent" path, used only when the workspace context has no `navigationParentItemPath` of
	 * its own. Always shows its own back button when set, independent of `showBackToParentButton`, which only
	 * governs the button for `navigationParentItemPath`.
	 * @deprecated Implement `navigationParentItemPath` on the workspace context instead. Will be removed in Umbraco 20.
	 * @returns {string | undefined} The fallback back-to-parent path.
	 */
	@property({ attribute: 'back-path' })
	public get backPath(): string | undefined {
		return this.#backPath;
	}
	public set backPath(value: string | undefined) {
		if (value === this.#backPath) return;
		if (value !== undefined) umbBackPathDeprecation.warn();
		this.#backPath = value;
	}

	/**
	 * Shows a "back to parent" button linking to the workspace context's own `navigationParentItemPath`. Left as an
	 * explicit opt-in since not every entity should surface this button (e.g. tree-based entities rely on the tree
	 * itself for navigation). Does not affect the deprecated `backPath`, which always shows its own button when set.
	 */
	@property({ type: Boolean, attribute: 'show-back-to-parent-button' })
	public showBackToParentButton = false;

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

	@state()
	private _navigationParentItemPath?: string;

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
			this.observe(
				this.#context?.navigationParentItemPath,
				(path) => (this._navigationParentItemPath = path),
				'umbObserveNavigationParentItemPath',
			);
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

		// A dedicated UmbDeleteEntityWorkspaceRedirectController (wired into every UmbEntityDetailWorkspaceContextBase)
		// already redirects using this same path — only fall back to the deprecated backPath when the workspace
		// context doesn't expose one.
		if (this._navigationParentItemPath) return;

		if (this.#backPath) {
			// The deleted entity's own URL is gone for good — replace it rather than push, so "back" doesn't land on a 404.
			window.history.replaceState(null, '', this.#backPath);
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
				.backPath=${(this.showBackToParentButton ? this._navigationParentItemPath : undefined) ?? this.#backPath}
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
