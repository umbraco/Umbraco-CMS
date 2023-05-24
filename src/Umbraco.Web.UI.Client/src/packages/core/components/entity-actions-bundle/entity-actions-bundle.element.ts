import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { map } from 'rxjs';
import { UmbSectionSidebarContext, UMB_SECTION_SIDEBAR_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/section';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-entity-actions-bundle')
export class UmbEntityActionsBundleElement extends UmbLitElement {
	private _entityType?: string;
	@property({ type: String, attribute: 'entity-type' })
	public get entityType() {
		return this._entityType;
	}
	public set entityType(value: string | undefined) {
		const oldValue = this._entityType;
		if (oldValue === value) return;

		this._entityType = value;
		this.#observeEntityActions();
		this.requestUpdate('entityType', oldValue);
	}

	@property({ type: String })
	public headline?: string;

	@state()
	private _hasActions = false;

	#sectionSidebarContext?: UmbSectionSidebarContext;

	constructor() {
		super();

		this.consumeContext(UMB_SECTION_SIDEBAR_CONTEXT_TOKEN, (sectionContext) => {
			this.#sectionSidebarContext = sectionContext;
		});
	}

	#observeEntityActions() {
		this.observe(
			umbExtensionsRegistry
				.extensionsOfType('entityAction')
				.pipe(map((actions) => actions.filter((action) => action.conditions.entityTypes.includes(this.entityType!)))),
			(actions) => {
				this._hasActions = actions.length > 0;
				console.log('umb-entity-actions-bundle — observe', this._entityType, this._hasActions);
			},
			'observeEntityAction'
		);
	}

	private _openActions() {
		if (!this.entityType) throw new Error('Entity type is not defined');
		this.#sectionSidebarContext?.toggleContextMenu(this.entityType, undefined, this.headline);
	}

	render() {
		return html`
			${this._hasActions
				? html`
						<uui-action-bar slot="actions">
							<uui-button @click=${this._openActions} label="Open actions menu">
								<uui-symbol-more></uui-symbol-more>
							</uui-button>
						</uui-action-bar>
				  `
				: nothing}
		`;
	}

	static styles = [UUITextStyles, css``];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-actions-bundle': UmbEntityActionsBundleElement;
	}
}
