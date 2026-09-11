import { UMB_TEMPLATE_WORKSPACE_CONTEXT } from './template-workspace.context-token.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import type { UmbInputWithAliasElement } from '@umbraco-cms/backoffice/components';
import { umbBindToValidation } from '@umbraco-cms/backoffice/validation';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';

/**
 * Template alias pattern - allows first character to be a letter, digit, or underscore.
 * Mirrors server-side CleanStringType.UnderscoreAlias behavior.
 */
const UMB_TEMPLATE_ALIAS_PATTERN = '^[A-Za-z0-9_][A-Za-z0-9_-]{0,254}$';

@customElement('umb-template-workspace-editor')
export class UmbTemplateWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _name?: string = '';

	@state()
	private _alias?: string = '';

	/**
	 * Whether editing is restricted. True when in production mode OR when runtime mode is still unknown.
	 * This ensures a safe default (restricted) until we confirm the runtime mode.
	 */
	@state()
	private _isRestricted = true;

	#templateWorkspaceContext?: typeof UMB_TEMPLATE_WORKSPACE_CONTEXT.TYPE;
	#isNew = false;

	constructor() {
		super();

		this.consumeContext(UMB_SERVER_CONTEXT, (context) => {
			this.observe(context?.isProductionMode, (isProductionMode) => {
				// Restricted until we confirm it's NOT production mode (safe default).
				this._isRestricted = isProductionMode !== false;
			});
		});

		this.consumeContext(UMB_TEMPLATE_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#templateWorkspaceContext = workspaceContext;
			this.observe(this.#templateWorkspaceContext?.name, (name) => {
				this._name = name;
			});

			this.observe(this.#templateWorkspaceContext?.alias, (alias) => {
				this._alias = alias;
			});

			this.observe(this.#templateWorkspaceContext?.isNew, (isNew) => {
				this.#isNew = !!isNew;
			});
		});
	}

	#onNameAndAliasChange(event: InputEvent & { target: UmbInputWithAliasElement }) {
		this.#templateWorkspaceContext?.setName(event.target.value ?? '');
		this.#templateWorkspaceContext?.setAlias(event.target.alias ?? '');
	}

	override render() {
		return html`
			<umb-entity-detail-workspace-editor>
				<umb-input-with-alias
					slot="header"
					id="name"
					label=${this.localize.term('placeholders_entername')}
					placeholder=${this.localize.term('placeholders_entername')}
					.value=${this._name}
					.alias=${this._alias}
					alias-pattern=${UMB_TEMPLATE_ALIAS_PATTERN}
					?auto-generate-alias=${this.#isNew}
					?readonly=${this._isRestricted}
					@change=${this.#onNameAndAliasChange}
					required
					${umbBindToValidation(this)}
					${umbFocus()}>
				</umb-input-with-alias>
			</umb-entity-detail-workspace-editor>
		`;
	}

	static override styles = [
		css`
			umb-input-with-alias {
				width: 100%;
			}
		`,
	];
}

export default UmbTemplateWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-template-workspace-editor': UmbTemplateWorkspaceEditorElement;
	}
}
